using Excess.Concurrent.Runtime;
using Excess.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Excess.Server.Middleware
{
    public class Loader
    {
        public static void FromAssemblies(
            IDistributedApp app, 
            IEnumerable<Assembly> assemblies, 
            IEnumerable<string> except = null,
            IEnumerable<string> only = null)
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
                    if (isConcurrent(type))
                        app.RegisterClass(type);

                    if (isService(type, out id))
                        app.RegisterInstance(id, (IConcurrentObject)instantiator.Create(type));
                }
            }
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
