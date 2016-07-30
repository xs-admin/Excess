using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Attributes;
using Excess.Concurrent.Compiler;

using Excess.Server.Compiler.Model;

namespace Excess.Server.Compiler
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using ExcessCompilation = ICompilation<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompilationAnalysis = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    public class ServerExtensionOptions
    {
        public ServerExtensionOptions(bool generateJsServices = true)
        {
            GenerateJsServices = generateJsServices;
        }

        public bool GenerateJsServices { get; set; }
    }

    [Extension("server")]
    public class ServerExtension
    {
        public static void Compilation(CompilationAnalysis compilation)
        {
            compilation
                .match<ClassDeclarationSyntax>(isService)
                    .then(jsService)
                .match<ClassDeclarationSyntax>(isConcurrentClass)
                    .then(jsConcurrentClass)
                .after(addJSFiles);
        }

        public static void Apply(ExcessCompiler compiler, Scope scope, CompilationAnalysis compilation = null)
        {
            var options = scope?.get<ServerExtensionOptions>()
                ?? new ServerExtensionOptions();

            var keywords = scope?.get("keywords") as List<string>;
            keywords?.Add("service");

            //compiler.Syntax()
            //    .extension("server", ExtensionKind.Type, CompileServer);

            var lexical = compiler.Lexical();
            lexical.match()
                .token("service", named: "keyword")
                .identifier(named: "ref")
                .then(lexical.transform()
                    .replace("keyword", "class ")
                    .then(CompileService));

            compiler.Environment()
                .dependency("System.Configuration")
                .dependency("System.Security.Principal")
                .dependency("Excess.Server.Middleware");

            compiler.Lexical()
                .indented<ServerModel, ServerInstance>("server", ExtensionKind.Type, InitApp)
                    .match<ServerModel, ServerInstance, ServerLocation>(new[] {
                        "@{Url}",
                        "on port {Port}",
                        "@{Url} port {Port}",
                        "@{Url}, port {Port}" }, 
                        then: SetServerLocation)

                    .match<ServerModel, ServerInstance, ConfigModel>("using {Threads} threads", then: SetConfiguration)
                    .match<ServerModel, ServerInstance, ConfigModel>("identity @{Identity}",    then: SetConfiguration)

                    .match<ServerModel, ServerInstance, HostingModel>("hosting {ClassName}", 
                        then: (server, hosting) => server.HostedClasses.Add(hosting.ClassName))

                    .match<ServerModel, ServerInstance, ServerInstance>("new instance",
                        then: AddInstance,
                        children: child => child.match_parent())

                    .match<ServerModel, ServerInstance, SQLLocation>(new[] {
                        "sql @{ConnectionString}",
                        "sql with {ConnectionInstantiator} @{ConnectionString}",
                        "sql on connection {ConnectionId}",
                        "sql with {ConnectionInstantiator} on connection {ConnectionId}",},
                        then: (server, sql) => server.SetSqlLocation(sql))

                    .then()
                        .transform<ServerInstance>(LinkServerInstance);
        }

        private static void InitApp(ServerInstance app, LexicalExtension<SyntaxToken> extension)
        {
            app.Id = extension.Identifier.ToString();
            if (string.IsNullOrWhiteSpace(app.Id))
                app.Id = "Default"; 
        }

        private static void AddInstance(ServerInstance app, ServerInstance node)
        {
            Debug.Assert(app.Parent == null); //td: error, no nesting 

            node.Parent = app;
            node.Id = app.Id + "__" + app.Nodes.Count;

            app.Nodes.Add(node);
        }

        private static void SetConfiguration(ServerInstance instance, ConfigModel config)
        {
            instance.Threads = config.Threads; //td: multiples
            instance.Identity = config.Identity;
        }

        private static void SetServerLocation(ServerInstance instance, ServerLocation location)
        {
            instance.Url = location.Url;
            instance.Port = location.Port;
        }

        public static InitializerExpressionSyntax StringArrayInitializer(IEnumerable<string> values)
        {
            return CSharp.InitializerExpression(
                SyntaxKind.ArrayInitializerExpression,
                CSharp.SeparatedList(values
                    .Select(value => Roslyn.Quoted(value))));
        }

        private static SyntaxNode LinkServerInstance(ServerInstance app, Func<ServerModel, Scope, SyntaxNode> transform, Scope scope)
        {
            //a list of statements and types to be added
            var statements = new List<StatementSyntax>();
            foreach (var node in app.Nodes)
            {
                var appStatement = default(StatementSyntax);
                scope.AddType(LinkNode(node, out appStatement));

                if (appStatement != null)
                    statements.Add(appStatement);
            }

            //create the http start
            var except = Templates
                .StringArray
                .Get<ArrayCreationExpressionSyntax>()
                    .WithInitializer(StringArrayInitializer(app
                        .Nodes
                        .SelectMany(node => node.HostedClasses)));

            //find functional filters
            var filters = new List<ExpressionSyntax>();
            if (app.SQL != null)
            {
                var connectionString = default(ExpressionSyntax);
                if (app.SQL.ConnectionString != null)
                {
                    Debug.Assert(app.SQL.ConnectionId == null);
                    connectionString = Roslyn.Quoted(app.SQL.ConnectionString);
                }
                else
                {
                    Debug.Assert(app.SQL.ConnectionId != null);
                    connectionString = Templates
                        .SqlConnectionStringFromConfig
                        .Get<ExpressionSyntax>(
                            Roslyn.Quoted(app.SQL.ConnectionId));
                }

                var connectionClass = app.SQL.ConnectionInstantiator ?? "SqlConnection";
                filters.Add(Templates
                    .SqlFilter
                    .Get<ExpressionSyntax>(
                        connectionString,
                        CSharp.IdentifierName(connectionClass)));
            }

            filters.Add(Templates.UserFilter);

            //start the server
            statements.Add(Templates
                .StartHttpServer
                .Get<StatementSyntax>(
                    Roslyn.Quoted(app.Host.Address),
                    Roslyn.Quoted(app.Identity),
                    Roslyn.Constant(app.Threads),
                    except,
                    Roslyn.Constant(app.Nodes.Count),
                    app.Id,
                    Templates
                        .EmptyArray
                        .WithInitializer(CSharp.InitializerExpression(
                            SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList(
                            filters)))));

            //generate configuration class
            var result = Templates
                .ServerInstance
                .Get<ClassDeclarationSyntax>(app.Id, Roslyn.Quoted(app.Id));

            var start = result
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "Run"
                              && method.ParameterList.Parameters.Count == 2)
                .Single();

            return result
                .ReplaceNode(start, start
                    .AddBodyStatements(statements.ToArray()));
        }

        private static ClassDeclarationSyntax LinkNode(ServerInstance instance, out StatementSyntax appStatement)
        {
            appStatement = Templates.CallStartNode.Get<StatementSyntax>(instance.Id); //td:
            var statements = new List<StatementSyntax>();

            var only = Templates
                .StringArray
                .Get<ArrayCreationExpressionSyntax>()
                    .WithInitializer(StringArrayInitializer(instance.HostedClasses));

            statements.Add(Templates
                .StartNetMQServer
                .Get<StatementSyntax>(
                    Roslyn.Quoted(instance.Host.Address),
                    Roslyn.Quoted(instance.Parent.Identity),
                    Roslyn.Constant(instance.Threads),
                    only,
                    instance.Id));

            var result = Templates
                .ServerInstance
                .Get<ClassDeclarationSyntax>(instance.Id, Roslyn.Quoted(instance.Id));

            var start = result
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "Run"
                              && method.ParameterList.Parameters.Count == 2)
                .Single();

            return result
                .ReplaceNode(start, start
                    .AddBodyStatements(statements.ToArray()));
        }

        //generation
        private static bool isService(ClassDeclarationSyntax @class, ExcessCompilation compilation, Scope scope)
        {
            if (!isConcurrentClass(@class, compilation, scope))
                return @class.Identifier.ToString() == "Functions" && Roslyn.IsStatic(@class);

            return @class
                .AttributeLists
                .Any(attrList => attrList
                    .Attributes
                    .Any(attr => attr.Name.ToString() == "Service"));
        }

        private static void jsService(SyntaxNode node, ExcessCompilation compilation, Scope scope)
        {
            Debug.Assert(node is ClassDeclarationSyntax);
            var @class = node as ClassDeclarationSyntax;

            var serviceAttribute = @class
                .AttributeLists
                .Where(attrList => attrList
                    .Attributes
                    .Any(attr => attr.Name.ToString() == "Service"))
                .SingleOrDefault()
                    ?.Attributes
                    .FirstOrDefault(attr => attr.Name.ToString() == "Service");

            var model = compilation.GetSemanticModel(node);
            var config = scope.GetServerConfiguration();
            var name = @class.Identifier.ToString();
            var id = Guid.NewGuid();
            var body = string.Empty;

            Debug.Assert(config != null);
            if (serviceAttribute == null)
            {
                //functions
                var @namespace = @class.FirstAncestorOrSelf<NamespaceDeclarationSyntax>(
                    ancestor => ancestor is NamespaceDeclarationSyntax);

                Debug.Assert(@namespace != null); //td: error
                name = @namespace.Name.ToString();

                var path = $@"'/{string.Join("/", @namespace.Name.ToString().Split('.'))}'";
                body = functionalBody(@class, path, model);

                config.AddFunctionalContainer(name, body);
                return; //td: separate
            }
            else
            {
                var guidString = serviceAttribute
                    .ArgumentList
                    .Arguments
                    .Single(attr => attr.NameColon.Name.ToString() == "id")
                    .Expression
                    .ToString();

                id = Guid.Parse(guidString.Substring(1, guidString.Length - 2));
                body = concurrentBody(@class, config, model);
            }

            config.AddClientInterface(node.SyntaxTree, Templates
                .jsService
                .Render(new
                {
                    Name = name,
                    Body = body,
                    ID = id
                }));
        }

        private static bool isConcurrentClass(ClassDeclarationSyntax @class, ExcessCompilation compilation, Scope scope)
        {
            return @class
                .AttributeLists
                .Any(attrList => attrList
                    .Attributes
                    .Any(attr => attr.Name.ToString() == "Concurrent"));
        }

        private static void jsConcurrentClass(SyntaxNode node, ExcessCompilation compilation, Scope scope)
        {
            Debug.Assert(node is ClassDeclarationSyntax);
            var @class = node as ClassDeclarationSyntax;
            var model = compilation.GetSemanticModel(node);
            var config = scope.GetServerConfiguration();
            Debug.Assert(config != null);

            var body = concurrentBody(@class, config, model);
            config.AddClientInterface(node.SyntaxTree, Templates
                .jsConcurrentClass
                .Render(new
                {
                    Name = @class.Identifier.ToString(),
                    Body = body
                }));
        }

        private static string concurrentBody(ClassDeclarationSyntax @class, IServerConfiguration config, SemanticModel model)
        {
            var result = new StringBuilder();
            ConcurrentExtension.Visit(@class,
                methods: (name, type, parameters) =>
                {
                    result.AppendLine(Templates
                        .jsMethod
                        .Render(new
                        {
                            Name = name.ToString(),
                            Arguments = argumentsFromParameters(parameters),
                            Data = objectFromParameters(parameters),
                            Path = "'/' + this.__ID",
                            Response = calculateResponse(type, model),
                        }));
                },
                fields: (name, type, value) =>
                {
                    //td: shall we transmit properties?
                    //result.AppendLine(Templates
                    //    .jsProperty
                    //    .Render(new
                    //    {
                    //        Name = name.ToString(),
                    //        Value = valueString(value, type, model)
                    //    }));
                });

            return result.ToString();
        }

        private static string functionalBody(ClassDeclarationSyntax @class, string path, SemanticModel model)
        {
            var result = new StringBuilder();
            var methods = @class
                .Members
                .Select(member => (MethodDeclarationSyntax)member) //functional is only to have members
                .Where(method => Roslyn.IsVisible(method)); //only publics

            foreach (var method in methods)
            {
                var parameters = method
                    .ParameterList
                    .Parameters
                    .Where(param => param.Identifier.ToString() != "__scope"); //account for 

                result.AppendLine(Templates
                    .jsMethod
                    .Render(new
                    {
                        Name = method.Identifier.ToString(),
                        Arguments = argumentsFromParameters(parameters),
                        Data = objectFromParameters(parameters),
                        Path = path,
                        Response = calculateResponse(method.ReturnType, model),
                    }));
            }

            return result.ToString();
        }

        private static void addJSFiles(ExcessCompilation compilation, Scope scope)
        {
            var serverConfig = compilation.Scope.GetServerConfiguration();
            if (serverConfig == null)
                throw new ArgumentException("IServerConfiguration");

            var clientCode = serverConfig.GetClientInterface();
            var servicePath = serverConfig.GetServicePath();
            if (servicePath == null)
                throw new InvalidOperationException("cannot find the path");

            compilation.AddContent(servicePath, Templates
                .servicesFile
                .Render(new
                {
                    Members = clientCode
                }));
        }

        private static string calculateResponse(TypeSyntax type, SemanticModel model)
        {
            var typeSymbol = model.GetSymbolInfo(type).Symbol as ITypeSymbol;
            if (typeSymbol != null)
                return ResponseVisitor.Get(typeSymbol, "response");

            return string.Empty;
        }

        private static string valueString(ExpressionSyntax value, TypeSyntax type, SemanticModel model)
        {
            if (value != null)
                return value.ToString();

            return "null"; //td: use the model
        }

        private static string objectFromParameters(IEnumerable<ParameterSyntax> parameters)
        {
            var result = new StringBuilder();
            foreach (var parameter in parameters)
            {
                var identifier = parameter.Identifier.ToString();
                result.AppendLine($"{identifier} : {identifier},");
            }

            return result.ToString();
        }

        private static string argumentsFromParameters(IEnumerable<ParameterSyntax> parameters)
        {
            return CSharp
                .ArgumentList(CSharp.SeparatedList(
                    parameters
                    .Select(parameter => CSharp.Argument(CSharp.
                        IdentifierName(parameter.Identifier)))))
                .ToString();
        }

        private static SyntaxNode CompileService(SyntaxNode node, Scope scope)
        {
            var @class = (node as ClassDeclarationSyntax)
                .AddAttributeLists(CSharp.AttributeList(CSharp.SeparatedList(new[] {CSharp
                        .Attribute(CSharp
                            .ParseName("Service"),
                            CSharp.ParseAttributeArgumentList(
                                $"(id : \"{Guid.NewGuid()}\")"))})));

            var options = new Options();
            return ConcurrentExtension.CompileClass(options)(@class, scope);
        }
    }
}
