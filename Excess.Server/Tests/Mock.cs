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
    using Middleware.NetMQ;
    using System.Threading;
    using System.Net.Http;
    using Newtonsoft.Json.Linq;

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
            Assembly assembly;
            var node = buildConcurrent(code, out assembly);

            return TestServer.Create(app =>
            {
                app.UseConcurrent(server =>
                {
                    server.Instantiator = new ReferenceInstantiator(assembly, null, null, null);

                    foreach (var @class in server.Instantiator.GetConcurrentClasses())
                    {
                        server.RegisterClass(@class);
                    }

                    foreach (var instance in server.Instantiator.GetConcurrentInstances())
                    {
                        server.RegisterInstance(instance.Key, instance.Value);
                    }
                });
            });
        }

        public static TestServer CreateServer(string code, string configuration, Dictionary<string, Guid> instances)
        {
            var errors = new List<Diagnostic>();
            var configurations = new List<Type>();
            var assembly = null as Assembly;
            var node = buildConcurrent(code,
                out assembly,
                errors: errors,
                configurations: configurations);

            if (errors.Any())
                return null;

            var config = configurations
                .Single(cfg => cfg.Name == configuration);

            return TestServer.Create(app =>
            {
                var hostedClasses = new List<Type>();
                var hostedInstances = new Dictionary<Guid, ConcurrentObject>();
                var nodeCount = startNodes(config, hostedClasses, hostedInstances);

                if (instances != null)
                {
                    foreach(var hostedInstance in hostedInstances)
                        instances[hostedInstance.Value.GetType().Name] = hostedInstance.Key;
                }

                app.UseConcurrent(server =>
                {
                    server.Identity = CreateIdentityServer(nodeCount);
                    server.Instantiator = new ReferenceInstantiator(assembly, null, hostedClasses, null);

                    foreach (var @class in server.Instantiator.GetConcurrentClasses())
                        server.RegisterClass(@class);

                    foreach (var instance in server.Instantiator.GetConcurrentInstances())
                    {
                        if (instances != null)
                            instances[instance.Value.GetType().Name] = instance.Key;

                        server.RegisterInstance(instance.Key, instance.Value);
                    }
                });
            });
        }

        public static dynamic ParseResponse(HttpResponseMessage response)
        {
            var result = JObject.Parse(response
                .Content
                .ReadAsStringAsync()
                .Result);

            return result
                .Property("__res")
                .Value;
        }

        public static IIdentityServer CreateIdentityServer(int clients)
        {
            var identity = new IdentityServer();
            var failure = null as Exception;
            var waiter = new ManualResetEvent(false);
            identity.Start("tcp://localhost:5000", clients, ex =>
            {
                failure = ex;
                waiter.Set();
            });

            waiter.WaitOne();
            if (failure != null)
                throw failure;

            return identity;
        }

        //start a configuration, but just the nodes
        private static int startNodes(Type config, IList<Type> hostedTypes, IDictionary<Guid, ConcurrentObject> hostedInstances)
        {
            var method = config.GetMethod("StartNodes");
            var instance = Activator.CreateInstance(config);
            return (int)method.Invoke(instance, new object[] { hostedTypes, hostedInstances });
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
            out Assembly assembly,
            List<Diagnostic> errors = null, 
            List<Type> configurations = null,
            IPersistentStorage storage = null)
        {
            var compilation = createCompilation(text, errors, storage);
            assembly = compilation.build();

            if (assembly == null)
            {
                if (errors != null)
                    errors.AddRange(compilation.errors());

                return null;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(ConcurrentObject) && configurations != null)
                    checkForConfiguration(type, configurations);
            }

            var threads = 2; //td: config
            return new Node(threads);
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
