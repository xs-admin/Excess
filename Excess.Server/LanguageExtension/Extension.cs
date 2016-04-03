using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace LanguageExtension
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using ExcessCompilation = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;
    using System.Text;

    public class Extension
    {
        public static void Apply(ExcessCompiler compiler, bool withCompilation = true)
        {
            compiler.Syntax()
                .extension("server", ExtensionKind.Type, CompileServer);

            if (withCompilation)
            {
                var compilation = compiler.Compilation();
                if (compilation != null)
                    compilation
                        .match<ClassDeclarationSyntax>(isConcurrentClass)
                            .then(jsConcurrentClass)
                        .match<ClassDeclarationSyntax>(isConcurrentObject)
                            .then(jsConcurrentObject);
            }
        }

        //server information
        class ServerModel
        {
            public ServerModel(SyntaxToken serverId = default(SyntaxToken))
            {
                ServerId = serverId;
                Threads = Templates.DefaultThreads;
                Nodes = new List<ServerModel>();
                DeployStatements = new List<StatementSyntax>();
                StartStatements = new List<StatementSyntax>();
                HostedClasses = new List<TypeSyntax>();
            }

            public SyntaxToken ServerId { get; private set; }
            public ExpressionSyntax Url { get; set; }
            public ExpressionSyntax Threads { get; set; }
            public ExpressionSyntax Identity { get; set; }
            public ExpressionSyntax Connection { get; set; }
            public List<ServerModel> Nodes { get; private set; }
            public List<StatementSyntax> DeployStatements { get; private set; }
            public List<StatementSyntax> StartStatements { get; private set; }
            public List<TypeSyntax> HostedClasses { get; private set; }
        };

        //server compilation
        private static SyntaxNode CompileServer(SyntaxNode node, Scope scope, SyntacticalExtension<SyntaxNode> data)
        {
            Debug.Assert(node is MethodDeclarationSyntax);
            var methodSyntax = node as MethodDeclarationSyntax;

            var mainServer = new ServerModel();
            if (!parseMainServer(methodSyntax, mainServer))
                return node; //errors

            //main server class
            var configurationClass = Templates
                .ConfigClass
                .Get<ClassDeclarationSyntax>(data.Identifier);

            configurationClass = configurationClass
                .ReplaceNodes(configurationClass
                    .DescendantNodes()
                    .OfType<MethodDeclarationSyntax>(),
                    (on, nn) =>
                    {
                        var methodName = nn.Identifier.ToString();
                        switch (methodName)
                        {
                            case "Deploy":
                                return nn.AddBodyStatements(
                                    mainServer
                                        .Nodes
                                        .SelectMany(serverNode => serverNode.DeployStatements)
                                    .Union(
                                        mainServer.DeployStatements)
                                    .ToArray());
                            case "Start":
                                return nn.AddBodyStatements(mainServer.StartStatements.ToArray());
                            case "StartNodes":
                                return nn.AddBodyStatements(
                                    mainServer
                                    .Nodes
                                    .Select(serverNode => Templates
                                        .NodeInvocation
                                        .Get<StatementSyntax>(serverNode.ServerId))
                                    .ToArray());
                            case "RemoteTypes":
                                var initializer = Templates
                                    .RemoteTypes
                                    .DescendantNodes()
                                    .OfType<InitializerExpressionSyntax>()
                                    .Single();

                                return nn.AddBodyStatements(Templates
                                    .RemoteTypes
                                    .ReplaceNode(
                                        initializer,
                                        initializer.AddExpressions(mainServer
                                            .Nodes
                                            .SelectMany(serverNode => serverNode.HostedClasses)
                                            .Select(type => CSharp.TypeOfExpression(type))
                                            .ToArray())));
                            case "NodeCount":
                                return nn.AddBodyStatements(CSharp
                                    .ReturnStatement(CSharp.ParseExpression(
                                        mainServer.Nodes.Count.ToString())));
                        }

                        throw new NotImplementedException();
                    });

            //add a method per node which will be invoked when starting
            configurationClass = configurationClass
                .AddMembers(mainServer
                    .Nodes
                    .Select(serverNode =>
                    {
                        var hostedTypes = Templates
                            .TypeArray
                            .WithInitializer(CSharp.InitializerExpression(
                                SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList<ExpressionSyntax>(
                                serverNode
                                    .HostedClasses
                                    .Select(type => CSharp.TypeOfExpression(type)))));

                        return Templates
                            .NodeMethod
                            .Get<MethodDeclarationSyntax>(
                                serverNode.ServerId,
                                hostedTypes)
                            .AddBodyStatements(serverNode
                                .StartStatements
                                .ToArray());
                    })
                    .ToArray());


            //apply changes
            var document = scope.GetDocument();
            document.change(node.Parent, RoslynCompiler.AddType(configurationClass));
            document.change(node.Parent, RoslynCompiler.RemoveMember(node));
            return node; //untouched, it will be removed
        }

        private static bool parseMainServer(MethodDeclarationSyntax method, ServerModel result)
        {
            var valid = true;
            foreach (var statement in method.Body.Statements)
            {
                if (statement is ExpressionStatementSyntax)
                {
                    var expressionStatement = statement as ExpressionStatementSyntax;
                    if (expressionStatement.Expression is AssignmentExpressionSyntax)
                        valid = parseServerParameters(expressionStatement.Expression as AssignmentExpressionSyntax, result);
                    else
                        valid = false; //td: error
                }
                else if (statement is LocalDeclarationStatementSyntax)
                {
                    var localDeclaration = (statement as LocalDeclarationStatementSyntax)
                        .Declaration;
                    if (localDeclaration.Type.ToString() == "Node" && localDeclaration.Variables.Count == 1)
                    {
                        var variable = localDeclaration.Variables[0];
                        var initializer = variable.Initializer;
                        if (initializer != null)
                        {
                            var nodeName = variable.Identifier;
                            var value = variable.Initializer.Value;
                            if (value is ObjectCreationExpressionSyntax)
                            {
                                var nodeServer = new ServerModel(variable.Identifier);
                                nodeServer.Identity = result.Identity;

                                var objectCreation = value as ObjectCreationExpressionSyntax;
                                valid = parseNodeStatements(objectCreation, nodeServer);
                                result.Nodes.Add(nodeServer);
                            }
                            else
                                valid = false; //td: error
                        }
                    }
                    else
                        valid = false; //td: error
                }
                else 
                    valid = false; //td: error
            }

            if (!valid)
                return false;

            //instances hosted by nodes
            var hostedInNodes = result
                .Nodes
                .SelectMany(node => node
                    .HostedClasses
                    .Select(type => CSharp.TypeOfExpression(type)));

            var hostedInstances = Templates
                .TypeArray
                .WithInitializer(CSharp.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression, CSharp
                    .SeparatedList<ExpressionSyntax>(
                        hostedInNodes)));

            //the web server
            result
                .StartStatements
                .AddRange(new StatementSyntax[]
                {
                    Templates
                        .HttpServer
                        .Get<StatementSyntax>(
                            result.Url,
                            result.Identity,
                            result.Threads)
                });

            return true;
        }

        private static bool parseNodeStatements(ObjectCreationExpressionSyntax creation, ServerModel result)
        {
            var valid = true;
            foreach (var expression in creation.Initializer.Expressions)
            {
                if (expression is AssignmentExpressionSyntax)
                    valid = parseServerParameters(expression as AssignmentExpressionSyntax, result);
                else
                    valid = false; //td: error
            }

            if (valid)
            {
                var serverType = getNodeServer(creation.Type);
                if (serverType != null)
                {
                    result
                        .StartStatements
                        .AddRange(new StatementSyntax[]
                        {
                            Templates
                                .NodeServer
                                .Get<StatementSyntax>(
                                    serverType,
                                    result.Url,
                                    result.Identity,
                                    result.Threads)
                        });
                    return true;
                }
            }

            return false;
        }

        private static bool parseServerParameters(AssignmentExpressionSyntax assignment, ServerModel result)
        {
            switch (assignment.Left.ToString())
            {
                case "Url":
                    result.Url = assignment.Right;
                    break;
                case "Threads":
                    result.Threads = assignment.Right;
                    break;
                case "Identity":
                    result.Identity = assignment.Right;
                    break;
                case "Hosts":
                    if (assignment.Right is ImplicitArrayCreationExpressionSyntax)
                    {
                        var hostedClasses = (assignment.Right as ImplicitArrayCreationExpressionSyntax)
                            .Initializer
                            .Expressions;

                        var valid = true;
                        foreach (var @class in hostedClasses)
                        {
                            var type = @class as TypeSyntax;
                            if (type == null && @class is IdentifierNameSyntax)
                                type = CSharp.ParseTypeName(@class.ToString());

                            if (type != null)
                                result.HostedClasses.Add(type);
                            else
                                valid = false; //error
                        }

                        return valid;
                    }
                    else
                        return false; //td: error
                default:
                    return false; //td: error
            }

            return true;
        }

        private static string getNodeServer(TypeSyntax type)
        {
            switch (type.ToString())
            {
                case "NetMQ.Node": return "NetMQNode";
            }

            throw new ArgumentException("type");
        }

        //generation
        private static bool isConcurrentObject(ClassDeclarationSyntax @class, SemanticModel model, Scope scope)
        {
            if (!isConcurrentClass(@class, model, scope))
                return false;

            return @class
                .AttributeLists
                .Any(attrList => attrList
                    .Attributes
                    .Any(attr => attr.Name.ToString() == "ConcurrentSingleton"));
        }

        private static void jsConcurrentObject(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static bool isConcurrentClass(ClassDeclarationSyntax @class, SemanticModel model, Scope scope)
        {
            if (@class.BaseList == null)
                return false;

            return @class
                .BaseList
                .Types
                .Any(type => type.Type.ToString() == "ConcurrentObject");
        }

        private static void jsConcurrentClass(SyntaxNode node, SemanticModel model, Scope scope)
        {
            Debug.Assert(node is ClassDeclarationSyntax);
            var @class = node as ClassDeclarationSyntax;

            var config = scope.GetServerConfiguration();
            Debug.Assert(config != null);

            var body = new StringBuilder();
            ConcurrentClass.Visit(@class,
                methods: (name, type, parameters) =>
                {
                    body.AppendLine(Templates
                        .jsMethod
                        .Render(new
                        {
                            Name = name.ToString(),
                            Arguments = argumentsFromParameters(parameters),
                            Data = objectFromParameters(parameters),
                            Response = calculateResponse(type, model),
                        }));
                },
                fields: (name, type, value) =>
                {
                    body.AppendLine(Templates
                        .jsProperty
                        .Render(new
                        {
                            Name = name.ToString(),
                            Value = valueString(value, type, model) 
                        }));
                });

            config.AddClientInterface(node.SyntaxTree, Templates
                .jsConcurrentClass
                .Render(new
                {
                    Name = @class.Identifier.ToString(),
                    Body = body.ToString()
                }));
        }

        private static string calculateResponse(TypeSyntax type, SemanticModel model)
        {
            var typeSymbol = model.GetSymbolInfo(type).Symbol as ITypeSymbol;
            Debug.Assert(typeSymbol != null);
            return ResponseVisitor.Get(typeSymbol, "response");
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

        //td: concurrent should offer this somehow
        static class ConcurrentClass
        {
            public static void Visit(ClassDeclarationSyntax @class,
                Action<SyntaxToken, TypeSyntax, IEnumerable<ParameterSyntax>> methods = null,
                Action<SyntaxToken, TypeSyntax, ExpressionSyntax> fields = null,
                Action<IEnumerable<ParameterSyntax>> constructors = null)
            {
                var publicMembers = @class
                    .DescendantNodes()
                    .OfType<MemberDeclarationSyntax>()
                    .Where(member => Roslyn.IsVisible(member));

                foreach (var member in publicMembers)
                {
                    if (member is MethodDeclarationSyntax && methods != null)
                    {
                        var method = member as MethodDeclarationSyntax;

                        //since we generate multiple methods
                        if (methods != null
                            && method
                                .AttributeLists
                                .Any(attrList => attrList
                                    .Attributes
                                    .Any(attr => attr.Name.ToString() == "Concurrent")))
                        {
                            methods(method.Identifier, method.ReturnType, method.ParameterList.Parameters);
                        }
                    }
                    else if (member is FieldDeclarationSyntax && fields != null)
                    {
                        var declaration = (member as FieldDeclarationSyntax)
                            .Declaration;

                        var variable = declaration
                            .Variables
                            .Single();

                        fields(variable.Identifier, declaration.Type, variable.Initializer.Value);
                    }
                    else if (member is PropertyDeclarationSyntax && fields != null)
                    {
                        var property = member as PropertyDeclarationSyntax;
                        fields(property.Identifier, property.Type, null);
                    }
                    else if (member is ConstructorDeclarationSyntax && constructors != null)
                        constructors((member as ConstructorDeclarationSyntax)
                            .ParameterList
                            .Parameters);
                    else
                        Debug.Assert(false); //td:
                }
            }
        }
    }
}
