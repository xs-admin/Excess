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
    using Excess.Compiler;
    using Excess.Compiler.Roslyn;
    using LanguageExtension;
    using Owin;
    using Startup;
    using System.Linq;
    using Spawner = Func<object[], ConcurrentObject>;

    public static class Mock
    {
        public static SyntaxTree Compile(string code, out string output)
        {
            RoslynCompiler compiler = new RoslynCompiler(
                environment: null,
                compilation: new CompilationAnalysis());

            Extension.Apply(compiler);
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
            List<Diagnostic> errors = null, 
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

            var threads = 1; //td: config
            return new Node(threads, exportTypes);
        }
    }
}
