using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace LanguageExtension
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using ExcessCompilation = ICompilationAnalysis<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis.CSharp;

    public class Extension
    {
        public static void Apply(ExcessCompiler compiler)
        {
            compiler.Syntax()
                .extension("server", ExtensionKind.Type, CompileServer);
        }

        public static void Apply(ExcessCompilation compilation)
        {
            compilation
                .match<ClassDeclarationSyntax>(isConcurrentClass)
                    .then(jsConcurrentClass)
                .match<ClassDeclarationSyntax>(isConcurrentObject)
                    .then(jsConcurrentObject);
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
            public ExpressionSyntax Connection { get; set; }
            public IList<ServerModel> Nodes { get; private set; }
            public IList<StatementSyntax> DeployStatements { get; private set; }
            public IList<StatementSyntax> StartStatements { get; private set; }
            public IList<TypeSyntax> HostedClasses { get; private set; }
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
                        }

                        throw new NotImplementedException();
                    });

            //add a method per node which will be invoked when starting each node 
            configurationClass = configurationClass
                .AddMembers(mainServer
                    .Nodes
                    .Select(serverNode => Templates
                        .NodeMethod
                        .Get<MethodDeclarationSyntax>(serverNode.ServerId)
                        .AddBodyStatements(serverNode
                            .StartStatements
                            .ToArray()))
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
                    SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList<ExpressionSyntax>(
                    hostedInNodes)));

            //the nodes themselves
            var serverNodes = Templates
                .NodeArray
                .WithInitializer(CSharp.InitializerExpression(
                    SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList<ExpressionSyntax>(
                    result.Nodes
                        .Select(node => node.Connection))));

            //the web server
            result
                .StartStatements
                .Add(Templates
                    .HttpServer
                    .Get<StatementSyntax>(
                        result.Url,
                        result.Threads,
                        hostedInstances,
                        serverNodes));

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
                var nodeInstances = Templates
                    .TypeArray
                    .WithInitializer(CSharp.InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression, CSharp.SeparatedList<ExpressionSyntax>(
                        result
                            .HostedClasses
                            .Select(type => CSharp.TypeOfExpression(type)))));

                result
                    .StartStatements
                    .Add(Templates
                        .NodeServer
                        .Get<StatementSyntax>(
                            result.Url,
                            result.Threads,
                            nodeInstances,
                            creation.Type));

                return true;
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

        //generation
        private static bool isConcurrentObject(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static void jsConcurrentObject(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static bool isConcurrentClass(ClassDeclarationSyntax arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }

        private static void jsConcurrentClass(SyntaxNode arg1, SemanticModel arg2, Scope arg3)
        {
            throw new NotImplementedException();
        }
    }
}
