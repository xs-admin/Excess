using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Excess.Concurrent.Runtime;

namespace Excess.Server.Middleware
{
    using System.Threading;
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
        //the local app
        IConcurrentApp ConcurrentApp { get; }

        //instance management
        bool HasInstance(Guid id);
        void RegisterInstance(Guid id, IConcurrentObject @object);
        void RemoteInstance(Guid id, Action<DistributedAppMessage> send);
        void RegisterClass(Type type);
        IEnumerable<Guid> Instances();

        //distributed functions
        Func<IDistributedApp, Exception> Connect { set; }
        Action<DistributedAppMessage> Receive { get; }
        Action<DistributedAppMessage> Send { set; }
        Action<string, DistributedAppMessage> SendToClient { set; }

        //lifetime
        void Start();
        void Stop();
        void AwaitCompletion();
    }

    public class DistributedApp : IDistributedApp
    {
        IConcurrentApp _concurrent;
        string _iid;
        public DistributedApp(IConcurrentApp concurrent, string iid = "")
        {
            _concurrent = concurrent;
            _iid = iid;
        }

        public IConcurrentApp ConcurrentApp { get { return _concurrent; } }

        ConcurrentDictionary<Guid, IConcurrentObject> _objects = new ConcurrentDictionary<Guid, IConcurrentObject>();
        public bool HasInstance(Guid id)
        {
            return _objects.ContainsKey(id);
        }

        public void RegisterInstance(Guid id, IConcurrentObject @object)
        {
            ConcurrentApp.Spawn(@object);
            _objects[id] = @object;
        }

        ConcurrentDictionary<Guid, Action<DistributedAppMessage>> _remotes = new ConcurrentDictionary<Guid, Action<DistributedAppMessage>>();
        public void RemoteInstance(Guid id, Action<DistributedAppMessage> send)
        {
            _remotes[id] = send;
        }

        public IEnumerable<Guid> Instances()
        {
            return _objects
                .Select(kvp => kvp.Key)
                .ToArray();
        }

        public Func<IDistributedApp, Exception> Connect { private get; set; }
        public Action<DistributedAppMessage> Send { private get; set; }
        public Action<string, DistributedAppMessage> SendToClient { private get; set; }
        public Action<DistributedAppMessage> Receive { get { return incomingMessage; } }

        public void Start()
        {
            if (Connect != null)
            {
                var exception = Connect(this);
                if (exception != null)
                    throw exception;
            }

            ConcurrentApp.AddSpawnListener(onSpawn);
            ConcurrentApp.Start();
        }

        public void Stop()
        {
            ConcurrentApp.Stop();
        }

        public void AwaitCompletion()
        {
            ConcurrentApp.AwaitCompletion();
        }

        Dictionary<Type, Dictionary<string, MethodFunc>> _methods = new Dictionary<Type, Dictionary<string, MethodFunc>>();
        public void RegisterClass(Type type)
        {
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

            ConcurrentApp.RegisterClass(type);
        }

        public void RegisterClass<T>() where T : IConcurrentObject
        {
            RegisterClass(typeof(T));
        }

        ConcurrentDictionary<Guid, Action<DistributedAppMessage>> _requests = new ConcurrentDictionary<Guid, Action<DistributedAppMessage>>();
        private void incomingMessage(DistributedAppMessage msg)
        {
            try
            {
                var @object = null as IConcurrentObject;
                var @continue = null as Action<DistributedAppMessage>;
                var @remote = null as Action<DistributedAppMessage>;
                if (_objects.TryGetValue(msg.Id, out @object))
                {
                    var methods = null as Dictionary<string, MethodFunc>;
                    if (!_methods.TryGetValue(@object.GetType(), out methods))
                        throw new ArgumentException("type");

                    var method = null as MethodFunc;
                    if (!methods.TryGetValue(msg.Method, out method))
                        throw new ArgumentException("method");

                    var json = JObject.Parse(msg.Data);
                    if (json == null)
                        throw new ArgumentException("args");

                    ConcurrentApp.Schedule(@object,
                        () => method(@object, json,
                            response => sendResponse(msg, response),
                            ex => sendFailure(msg, ex)),
                        ex => sendFailure(msg, ex));
                }
                else if (_remotes.TryGetValue(msg.Id, out @remote))
                {
                    Debug.Assert(msg.RequestId == Guid.Empty);

                    msg.RequestId = Guid.NewGuid();
                    _requests[msg.RequestId] = __res => msg.Success(__res.Data); //td: failure

                    @remote(msg);
                }
                else if (_requests.TryRemove(msg.RequestId, out @continue))
                    @continue(msg);
                else if (msg.Id == Guid.Empty && msg.RequestId == Guid.Empty)
                    internalCommand(msg.Method, msg.Data);
                else
                    throw new ArgumentException("id");
            }
            catch (Exception ex)
            {
                sendFailure(msg, ex);
            }
        }

        private void internalCommand(string method, string data)
        {
            switch (method)
            {
                case "__instance":
                    var instanceInfo = JObject.Parse(data) as dynamic;
                    var id = Guid.Parse((string)instanceInfo.__ID);
                    var iid = (string)instanceInfo.__IID;

                    RemoteInstance(id, msg => SendToClient(iid, msg));
                    break;
            }

            throw new ArgumentException("method");
        }

        public class ConcurrentJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type type)
            {
                return type.BaseType?.Name == "ConcurrentObject";
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new InvalidOperationException("write only");
            }

            public override void WriteJson(JsonWriter writer, object obj, JsonSerializer serializer)
            {
                var id = (Guid)obj
                    .GetType()
                    .GetField("__ID")
                    .GetValue(obj);

                writer.WriteStartObject();
                    writer.WriteRaw($"__ID : \"{id}\"");
                writer.WriteEndObject();
            }
        }

        static ConcurrentJsonConverter ConcurrentConverter = new ConcurrentJsonConverter();

        private void sendResponse(DistributedAppMessage msg, object response)
        {
            if (msg.Success == null && msg.RequestId == Guid.Empty)
                return; // no one is waiting for this

            var jsonMessage = $"{{\"__res\": {JsonConvert.SerializeObject(response, ConcurrentConverter)}}}";
            if (msg.Success != null)
                msg.Success(jsonMessage);
            else
                outgoingMessage(Guid.Empty, string.Empty, jsonMessage, msg.RequestId);
        }

        private void sendFailure(DistributedAppMessage msg, string message)
        {
            sendFailure(msg, new InvalidOperationException(message));
        }

        private void sendFailure(DistributedAppMessage msg, Exception ex)
        {
            if (msg.Failure == null && msg.RequestId == Guid.Empty)
                return; // no one is waiting for this

            if (msg.Failure != null)
                msg.Failure(ex);
            else
            {
                var jsonMessage = $"{{\"__ex\": \"{ex.Message}\"}}";
                outgoingMessage(Guid.Empty, string.Empty, jsonMessage, msg.RequestId);
            }
        }

        private void outgoingMessage(Guid id, string method, string data, Guid requestId, Action<DistributedAppMessage> continuation = null)
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

        private void onSpawn(Guid id, IConcurrentObject @object)
        {
            _objects[id] = @object;

            if (Send != null)
                outgoingMessage(Guid.Empty, "__instance", $"{{\"__ID\" : \"{id}\", \"__IID\" : \"{_iid}\"}}", Guid.Empty);
        }
    }
}

