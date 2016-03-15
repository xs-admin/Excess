using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Concurrent;

using Newtonsoft.Json.Linq;
using Excess.Concurrent.Runtime;

namespace Middleware
{
    using ServerFunc = Action<IConcurrentServer, string, JObject, Action<JObject>>;
    using MethodFunc = Action<ConcurrentObject, JObject, Action<JObject>>;

    public interface IConcurrentNode
    {
        void Connect(IConcurrentServer parent, Action connected, Action<Exception> failure);
        ConcurrentObject Bind(Guid id);
    }

    public interface IConcurrentServer
    {
        void Build(Action<IConcurrentServer, Node> builder);

        bool Has(Guid id);
        void Register(Guid id, ServerFunc func);
        void RegisterClass(Type @class);
        void RegisterClass<T>() where T : ConcurrentObject;
        void RegisterInstance(Guid id, ConcurrentObject @object);

        void RegisterNode(IConcurrentNode node);

        void Start();
    }

    public class ConcurrentServer : IConcurrentServer
    {
        Node _node = new Node(2, afap : false); //td: !!! config
        public ConcurrentServer()
        {
        }

        public Task<bool> Invoke(Guid @object, string method, JObject args, Action<JObject> success)
        {
            ServerFunc func;
            if (_funcs.TryGetValue(@object, out func))
                return _node.Queue(() => func(this, method, args, success));
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
        public bool Has(Guid id)
        {
            return _funcs.ContainsKey(id);
        }

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
            var publicMethods = type
                .GetMethods()
                .Where(method => isConcurrent(method) );

            var methodFuncs = new Dictionary<string, MethodFunc>();
            foreach (var method in publicMethods)
            {
                var parameters = method
                    .GetParameters();

                var paramCount = parameters.Length - 2;
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

                    var arguments = new object[paramCount + 2];
                    for (int i = 0; i < paramCount; i++)
                    {
                        var property = args.Property(paramNames[i]);
                        arguments[i] = property.Value.ToObject(paramTypes[i]);
                    }

                    arguments[paramCount] = __success;
                    arguments[paramCount + 1] = null as Action<Exception>;

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
            Register(id, (server, method, args, success) =>
            {
                var methodFunc = methods[method];
                methodFunc(@object, args, success);
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
                _node.Queue(null, () => 
                {
                    node.Connect(this, () => //success
                    { 
                        lock (_nodes)
                        {
                            _nodes.Remove(node);
                        }
                    },
                    (ex) => { _startFailed = ex; });
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
