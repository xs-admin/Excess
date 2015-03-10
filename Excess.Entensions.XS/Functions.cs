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

namespace Excess.Entensions.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Functions
    {
        static public void Apply(ExcessCompiler compiler)
        {
            var lexical   = compiler.Lexical();
            var semantics = compiler.Semantics();

            lexical
                .match() //lambda
                    .any('(', '=', ',')
                    .token("function", named: "fn")
                    .enclosed('(', ')')
                    .token('{', named: "brace")
                    .then(compiler.Lexical().transform()
                        .remove("fn")
                        .insert("=>", before: "brace"))
                .match()
                    .token("function", named: "fn") //declarations
                    .identifier(named: "id")
                    .enclosed('(', ')')
                    .token('{')
                    .then(lexical.transform()
                        .remove("fn")
                        .then(ProcessMemberFunction, referenceToken: "id"));
            semantics
                .error("CS0246", FunctionType);
        }

        private static SyntaxNode ProcessMemberFunction(SyntaxNode node, Scope scope)
        {
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();

            if (node is MethodDeclarationSyntax)
            {
                var method = node as MethodDeclarationSyntax;
                if (method.ReturnType.IsMissing)
                {
                    document.change(method, ReturnType);
                    return method.WithReturnType(RoslynCompiler.@void); 
                }

                return node;
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

        private static SyntaxNode ReturnType(SyntaxNode node, SyntaxNode newNode, SemanticModel model, Scope scope)
        {
            var method = (MethodDeclarationSyntax)node;
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
    }
}
