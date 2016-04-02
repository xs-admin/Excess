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
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Tests
{
    using ServerExtension = LanguageExtension.Extension;
    using ConcurrentExtension = Excess.Extensions.Concurrent.Extension;
    using Compilation = Excess.Compiler.Roslyn.Compilation;
    using FactoryMethod = Func<IConcurrentApp, object[], IConcurrentObject>;

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
            return CreateHttpServer(code, new Dictionary<string, Guid>()
            {
                { instanceClass, instanceId } 
            });
        }

        public static TestServer CreateHttpServer(string code, IDictionary<string, Guid> instances)
        {
            var errors = new List<Diagnostic>();
            var assembly = buildServer(code, errors: errors);
            if (errors.Any())
                return null;

            var app = appFromAssembly(assembly, userInstances: instances);
            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseExcess(app);
            });
        }

        public static TestServer CreateServer(string code, string configuration, IDictionary<string, Guid> instances)
        {
            var errors = new List<Diagnostic>();
            var configurations = new List<Type>();
            var assembly = buildServer(code,
                errors: errors,
                configurations: configurations);

            if (errors.Any())
                return null;

            var config = configurations
                .Single(cfg => cfg.Name == configuration);

            var app = appFromAssembly(assembly, allInstances: instances);
            return TestServer.Create(builder =>
            {
                builder.UseExcess(app);
            });
        }

        private static IDistributedApp appFromAssembly(Assembly assembly, 
            IDictionary<string, Guid> userInstances = null,
            IDictionary<string, Guid> allInstances = null)
        {
            var types = new Dictionary<string, FactoryMethod>();
            var concurrentApp = new TestConcurrentApp(types);
            var app = new DistributedApp(concurrentApp);

            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(ConcurrentObject))
                    continue;

                app.RegisterClass(type);

                Guid id;
                if (userInstances != null)
                {
                    if (userInstances.TryGetValue(type.Name, out id))
                        app.RegisterInstance(id, (IConcurrentObject)Activator.CreateInstance(type));
                }
                else if (type.IsConcurrentSingleton(out id))
                {
                    var instance = (IConcurrentObject)Activator.CreateInstance(type);
                    app.RegisterInstance(id, (IConcurrentObject)Activator.CreateInstance(type));

                    if (allInstances != null)
                        allInstances[type.Name] = id;
                }
            }

            return app;
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

        //start a configuration, but just the nodes
        private static int startNodes(Type config, IList<Type> hostedTypes, IDictionary<Guid, IConcurrentObject> hostedInstances)
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
                        .dependency<ExcessOwinMiddleware>("Middleware")
                        .dependency<IAppBuilder>("Owin")),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => Excess
                    .Extensions
                    .Concurrent
                    .Extension
                        .Apply((RoslynCompiler)compiler)),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => LanguageExtension
                    .Extension
                        .Apply(compiler))
            });

            var compilation = new Compilation(storage, compilationAnalysis);
            compilation.addDocument("test", text, injector);
            return compilation;
        }

        private static Assembly buildServer(string text,
            List<Diagnostic> errors = null, 
            List<Type> configurations = null,
            IPersistentStorage storage = null)
        {
            var compilation = createCompilation(text, errors, storage);
            var assembly = compilation.build();
            if (assembly == null)
            {
                if (errors != null)
                    errors.AddRange(compilation.errors());

                return null;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(IConcurrentObject) && configurations != null)
                    checkForConfiguration(type, configurations);
            }

            return assembly;
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
