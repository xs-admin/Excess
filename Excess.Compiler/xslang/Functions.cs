using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xslang
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Functions
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical   = compiler.Lexical();
            var semantics = compiler.Semantics();

            lexical
                //lambdas
                .match() 
                    .any('(', '=', ',')
                    .any(new[] { "function", "fn"}, named: "fn")
                    .enclosed('(', ')')
                    .token('{', named: "brace")
                        .then(compiler.Lexical().transform()
                            .remove("fn")
                            .insert("=>", before: "brace"))

                //declarations
                .match()
                    .any(new[] { "function", "fn" }, named: "fn") 
                    .identifier(named: "id")
                    .enclosed('(', ')')
                    .token('{')
                        .then(lexical.transform()
                            .remove("fn")
                            .then(ProcessMemberFunction, referenceToken: "id"))

                .match()
                    .any(new[] { "function", "fn" }, named: "fn")
                    .identifier(named: "id")
                    .enclosed('(', ')')
                    .token(':', named: "colon")
                    .until('{', named: "type")
                        .then(lexical.transform()
                            .replace("fn", null, "type")
                            .remove("colon")
                            .remove("type")
                                .then(ProcessMemberFunction, referenceToken: "id"))

                .match()
                    .token("scope", named: "keyword")
                    .token('{', named: "ref")
                    .then(lexical.transform()
                        .remove("keyword")
                        .then(ProcessScope, "ref"));

            //td: do we need this?
            //semantics
            //    .error("CS0246", FunctionType);
        }

        private static SyntaxNode ProcessMemberFunction(SyntaxNode node, Scope scope)
        {
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();

            if (node is MethodDeclarationSyntax)
            {
                var method = (node as MethodDeclarationSyntax)
                    .AddParameterListParameters(CSharp
                        .Parameter(Templates.ScopeToken)
                        .WithType(Templates.ScopeType));

                method = MemberFunctionModifiers(method);

                var service = scope.GetService<SyntaxToken, SyntaxNode, SemanticModel>();
                if (method.ReturnType.IsMissing)
                {
                    method = method.WithReturnType(RoslynCompiler.@void);

                    var calculateType = method.Body
                        .DescendantNodes()
                        .OfType<ReturnStatementSyntax>()
                        .Any();

                    var isMember = method.Parent is TypeDeclarationSyntax;
                    if (!isMember)
                    {
                        return service.MarkNode(Templates
                            .NamespaceFunction
                            .AddMembers((MemberDeclarationSyntax)document.change(
                                method,
                                LinkNamespaceFunction(calculateType))));
                    }

                    return calculateType
                        ? document.change(method, CalculateReturnType)
                        : method;
                }

                return service.MarkNode(Templates
                    .NamespaceFunction
                    .AddMembers((MemberDeclarationSyntax)document.change(
                        method,
                        LinkNamespaceFunction(false))));
            }

            //handle functions declared inside code blocks
            var statement = node
                .AncestorsAndSelf()
                .OfType<StatementSyntax>()
                .FirstOrDefault();

            Debug.Assert(statement != null); //td: error, maybe?
            Debug.Assert(statement is ExpressionStatementSyntax);

            var invocation = (statement as ExpressionStatementSyntax)
                .Expression as InvocationExpressionSyntax;
            Debug.Assert(invocation != null);

            var function = invocation.Expression as IdentifierNameSyntax;
            Debug.Assert(function != null);

            BlockSyntax parent = statement.Parent as BlockSyntax;
            Debug.Assert(parent != null); //td: error, maybe?

            var body = RoslynCompiler.NextStatement(parent, statement) as BlockSyntax;
            if (body == null)
            {
                //td: error, function declaration must be followed by a block of code
                return node;
            }

            //We are not allowed to modify parents, so schedule the removal of the code
            //And its insertion in the final lambda variable
            document.change(parent, RoslynCompiler.RemoveStatement(body));
            document.change(statement, ProcessCodeFunction(function, body));
            return node;
        }

        private static MethodDeclarationSyntax MemberFunctionModifiers(MethodDeclarationSyntax method)
        {
            var modifiers = method.Modifiers;
            if (!modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)))
                method = method.AddModifiers(CSharp.Token(SyntaxKind.StaticKeyword));

            if (!Roslyn.HasVisibilityModifier(method.Modifiers))
                method = method.AddModifiers(CSharp.Token(SyntaxKind.PublicKeyword));

            return method;
        }

        private static Func<SyntaxNode, SyntaxNode, SemanticModel, Scope, SyntaxNode> LinkNamespaceFunction(bool calculateType)
        {
            return (original, node, model, scope) =>
            {
                if (calculateType)
                    node = CalculateReturnType(original, node, model, scope);

                var calls = node
                    .DescendantNodes()
                    .OfType<ExpressionStatementSyntax>()
                    .Where(statement =>
                        statement.Expression is InvocationExpressionSyntax
                        && (statement.Expression as InvocationExpressionSyntax)
                            .Expression is IdentifierNameSyntax)
                    .Select(statement => (InvocationExpressionSyntax)statement.Expression);

                return node.ReplaceNodes(calls,
                    (on, nn) =>
                    {
                        var lastArgument = nn
                            .ArgumentList
                            .Arguments
                            .LastOrDefault();

                        if (lastArgument != null && lastArgument.ToString() == "__newScope")
                            return nn;

                        return nn.AddArgumentListArguments(
                            CSharp.Argument(Templates.ScopeIdentifier));
                    });
            };
        }

        private static SyntaxNode CalculateReturnType(SyntaxNode node, SyntaxNode newNode, SemanticModel model, Scope scope)
        {
            var method = node as MethodDeclarationSyntax;
            if (method == null)
                return node;

            var type = RoslynCompiler.GetReturnType(method.Body, model);
            return (newNode as MethodDeclarationSyntax)
                .WithReturnType(type);
        }

        private static void FunctionType(SyntaxNode node, SemanticModel model, Scope scope)
        {
            TypeSyntax newType = null;
            if (node is GenericNameSyntax)
            {
                var generic = node as GenericNameSyntax;
                if (generic.Identifier.ToString() == "function")
                {
                    List<TypeSyntax> arguments  = new List<TypeSyntax>();
                    TypeSyntax       returnType = null;

                    bool first = true;
                    foreach (var arg in generic.TypeArgumentList.Arguments)
                    {
                        if (first)
                        {
                            first = false;
                            if (arg.ToString() != "void")
                                returnType = arg;
                        }
                        else
                            arguments.Add(arg);
                    }

                    if (returnType == null)
                        newType = generic
                            .WithIdentifier(CSharp.Identifier("Action"))
                            .WithTypeArgumentList(CSharp.TypeArgumentList(CSharp.SeparatedList(
                                arguments)));
                    else
                        newType = generic
                            .WithIdentifier(CSharp.Identifier("Func"))
                            .WithTypeArgumentList(CSharp.TypeArgumentList(CSharp.SeparatedList(
                                arguments
                                    .Union(new[] { returnType}))));
                }
            }
            else if (node.ToString() == "function")
            {
                newType = CSharp.ParseTypeName("Action");
            }

            if (newType != null)
            {
                var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                document.change(node, RoslynCompiler.ReplaceNode(newType));
            }
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> ProcessCodeFunction(IdentifierNameSyntax name, BlockSyntax body)
        {
            return (node, scope) =>
            {
                LocalDeclarationStatementSyntax localDeclaration = (LocalDeclarationStatementSyntax)CSharp.ParseStatement("var id = () => {}");
                var variable = localDeclaration.Declaration.Variables[0];
                var lambda = variable.Initializer.Value as ParenthesizedLambdaExpressionSyntax;
                Debug.Assert(lambda != null);

                return localDeclaration
                    .WithDeclaration(localDeclaration
                        .Declaration
                        .WithVariables(CSharp.SeparatedList(new[] {
                                variable
                                    .WithIdentifier(name.Identifier)
                                    .WithInitializer(variable.Initializer
                                        .WithValue(lambda
                                            //.WithParameterList(invocation.ArgumentList) //td: extension arguments
                                            .WithBody(body)))})));
            };
        }

        private static SyntaxNode ProcessScope(SyntaxNode node)
        {
            var block = node as BlockSyntax;
            if (block == null)
            {
                //td: error
                return node;
            }

            return block.WithStatements(CSharp.List(
                ProcessScopeStatements(block.Statements)));
        }

        private static IEnumerable<StatementSyntax> ProcessScopeStatements(SyntaxList<StatementSyntax> statements)
        {
            yield return Templates.NewScope;

            foreach (var statement in statements)
            {
                if (statement is LocalDeclarationStatementSyntax)
                {
                    //changes to the scope, again expressed as variables
                    var localDeclaration = statement as LocalDeclarationStatementSyntax;
                    var type = localDeclaration.Declaration.Type;

                    Debug.Assert(localDeclaration.Declaration.Variables.Count == 1); //td: for now
                    var variable = localDeclaration.Declaration.Variables.Single();
                    if (variable.Initializer != null)
                    {
                        yield return statement;
                        yield return Templates.AddToNewScope
                            .Get<StatementSyntax>(
                                Roslyn.Quoted(variable.Identifier.ToString()),
                                CSharp.IdentifierName(variable.Identifier));
                    }
                    else
                    {
                        //td: error, should set values
                    }
                }
                else if (statement is ExpressionStatementSyntax)
                {
                    //invocations to happen on a different context
                    var invocation = (statement as ExpressionStatementSyntax)
                        .Expression as InvocationExpressionSyntax;

                    if (invocation != null)
                    {
                        if (invocation.Expression is MemberAccessExpressionSyntax)
                        {
                            //td: error, only namspace function calls?
                        }
                        else
                            yield return statement.ReplaceNode(invocation, invocation
                                .AddArgumentListArguments(CSharp.Argument(Templates
                                .NewScopeValue)));
                    }
                    else
                    {
                        //td: error, bad invocation
                    }
                }
                else
                {
                    //td: error, only variables and invocations
                }
            }
        }
    }
}
