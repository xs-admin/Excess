using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.XS
{
    public class XSModule
    {
        static public void Apply(RoslynCompiler compiler)
        {
            var lexical  = compiler.Lexical();
            var sintaxis = compiler.Sintaxis();

            //functions
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
                    //.token(startsMemberFunction)
                    .token("function", named: "fn")
                    .identifier(named: "id")
                    .enclosed('(', ')')
                    .token('{')
                    .then(lexical.transform()
                        .remove("fn")
                        .then("id", ProcessMemberFunction));
        }

        private static SyntaxNode ProcessUntypedFunction(SyntaxNode arg)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)arg;
            return method.WithReturnType(RoslynCompiler.@void); //td: schedule semantic
        }

        private static bool MapMemberFunction(SyntaxNode node)
        {
            return node is MethodDeclarationSyntax
                || node is StatementSyntax;
        }

        private static SyntaxNode ProcessMemberFunction(SyntaxNode node, Scope scope)
        {
            if (node is MethodDeclarationSyntax)
            {
                var method = node as MethodDeclarationSyntax;
                if (method.ReturnType.IsMissing)
                    return method.WithReturnType(RoslynCompiler.@void); //td: schedule type resolution

                return node;
            }

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
            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(parent, RoslynCompiler.RemoveStatement(body));
            document.change(statement, ProcessCodeFunction(function, body));
            return node;
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
