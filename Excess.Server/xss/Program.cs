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
            var concurrentInstances = null as IEnumerable<KeyValuePair<Guid, ConcurrentObject>>;
            var concurrentNodes = null as IEnumerable<IConcurrentNode>;
            if (!parseArguments(args, out errors, out url, out concurrentClasses, out concurrentInstances, out concurrentNodes))
            {
                Console.Write(errors);
                return;
            }

            //start a http concurrent server
            throw new NotImplementedException(); //td: instantiator

            var instantiator = null as ReferenceInstantiator; 
            Startup.HttpServer.Start(url, instantiator: instantiator);
        }

        private static bool parseArguments(string[] args, out string errors, out string url, out IEnumerable<Type> concurrentClasses, out IEnumerable<KeyValuePair<Guid, ConcurrentObject>> concurrentInstances, out IEnumerable<IConcurrentNode> concurrentNodes)
        {
            errors = null;
            url = null;
            concurrentClasses = null;
            concurrentInstances = null;
            concurrentNodes = null; //td: nodes
            var directory = Environment.CurrentDirectory;


            switch (args.Length)
            {
                case 1:
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

        private static void concurrentAssemblies(string filePath, out string errors, out IEnumerable<Type> concurrentClasses, out IEnumerable<KeyValuePair<Guid, ConcurrentObject>> concurrentInstances)
        {
            var assemblies = Directory
                .EnumerateFiles(filePath)
                .Select(file => Assembly.LoadFile(file));

            var found = false;
            var classes = new List<Type>();
            var instances = new Dictionary<Guid, ConcurrentObject>();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (isConcurrent(type, instances))
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

        private static bool isConcurrent(Type type, Dictionary<Guid, ConcurrentObject> instances)
        {
            if (type.BaseType == null || type.BaseType.Name != "ConcurrentObject")
                return false;

            var method = type.GetMethod("__singleton", BindingFlags.Static);
            if (method != null)
                method.Invoke(null, new object[] { instances });

            return true;
        }
    }
}
