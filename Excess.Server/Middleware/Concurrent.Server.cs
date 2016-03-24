using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Threading;

using Newtonsoft.Json.Linq;
using Excess.Concurrent.Runtime;

namespace Middleware
{
    using System.Diagnostics;
    using MethodFunc = Action<ConcurrentObject, JObject, Action<JObject>>;

    public interface IConcurrentServer
    {
        IIdentityServer Identity { get; set; }
        IInstantiator Instantiator { get; set; }

        void Build(Action<IConcurrentServer, Node> builder);

        bool Has(Guid id);
        void RegisterClass(Type @class);
        void RegisterClass<T>() where T : ConcurrentObject;
        void RegisterInstance(Guid id, ConcurrentObject @object);
    }

    public class ConcurrentServer : IConcurrentServer
    {
        Node _node = new Node(2, afap : false); //td: !!! config

        IIdentityServer _identity;
        public ConcurrentServer(IIdentityServer identity = null)
        {
            _identity = identity;
        }

        public IIdentityServer Identity
        {
            get { return _identity; }
            set { _identity = value; }
        }

        public IInstantiator Instantiator { get; set; }

        public void StartListening()
        {
            Debug.Assert(_node != null && _identity != null);

            _node.AddSpawnListener(@object =>
            {
                //td: no need to synch internals, only those leaving the server boundary

                var methods = _types[@object.GetType()];
                var id = objectId(@object);
                _identity.register(id, (method, data, success) =>
                {
                    var methodFunc = methods[method];
                    methodFunc(@object, JObject.Parse(data), response => success(response.ToString()));
                });
            });
        }

        public Task<bool> Invoke(Guid @object, string method, string data, Action<JObject> success)
        {
            var completion = new TaskCompletionSource<bool>();
            _node.Queue(() => _identity.dispatch(@object, 
                method, 
                data, 
                response =>
                {
                    try
                    {
                        success(JObject.Parse(response)); //td: too much parsing
                    }
                    catch (Exception ex)
                    {
                        throw new NotImplementedException("serialize failure to be decoded on the other side");
                    }

                    completion.SetResult(true); //td: 
                })); 

            return completion.Task;
        }

        //IConcurrentServer
        public bool Has(Guid id)
        {
            return _identity.has(id);
        }

        bool _running = false;
        public void Build(Action<IConcurrentServer, Node> builder)
        {
            builder(this, _node);
        }

        Dictionary<Type, Dictionary<string, MethodFunc>> _types = new Dictionary<Type, Dictionary<string, MethodFunc>>();
        public void RegisterClass(Type @class)
        {
            if (_running)
                throw new InvalidOperationException();

            var type = @class;
            var publicMethods = type
                .GetMethods()
                .Where(method => isConcurrent(method) );

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

                methodFuncs[method.Name] = (@object, args, success) =>
                {
                    Action<object> __success = (object returnValue) =>
                    {
                        success(JObject
                            .FromObject(new { __res = returnValue }));
                    };

                    var arguments = new object[paramCount + 3];
                    for (int i = 0; i < paramCount; i++)
                    {
                        var property = args.Property(paramNames[i]);

                        arguments[i] = property.Value.ToObject(paramTypes[i]);
                    }

                    arguments[paramCount] = default(CancellationToken);
                    arguments[paramCount + 1] = __success;
                    arguments[paramCount + 2] = null as Action<Exception>;

                    method.Invoke(@object, arguments);
                };
            }

            _types[type] = methodFuncs;
        }

        public void RegisterClass<T>() where T : ConcurrentObject
        {
            RegisterClass(typeof(T));
        }

        public void RegisterInstance(Guid id, ConcurrentObject @object)
        {
            _node.Spawn(@object);

            var methods = _types[@object.GetType()];
            _identity.register(id, (method, data, success) =>
            {
                var methodFunc = methods[method];
                methodFunc(@object, JObject.Parse(data), response => success(response.ToString()));
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

        private Guid objectId(ConcurrentObject @object)
        {
            var attribute = @object
                .GetType()
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ConcurrentSingleton")
                .SingleOrDefault();

            if (attribute != null && attribute.ConstructorArguments.Count == 1)
                return Guid.Parse((string)attribute.ConstructorArguments[0].Value);

            return (Guid)@object
                .GetType()
                .GetField("__ID")
                .GetValue(@object);
        }

    }
}
