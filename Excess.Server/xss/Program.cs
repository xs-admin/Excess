using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Middleware;
using Excess.Concurrent.Runtime;

namespace xss
{
    class Program
    {
        static void Main(string[] args)
        {
            var errors = null as string;
            var url = null as string;
            var concurrentClasses = null as IEnumerable<Type>;
            var concurrentInstances = null as IEnumerable<KeyValuePair<Guid, Type>>;
            var staticFiles = null as string;
            if (!parseArguments(args, out errors, out url, out concurrentClasses, out concurrentInstances, out staticFiles))
            {
                Console.Write(errors);
                return;
            }

            //start a http concurrent server
            HttpServer.Start(url,
                classes: concurrentClasses,
                instances: concurrentInstances,
                staticFiles: staticFiles);
        }

        private static bool parseArguments(string[] args, out string errors, out string url, out IEnumerable<Type> concurrentClasses, out IEnumerable<KeyValuePair<Guid, Type>> concurrentInstances, out string staticFiles)
        {
            staticFiles = args.Length == 2
                ? args[1]
                : null;

            errors = null;
            url = null;
            concurrentClasses = null;
            concurrentInstances = null;
            var directory = Environment.CurrentDirectory;

            switch (args.Length)
            {
                case 1:
                case 2:
                    url = args[0];

                    var currentDirectory = Environment.CurrentDirectory;
                    concurrentAssemblies(currentDirectory, out errors, out concurrentClasses, out concurrentInstances);
                    break;
                case 0:
                    errors = "you must provide the url where the server is hosted";
                    break;
                default:
                    errors = "too many parameters";
                    break;
            }

            return errors != null;
        }

        private static void concurrentAssemblies(string filePath, out string errors, out IEnumerable<Type> concurrentClasses, out IEnumerable<KeyValuePair<Guid, Type>> concurrentInstances)
        {
            var assemblies = Directory
                .EnumerateFiles(filePath)
                .Where(file => Path.GetExtension(file) == ".dll")
                .Select(file => Assembly.LoadFrom(file));

            var found = false;
            var classes = new List<Type>();
            var instances = new Dictionary<Guid, Type>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    var id = default(Guid);
                    if (isService(type, out id))
                    {
                        instances[id] = type;
                    }
                    else if (isConcurrent(type))
                    {
                        found = true;
                        classes.Add(type);
                    }
                }
            }

            errors = found
                ? null
                : "could not locate any concurrent assembly";

            concurrentClasses = classes;
            concurrentInstances = instances;
        }

        private static bool isService(Type type, out Guid id)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "Service")
                .SingleOrDefault();

            if (attribute != null)
            {
                attribute = type
                    .CustomAttributes
                    .Where(attr => attr.AttributeType.Name == "Concurrent")
                    .SingleOrDefault();

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
