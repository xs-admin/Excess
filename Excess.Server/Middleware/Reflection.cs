using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Owin;
using Excess.Concurrent.Runtime;
using Excess.Runtime;

namespace Excess.Server.Middleware
{
    using FilterFunction = Func<
        Func<string, IOwinRequest, __Scope, object>,  //prev
        Func<string, IOwinRequest, __Scope, object>>; //next

    public class Loader
    {
        public static void FromAssemblies(
            IDistributedApp       app, 
            IEnumerable<Assembly> assemblies, 
            IEnumerable<string>   except = null,
            IEnumerable<string>   only = null)
        {
            if (assemblies != null)
                Application.Start(assemblies);

            var instantiator = Application.GetService<IInstantiator>()
                ?? new DefaultInstantiator();

            var classes = new List<Type>();
            var instances = new Dictionary<Guid, Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (except != null && except.Contains(type.Name))

                        continue;

                    if (only != null && !only.Contains(type.Name))
                        continue;

                    var id = default(Guid);
                    var isConcurrentType = isConcurrent(type);
                    var isServiceType = isService(type, out id);
                    if (isConcurrentType)
                        app.RegisterClass(type);

                    if (isServiceType)
                        app.RegisterInstance(id, (IConcurrentObject)instantiator.Create(type));

                    if (isServiceType || isConcurrentType)
                        continue;
                }
            }
        }

        private static bool isFilterFunction(Type type, out FilterFunction filterFunc)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ServerFilter")
                .SingleOrDefault();

            if (attribute != null)
            {
                var method = type.GetMethod("Apply");
                Debug.Assert(method != null);

                filterFunc = wrapped =>
                    (data, request, scope) => method
                        .Invoke(null, new object[] {
                            request, wrapped });

                return true;
            }

            filterFunc = null;
            return false;
        }

        private static bool isService(Type type, out Guid id)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "Service")
                .SingleOrDefault();

            if (attribute != null)
            {
                id = Guid.Parse((string)attribute.ConstructorArguments[0].Value);
                return true;
            }

            id = Guid.Empty;
            return false;
        }

        private static bool isConcurrent(Type type)
        {
            return type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "Concurrent")
                .Any();
        }
    }
}
