using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Runtime
{
    public static class Application
    {
        static Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void RegisterService<T>(T service)
        {
            lock (_services)
            {
                _services[typeof(T)] = service;
            }
        }

        public static T GetService<T>()
        {
            object result = null;
            lock (_services)
            {
                _services.TryGetValue(typeof(T), out result);
            }

            return (T)result;
        }

        public static void Start(IEnumerable<Assembly> clients)
        {
            foreach (var client in clients)
            {
                foreach (var type in client.GetTypes())
                {
                    if (type
                        .CustomAttributes
                        .Any(attr => attr.AttributeType == typeof(AutoInit)))
                    {
                            type.GetMethod("__init")
                                ?.Invoke(null, new object[] { });
                    }
                }
            }
        }
    }
}
