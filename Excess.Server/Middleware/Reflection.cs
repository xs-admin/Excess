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
        public static void Run(IEnumerable<Assembly> assemblies, string[] args)
        {
            var serverName = default(string);
            switch (args.Length)
            {
                case 0: break;
                case 1:
                    serverName = args[0];
                    break;
                default:
                    Console.WriteLine("please provide one and only one server");
                    return;
            }

            //init the app
            var appScope = Application.Load(assemblies);

            //find the server
            var servers = assemblies
                .SelectMany(assembly => assembly.GetTypes()
                    .Where(type => isServer(type))
                    .Select(type => (IServer)Activator.CreateInstance(type)))
                .ToList();

            if (servers == null || !servers.Any())
            {
                Console.WriteLine("no servers are registered");
                return;
            }

            if (serverName == null)
            {
                if (servers.Count == 1)
                    serverName = servers.Single().Name;
                else
                    serverName = "Default";
            }

            var serverInstance = servers
                .SingleOrDefault(server => server.Name == serverName);

            if (serverInstance == null)
            {
                Console.WriteLine($"no server is registered under the name {serverName}");
                return;
            }

            //run
            serverInstance.Run(appScope);
        }

        private static bool isServer(Type serverType) => serverType.FindInterfaces(
            (type, data) => type == typeof(IServer), null)
            .Any();

        public static void FromAssemblies(
            IDistributedApp       app, 
            IEnumerable<Assembly> assemblies,
            IInstantiator         instantiator,
            IEnumerable<string>   except = null,
            IEnumerable<string>   only = null)
        {
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
