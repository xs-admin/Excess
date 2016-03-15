using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Owin;
using Microsoft.Owin.Testing;
using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Core;
using Excess.Concurrent.Runtime;
using Middleware;
using Startup;

namespace Tests
{
    using Spawner = Func<object[], ConcurrentObject>;
    using ServerExtension = LanguageExtension.Extension;
    using ConcurrentExtension = Excess.Extensions.Concurrent.Extension;
    using Compilation = Excess.Compiler.Roslyn.Compilation;

    public static class Mock
    {
        public static SyntaxTree Compile(string code, out string output)
        {
            RoslynCompiler compiler = new RoslynCompiler(
                environment: null,
                compilation: new CompilationAnalysis());

            ServerExtension.Apply(compiler);
            ConcurrentExtension.Apply(compiler);
            return compiler.ApplySemanticalPass(code, out output);
        }

        public static Compilation Build(string code, 
            IPersistentStorage storage = null, 
            List<string> errors = null)
        {
            var compilation = createCompilation(code, 
                storage: storage, 
                compilationAnalysis: new CompilationAnalysis());

            if (compilation.build() == null && errors != null)
            {
                foreach (var error in compilation.errors())
                    errors.Add(error.ToString());
            }

            return compilation;
        }

        public static TestServer CreateHttpServer(string code, Guid instanceId, string instanceClass)
        {
            return CreateHttpServer(code, new KeyValuePair<Guid, string>(instanceId, instanceClass));
        }

        public static TestServer CreateHttpServer(string code, params KeyValuePair<Guid, string>[] services)
        {
            List<Type> classes = new List<Type>();
            var node = buildConcurrent(code, classes : classes);

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

        public static TestServer CreateServer(string code, string configuration, Dictionary<string, Guid> singletons)
        {
            var errors = new List<Diagnostic>();
            var classes = new List<Type>();
            var configurations = new List<Type>();
            var instances = new Dictionary<Guid, ConcurrentObject>();
            var node = buildConcurrent(code,
                errors: errors,
                classes: classes,
                configurations: configurations,
                instances: instances,
                instanceNames : singletons);

            if (errors.Any())
                return null;

            var config = configurations
                .Single(cfg => cfg.Name == configuration);

            return TestServer.Create(app =>
            {
                startNodes(config);

                app.UseConcurrent(server =>
                {
                    foreach (var @class in classes)
                        server.RegisterClass(@class);

                    foreach (var instance in instances)
                        server.RegisterInstance(instance.Key, instance.Value);
                });
            });
        }

        //start a configuration, but just the nodes
        private static void startNodes(Type config)
        {
            var method = config.GetMethod("StartNodes");
            var instance = Activator.CreateInstance(config);
            method.Invoke(instance, new object[] { null });
        }

        private static Compilation createCompilation(string text,
            List<Diagnostic> errors = null,
            IPersistentStorage storage = null,
            CompilationAnalysis compilationAnalysis = null)
        {
            var injector = new CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>(new[]
            {
                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => compiler
                    .Environment()
                        .dependency(new[] {
                            "System.Threading",
                            "System.Threading.Tasks",
                            "System.Diagnostics",})
                        .dependency<ConcurrentObject>("Excess.Concurrent.Runtime")
                        .dependency<IConcurrentServer>("Middleware")
                        .dependency<HttpServer>("Startup")
                        .dependency<IAppBuilder>("Owin")),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => Excess
                    .Extensions
                    .Concurrent
                    .Extension
                        .Apply(compiler)),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => LanguageExtension
                    .Extension
                        .Apply(compiler))
            });

            var compilation = new Compilation(storage, compilationAnalysis);
            compilation.addDocument("test", text, injector);
            return compilation;
        }

        private static Node buildConcurrent(string text,
            List<Type> classes = null,
            Dictionary<Guid, ConcurrentObject> instances = null,
            Dictionary<string, Guid> instanceNames = null,
            List<Diagnostic> errors = null, 
            List<Type> configurations = null,
            IPersistentStorage storage = null)
        {
            var compilation = createCompilation(text, errors, storage);

            Assembly assembly = compilation.build();
            if (assembly == null)
            {
                if (errors != null)
                    errors.AddRange(compilation.errors());

                return null;
            }

            var exportTypes = new Dictionary<string, Spawner>();
            foreach (var type in assembly.GetTypes())
            {
                //non-concurrent
                if (type.BaseType != typeof(ConcurrentObject))
                {
                    if (configurations != null)
                        checkForConfiguration(type, configurations);
                    continue;
                }

                //singletons
                if (instances != null && 
                    type
                    .CustomAttributes
                    .Any(attr => attr.AttributeType.Name == "ConcurrentSingleton"))
                {
                    var id = Guid.NewGuid();
                    instances[id] = (ConcurrentObject)Activator.CreateInstance(type);
                    if (instanceNames != null)
                        instanceNames[type.Name] = id;
                }

                //classes
                if (classes != null)
                    classes.Add(type);

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

            var threads = 1; //td: config
            return new Node(threads, exportTypes);
        }

        private static void checkForConfiguration(Type type, List<Type> configurations)
        {
            if (type
                .CustomAttributes
                .Any(attr => attr.AttributeType.Name == "ServerConfiguration"))
            {
                configurations.Add(type);
            }
        }
    }
}
