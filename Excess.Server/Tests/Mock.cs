using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Owin.Testing;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;
using Excess.Concurrent.Runtime;
using Middleware;

namespace Tests
{
    using System.Linq;
    using Spawner = Func<object[], ConcurrentObject>;

    public static class Mock
    {
        public static TestServer CreateHttpServer(string code, Guid instanceId, string instanceClass)
        {
            return CreateHttpServer(code, new KeyValuePair<Guid, string>(instanceId, instanceClass));
        }

        public static TestServer CreateHttpServer(string code, params KeyValuePair<Guid, string>[] services)
        {
            IEnumerable<Type> classes;
            var node = buildConcurrent(code, out classes);

            var instances = new Dictionary<Guid, ConcurrentObject>();
            foreach (var service in services)
            {
                var instance = node.Spawn(service.Value);
                instances[service.Key] = instance;
            }

            return TestServer.Create(app =>
            {
                app.UseConcurrent(server =>
                {
                    foreach (var @class in classes)
                    {
                        server.RegisterClass(@class);
                    }

                    foreach (var instance in instances)
                    {
                        server.RegisterInstance(instance.Key, instance.Value);
                    }
                });
            });
        }

        private static Node buildConcurrent(string text, out IEnumerable<Type> classes)
        {
            IEnumerable<Diagnostic> errors = null;

            var compilation = new Excess
                .Compiler
                .Roslyn
                .Compilation(null, null);

            var injector = new CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>(new[]
            {
                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => compiler
                    .Environment()
                        .dependency(new[] {
                            "System.Threading",
                            "System.Threading.Tasks",
                            "System.Diagnostics",})
                        .dependency<ConcurrentObject>("Excess.Concurrent.Runtime")),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => Excess
                    .Extensions
                    .Concurrent
                    .Extension
                        .Apply(compiler))
            });

            compilation.addDocument("test", text, injector);

            Assembly assembly = compilation.build();
            if (assembly == null)
            {
                errors = compilation.errors();

                //debug
                StringBuilder errorLines = new StringBuilder();
                foreach (var error in errors)
                {
                    errorLines.AppendLine(error.ToString());
                }

                throw new InvalidProgramException(errorLines.ToString());
            }

            var exportTypes = new Dictionary<string, Spawner>();
            var classList = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(ConcurrentObject))
                    continue;

                classList.Add(type);

                var useParameterLess = type.GetConstructors().Length == 0;
                if (!useParameterLess)
                    useParameterLess = type.GetConstructor(new Type[] { }) != null;

                var typeName = type.ToString();
                exportTypes[typeName] = (args) =>
                {
                    if (useParameterLess)
                        return (ConcurrentObject)Activator.CreateInstance(type);

                    var ctor = type.GetConstructor(args
                        .Select(arg => arg.GetType())
                        .ToArray());

                    if (ctor != null)
                        return (ConcurrentObject)ctor.Invoke(args);

                    throw new InvalidOperationException("unable to find a constructor");
                };
            }

            classes = classList;

            var threads = 1; //since we are not testing concurrent, simplify
            return new Node(threads, exportTypes);
        }
    }
}
