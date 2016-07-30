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
        public static __Scope Load(IEnumerable<Assembly> assemblies)
        {
            var result = new __Scope(default(__Scope));
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type
                        .CustomAttributes
                        .Any(attr => attr.AttributeType == typeof(AutoInit)))
                    {
                        type.GetMethod("__init")
                            ?.Invoke(null, new object[] { result });
                    }
                }
            }

            return result;
        }
    }
}
