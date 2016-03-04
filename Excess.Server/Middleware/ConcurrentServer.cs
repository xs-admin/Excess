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
using System.Text;

namespace Middleware
{
    using ServerFunc = Action<IConcurrentServer, string, IOwinContext>;
    using MethodFunc = Action<ConcurrentObject, JObject, IOwinResponse>;

    public interface IConcurrentServer
    {
        void Build(Action<IConcurrentServer, Node> builder);

        void Register(Guid id, ServerFunc func);
        void RegisterClass(Type @class);
        void RegisterClass<T>() where T : ConcurrentObject;
        void RegisterInstance(Guid id, ConcurrentObject @object);
    }

    public class ConcurrentServer : OwinMiddleware, IConcurrentServer
    {
        Node _node = new Node(5); //td: config
        public ConcurrentServer(OwinMiddleware next) : base(next)
        {
        }

        //IConcurrentServer
        bool _built = false;
        public void Build(Action<IConcurrentServer, Node> builder)
        {
            builder(this, _node);
            _built = true;
        }

        ConcurrentDictionary<Guid, ServerFunc> _funcs = new ConcurrentDictionary<Guid, ServerFunc>();
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
            if (_built)
                throw new InvalidOperationException();

            var type = @class;
            var publicMethods = type.GetMethods(System.Reflection.BindingFlags.Public);
            var methodFuncs = new Dictionary<string, MethodFunc>();
            foreach (var method in publicMethods)
            {
                methodFuncs[method.Name] = (@object, args, response) =>
                {
                    Action<object> success = (object returnValue) =>
                    {
                        var result = new JObject();
                        foreach (var property in returnValue
                            .GetType()
                            .GetProperties(System.Reflection.BindingFlags.Public))
                        {
                            result.Add(property.Name, JToken.FromObject(property.GetValue(returnValue)));
                        }

                        response.Write(result.ToString());
                    };

                    Action<Exception> failure = (ex) =>
                    {
                        response.StatusCode = 500;
                        response.ReasonPhrase = ex.Message;
                    };

                    var parameters = method
                        .GetParameters()
                        .Select(parameter => {
                            var property = args.Property(parameter.Name);
                            return property.Value.ToObject(parameter.ParameterType);
                        })
                        .Union(new object[] {
                            success,
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
            Register(id, (server, method, context) =>
            {
                var methodFunc = methods[method];

                //deserialize the parameters, as json
                var requestBody = context.Request.Body;
                StreamReader reader = new StreamReader(requestBody);
                JsonTextReader jsonReader = new JsonTextReader(reader);
                JsonSerializer serializer = new JsonSerializer();

                var args = serializer.Deserialize<JObject>(jsonReader);
                methodFunc(@object, args, context.Response);
            });
        }

        //OwinMiddleware
        public async override Task Invoke(IOwinContext context)
        {
            if (context.Request.Method == "POST")
            {
                Guid id;
                string method;
                if (TryParsePath(context.Request.Path.Value, out id, out method))
                {
                    ServerFunc func;
                    if (_funcs.TryGetValue(id, out func))
                        await _node.Queue(() => func(this, method, context));
                    else
                        throw new InvalidOperationException();
                }
            }

            await Next.Invoke(context);
        }


        private static bool TryParsePath(string value, out Guid id, out string method)
        {
            id = Guid.Empty;
            method = null;

            var storage = new StringBuilder();
            var awaitingId = true;
            foreach (var ch in value)
            {
                if (ch == '/')
                {
                    if (awaitingId)
                    {
                        if (!Guid.TryParse(storage.ToString(), out id))
                            return false;

                        awaitingId = false;
                        storage.Clear();
                    }
                    else return false;
                }
                else if (ch == '?')
                    break;
                else
                    storage.Append(ch);
            }

            method = storage.ToString();
            return true;
        }
    }
}
