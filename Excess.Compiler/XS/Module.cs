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
                        .remove("fn"));
                        //td: !!!
                        //.then("id", ProcessMemberFunction, 
                        //    mapper: MapMemberFunction));

            sintaxis
                .match<MethodDeclarationSyntax>(method => method.ReturnType.ToString() == "function")
                    .then(ProcessUntypedFunction)
                .extension("function", ExtensionKind.Code, ProcessCodeFunction);
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

            var statement = node as StatementSyntax;
            Debug.Assert(statement != null); //td: error, maybe?

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(statement, null, "custom-extension");
            return statement;
        }

        static private bool startsMemberFunction(SyntaxToken token)
        {
            var kind = token.CSharpKind();
            return RoslynCompiler.isLexicalIdentifier(kind) || kind == SyntaxKind.GreaterThanToken;
        }

        static private SyntaxNode ProcessCodeFunction(SyntaxNode node, Scope result, SyntacticalExtension<SyntaxNode> extension)
        {
            var expr = CSharp.ParseStatement("var " + extension.Identifier + " = () => {};");
            return expr
                .ReplaceNode(expr
                    .DescendantNodes()
                    .OfType<ParenthesizedLambdaExpressionSyntax>()
                    .First(), CSharp.ParenthesizedLambdaExpression((BlockSyntax)extension.Body));
        }
    }
}
