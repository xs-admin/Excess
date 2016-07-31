using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Owin;
using Microsoft.Owin.Testing;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Core;
using Excess.Concurrent.Runtime;
using Excess.Server.Middleware;
using Excess.Runtime;
using xslang;

namespace Tests
{
    using ServerExtension = Excess.Server.Compiler.ServerExtension;
    using ConcurrentExtension = Excess.Concurrent.Compiler.ConcurrentExtension;
    using FactoryMethod = Func<IConcurrentApp, object[], IConcurrentObject>;
    using Compilation = Excess.Compiler.Roslyn.RoslynCompilation;
    using CompilationAnalysis = Excess.Compiler.ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    public static class Mock
    {
        public static SyntaxTree Compile(string code, out string output)
        {
            RoslynCompiler compiler = new RoslynCompiler(environment: null);

            ServerExtension.Apply(compiler, compiler.Scope);
            ConcurrentExtension.Apply(compiler);
            return compiler.ApplySemanticalPass(code, out output);
        }

        public static Compilation Build(string code, 
            IPersistentStorage storage = null, 
            List<string> errors = null,
            bool generateJSFiles = false)
        {
            var analysis = generateJSFiles
                ? new CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>()
                : null;

            var compilation = createCompilation(code, 
                storage: storage ?? (generateJSFiles
                    ? new MockStorage()
                    : null),
                analysis: analysis);

            if (generateJSFiles)
            {
                var environment = compilation.Scope.get<ICompilerEnvironment>();
                Assert.IsNotNull(environment);

                environment.setting("servicePath", "");
            }

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

        public static TestServer CreateFunctionalServer(string code)
        {
            var errors = new List<Diagnostic>();
            var assembly = buildServer(code, errors: errors);
            if (errors.Any())
                return null;

            var functionTypes = assembly
                .ExportedTypes
                .Where(type => type.Name == "Functions");

            var scope = new __Scope(null);
            return TestServer.Create(appBuilder =>
            {
                appBuilder.UseExcess(scope, functional: functionTypes);
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

            var nodeConfigs = configurations
                .Where(cfg => cfg.Name.StartsWith(configuration + "__"))
                .ToArray();

            var remoteTypes = null as IEnumerable<Type>;
            var clients = 0;
            var startNodes = false;
            if (config != null)
            {
                remoteTypes = (IEnumerable<Type>)config
                    .GetMethod("RemoteTypes")
                    .Invoke(null, new object[] { });

                clients = nodeConfigs.Length;
                startNodes = clients > 0;
            }

            var classes = new List<Type>();
            var app = appFromAssembly(assembly, 
                allInstances: instances, 
                remoteTypes: remoteTypes, 
                commonClasses: classes);

            app.Connect = _ =>
            {
                if (startNodes)
                    Task.Run(() => config
                        .GetMethod("StartNodes")
                        .Invoke(null, new object[] { classes }));

                var waiter = new ManualResetEvent(false);
                var error = null as Exception;
                NetMQFunctions.StartServer(app, "tcp://localhost:5000", clients,
                    connected: ex =>
                    {
                        error = ex;
                        waiter.Set();
                    });

                waiter.WaitOne();
                return error;
            };

            return TestServer.Create(builder => builder.UseExcess(app));
        }

        private static IDistributedApp appFromAssembly(Assembly assembly, 
            IDictionary<string, Guid> userInstances = null,
            IDictionary<string, Guid> allInstances = null,
            IEnumerable<Type> remoteTypes = null,
            IList<Type> commonClasses = null)
        {
            var types = new Dictionary<string, FactoryMethod>();
            var concurrentApp = new TestConcurrentApp(types, new DefaultInstantiator());
            var app = new DistributedApp(concurrentApp);

            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != typeof(ConcurrentObject))
                    continue;

                Guid id;
                if (remoteTypes != null && remoteTypes.Contains(type))
                {
                    if (allInstances != null && isService(type, out id))
                        allInstances[type.Name] = id;
                    continue;
                }

                commonClasses?.Add(type);
                app.RegisterClass(type);

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
                    .Single();

                id = Guid.Parse((string)attribute.NamedArguments[0].TypedValue.Value);
                return true;
            }

            id = Guid.Empty;
            return false;
        }

        public static dynamic ParseResponse(HttpResponseMessage response)
        {
            var json = JObject.Parse(response
                .Content
                .ReadAsStringAsync()
                .Result) as dynamic;

            if (json.__res != null)
                return json.__res;

            return json
                .Property("__ex")
                .Value;
        }

        private static Compilation createCompilation(string text,
            List<Diagnostic> errors = null,
            IPersistentStorage storage = null,
            CompilationAnalysis analysis = null)
        {
            var injector = new CompositeInjector<SyntaxToken, SyntaxNode, SemanticModel>(new[]
            {
                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(compiler => compiler
                    .Environment()
                        .dependency<ExcessOwinMiddleware>("Excess.Server.Middleware")
                        .dependency<IAppBuilder>("Owin")
                        .dependency<__Scope>("Excess.Runtime")),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(
                    compiler => ConcurrentExtension.Apply((RoslynCompiler)compiler)),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(
                    compiler => ServerExtension.Apply(compiler, new Scope(null))),

                new DelegateInjector<SyntaxToken, SyntaxNode, SemanticModel>(
                    compiler => Functions.Apply(compiler))
            });

            if (analysis != null)
                ServerExtension.Compilation(analysis);

            var compilation = new Compilation(storage, analysis: analysis);
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
