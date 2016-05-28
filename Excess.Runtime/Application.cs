using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
