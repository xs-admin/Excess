using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                .match()
                    .any('(', '=', ',')
                    .token("function", named: "fn")
                    .enclosed('(', ')')
                    .token('{', named: "brace")
                    .then(compiler.Lexical().transform()
                        .remove("fn")
                        .insert("=>", before: "brace"))
                .match()
                    .token(MemberFunction)
                    .token("function", named: "fn")
                    .identifier()
                    .enclosed('(', ')')
                    .token('{')
                    .then(lexical.transform()
                        .remove("fn")
                        .then(ProcessMemberFunction));

            sintaxis
                .match<MethodDeclarationSyntax>(method => method.ReturnType.ToString() == "function")
                    .then(ProcessUntypedFunction)
                .extension("function", ExtensionKind.Code, ProcessCodeFunction);
        }

        private static SyntaxNode ProcessUntypedFunction(SyntaxNode arg)
        {
            MethodDeclarationSyntax method = (MethodDeclarationSyntax)arg;
            return method.WithReturnType(RoslynCompiler.@void); //td: schedule
        }

        private static SyntaxNode ProcessMemberFunction(SyntaxNode arg)
        {
            throw new NotImplementedException();
        }

        static private bool MemberFunction(SyntaxToken token)
        {
            var kind = token.CSharpKind();
            return RoslynCompiler.isLexicalIdentifier(kind) || kind == SyntaxKind.GreaterThanToken;
        }

        static private IEnumerable<SyntaxNode> ProcessCodeFunction(ISyntacticalMatchResult<SyntaxNode> result, SyntacticalExtension<SyntaxNode> extension)
        {
            return new[] { result.Node };
        }
    }
}
