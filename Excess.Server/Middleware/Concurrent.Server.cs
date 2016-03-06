using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Owin;
using System.Collections.Concurrent;
using Excess.Concurrent.Runtime;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;

namespace Middleware
{
    using ServerFunc = Action<IConcurrentServer, string, JObject, Action<JObject>, Action<Exception>>;
    using MethodFunc = Action<ConcurrentObject, JObject, Action<JObject>, Action<Exception>>;

    public interface IConcurrentNode
    {
        void             Connect(IConcurrentServer server);
        ConcurrentObject Bind(Guid id);
    }

    public interface IConcurrentServer
    {
        void Build(Action<IConcurrentServer, Node> builder);

        void Register(Guid id, ServerFunc func);
        void RegisterClass(Type @class);
        void RegisterClass<T>() where T : ConcurrentObject;
        void RegisterInstance(Guid id, ConcurrentObject @object);

        void RegisterNode(IConcurrentNode node);

        void Start();
    }

    public class ConcurrentServer : IConcurrentServer
    {
        Node _node = new Node(5); //td: config
        public ConcurrentServer()
        {
        }

        public void Invoke(Guid @object, string method, JObject args, Action<JObject> success, Action<Exception> failure)
        {
            ServerFunc func;
            if (_funcs.TryGetValue(@object, out func))
                _node.Queue(() => func(this, method, args, success, failure));
            else
                throw new InvalidOperationException();
        }

        //IConcurrentServer
        bool _running = false;
        Exception _startFailed = null;
        public void Build(Action<IConcurrentServer, Node> builder)
        {
            builder(this, _node);
        }

        protected ConcurrentDictionary<Guid, ServerFunc> _funcs = new ConcurrentDictionary<Guid, ServerFunc>();
        public void Register(Guid id, ServerFunc func)
        {
            for (;;)
            {
                if (_funcs.TryAdd(id, func))
                    break;
            }
        }

        Dictionary<Type, Dictionary<string, MethodFunc>> _types = new Dictionary<Type, Dictionary<string, MethodFunc>>();
        public void RegisterClass(Type @class)
        {
            if (_running)
                throw new InvalidOperationException();

            var type = @class;
            var publicMethods = type.GetMethods(System.Reflection.BindingFlags.Public);
            var methodFuncs = new Dictionary<string, MethodFunc>();
            foreach (var method in publicMethods)
            {
                methodFuncs[method.Name] = (@object, args, success, failure) =>
                {
                    Action<object> __success = (object returnValue) =>
                    {
                        success(JObject.FromObject(returnValue));
                    };

                    var parameters = method
                        .GetParameters()
                        .Select(parameter => {
                            var property = args.Property(parameter.Name);
                            return property.Value.ToObject(parameter.ParameterType);
                        })
                        .Union(new object[] {
                            __success,
                            failure
                        })
                        .ToArray();

                    method.Invoke(@object, parameters);
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
            var methods = _types[@object.GetType()];
            Register(id, (server, method, args, success, failure) =>
            {
                var methodFunc = methods[method];
                methodFunc(@object, args, success, failure);
            });
        }

        List<IConcurrentNode> _nodes = new List<IConcurrentNode>();
        public void RegisterNode(IConcurrentNode node)
        {
            _nodes.Add(node);
        }

        public virtual void Start()
        {
            foreach (var node in _nodes)
            {
                _node.Queue(null,
                () =>
                {
                    node.Connect(this);
                    lock(_nodes)
                    {
                        _nodes.Remove(node);
                    }
                },
                (ex) => { _startFailed = ex; });
            }

            int timeout = 10; 
            while (_nodes.Count > 0 && _startFailed != null && timeout > 0)
            {
                Thread.Sleep(100); //td: config, meh
                timeout--;
            }

            if (_startFailed != null)
                throw _startFailed;

            _running = _nodes.Count == 0; //all connected
            if (!_running)
                throw new InvalidOperationException("timeout");
        }
    }
}
