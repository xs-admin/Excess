using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using Excess.Concurrent.Runtime.Core;
using Newtonsoft.Json.Linq;

namespace Excess.Concurrent.Runtime
{
    using Newtonsoft.Json;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using FactoryMap = Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>;
    using InitiliazerFunc = Action<IDistributedApp>;
    using MethodFunc = Action<IConcurrentObject, JObject, Action<object>, Action<Exception>>;

    public class DistributedAppMessage
    {
        public Guid Id { get; set; }
        public string Method { get; set; }
        public string Data { get; set; }
        public Guid RequestId { get; set; }
        public Action<string> Success { get; set; }
        public Action<Exception> Failure { get; set; }
    }

    public interface IDistributedApp
    {
        IDistributedApp WithInitializer(Action<IDistributedApp> initializer);

        bool HasObject(Guid id);
        void RegisterClass(Type type);
        void RegisterClass<T>() where T : IConcurrentObject;
        void RegisterRemoteClass(Type type);
        void RegisterInstance(Guid id, IConcurrentObject @object);

        //distributed functions
        Func<IDistributedApp, Exception> Connect { set; }
        Action<DistributedAppMessage> Receive { get; }
        Action<DistributedAppMessage> Send { set; }
    }

    public class DistributedConcurrentApp : ThreadedConcurrentApp, IDistributedApp
    {
        public DistributedConcurrentApp(
            FactoryMap types = null, 
            int threadCount = 2, 
            bool blockUntilNextEvent = true) : base(types, threadCount, blockUntilNextEvent)
        {
        }

        ConcurrentDictionary<Guid, IConcurrentObject> _objects = new ConcurrentDictionary<Guid, IConcurrentObject>();

        List<InitiliazerFunc> _initializers = new List<InitiliazerFunc>();
        public IDistributedApp WithInitializer(Action<IDistributedApp> initializer)
        {
            _initializers.Add(initializer);
            return this;
        }

        public bool HasObject(Guid id)
        {
            return _objects.ContainsKey(id);
        }

        Dictionary<Type, Dictionary<string, MethodFunc>> _methods = new Dictionary<Type, Dictionary<string, MethodFunc>>();
        public void RegisterClass(Type type)
        {
            notWhileRunning();

            var publicMethods = type
                .GetMethods()
                .Where(method => isConcurrent(method));

            var methodFuncs = new Dictionary<string, MethodFunc>();
            foreach (var method in publicMethods)
            {
                var parameters = method
                    .GetParameters();

                var paramCount = parameters.Length - 3;
                var paramNames = parameters
                    .Take(paramCount)
                    .Select(param => param.Name)
                    .ToArray();

                var paramTypes = parameters
                    .Take(paramCount)
                    .Select(param => param.ParameterType)
                    .ToArray();

                methodFuncs[method.Name] = (@object, args, success, failure) =>
                {
                    var arguments = new object[paramCount + 3];
                    for (int i = 0; i < paramCount; i++)
                    {
                        var property = args.Property(paramNames[i]);

                        arguments[i] = property.Value.ToObject(paramTypes[i]);
                    }

                    arguments[paramCount] = default(CancellationToken);
                    arguments[paramCount + 1] = success;
                    arguments[paramCount + 2] = failure;

                    method.Invoke(@object, arguments);
                };
            }

            _methods[type] = methodFuncs;
            _types[type.Name] = (app, args) => (IConcurrentObject)Activator.CreateInstance(type, args);
        }

        public void RegisterClass<T>() where T : IConcurrentObject
        {
            RegisterClass(typeof(T));
        }

        public void RegisterRemoteClass(Type type)
        {
            notWhileRunning();
            _types[type.Name] = (app, args) => { throw new NotImplementedException(); };
        }

        public void RegisterInstance(Guid id, IConcurrentObject @object)
        {
            _objects[id] = @object;
        }

        bool _running = false;
        private void notWhileRunning()
        {
            if (_running)
                throw new InvalidProgramException("operation not permitted while the app is running");
        }

        public Func<IDistributedApp, Exception> Connect { private get; set; }
        public Action<DistributedAppMessage> Send { private get; set; }
        public Action<DistributedAppMessage> Receive { get { return incomingMessage; } }

        public override void Start()
        {
            Debug.Assert(Connect != null);
            try
            {
                var exception = Connect(this);
                if (exception != null)
                    throw exception;

                _running = true;
                base.Start();
            }
            catch
            {
                _running = false;
                throw;
            }
        }

        ConcurrentDictionary<Guid, Action> _requests = new ConcurrentDictionary<Guid, Action>();
        private void incomingMessage(DistributedAppMessage msg)
        {
            var @object = null as IConcurrentObject;
            var @continue = null as Action;
            if (_objects.TryGetValue(msg.Id, out @object))
            {
                var methods = null as Dictionary<string, MethodFunc>;
                var method = null as MethodFunc;
                if (!_methods.TryGetValue(@object.GetType(), out methods))
                    sendFailure(msg.RequestId, $"unknown: {@object.GetType().Name}");
                else if (!methods.TryGetValue(msg.Method, out method))
                    sendFailure(msg.RequestId, $"unknown: {msg.Method} on {@object.GetType().Name}");
                else
                {
                    var json = parseJson(msg);
                    if (json != null)
                    {
                        _queue.Enqueue(new Event
                        {
                            Who = @object,
                            What = () => method(@object, json, 
                                response => sendResponse(msg.RequestId, response),
                                ex => sendFailure(msg.RequestId, ex.Message)),
                            Failure = ex => sendFailure(msg.RequestId, ex.Message),
                        });
                    }
                }
            }
            else if (_requests.TryRemove(msg.RequestId, out @continue))
                @continue();
            else
                sendFailure(msg.RequestId, $"unknown: {(msg.Id != Guid.Empty? msg.Id : msg.RequestId)}");
        }

        private JObject parseJson(DistributedAppMessage msg)
        {
            try
            {
                return JObject.Parse(msg.Data);
            }
            catch(Exception ex)
            {
                sendFailure(msg.RequestId, ex.Message);
                return null;
            }
        }

        private void sendResponse(Guid requestId, object response)
        {
            var jsonMessage = $"{{\"__res\": \"{JsonConvert.SerializeObject(response)}\"}}";
            outgoingMessage(Guid.Empty, string.Empty, jsonMessage, requestId);
        }

        private void sendFailure(Guid requestId, string message)
        {
            if (requestId != Guid.Empty)
            {
                var jsonMessage = $"{{\"__ex\": \"message\"}}";
                outgoingMessage(Guid.Empty, string.Empty, jsonMessage, requestId);
            }
        }

        private void outgoingMessage(Guid id, string method, string data, Guid requestId, Action continuation = null)
        {
            if (continuation != null)
            {
                Debug.Assert(requestId == Guid.Empty);

                requestId = Guid.NewGuid();
                _requests[requestId] = continuation;
            }
            else
                Debug.Assert(id == Guid.Empty || requestId == Guid.Empty); //never both

            Send(new DistributedAppMessage
            {
                Id = id,
                Method = method,
                Data = data,
                RequestId = requestId
            });
        }

        private bool isConcurrent(MethodInfo method)
        {
            if (!method.IsPublic)
                return false;

            var parameters = method
                .GetParameters()
                .ToArray();

            var count = parameters.Length;
            return count > 2
                && parameters[count - 2].ParameterType == typeof(Action<object>)
                && parameters[count - 1].ParameterType == typeof(Action<Exception>);
        }
    }
}
