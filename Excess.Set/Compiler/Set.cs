using Excess.Compiler;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Compiler.SetParser;
using System.Diagnostics;

namespace Compiler
{
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class SetExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("set");

            var lexical = compiler.Lexical();
            lexical
                .match()
                    .token("set", named: "keyword")
                    .enclosed("<", ">", contents: "type")
                    .identifier(named: "id")
                    .token("=")
                    .enclosed("{", "}", contents: "def")
                    .then(CompileSet);
        }

        private static IEnumerable<SyntaxToken> CompileSet(
            IEnumerable<SyntaxToken> tokens, 
            ILexicalMatchResult<SyntaxToken, SyntaxNode, SemanticModel> match, 
            Scope scope)
        {
            var item = match.Items.FirstOrDefault(x => x.Identifier == "def");
            var defSpan = item?.Span;
            if (defSpan != null)
            {
                var defTokens = match.GetTokens(tokens, defSpan);
                var result = Parser.Parse(defTokens.Skip(1).Take(defSpan.Length - 2));
                Debug.Assert(result != null);

                item = match.Items.FirstOrDefault(x => x.Identifier == "id");
                var idSpan = item?.Span;
                var newTokens = SetTokens(tokens, idSpan, scope, result);
                return newTokens;
            }

            return tokens;
        }

        private static IEnumerable<SyntaxToken> SetTokens(IEnumerable<SyntaxToken> tokens, TokenSpan span, Scope scope, SetSyntax set)
        {
            var index = 0;
            foreach (var token in tokens)
            {
                if (index >= span.Start + span.Length)
                    break;

                if (index == 0)
                {
                    var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                    yield return document.change(token, LinkSet(set)/*, "lexical-extension"*/);
                }
                else 
                    yield return token;
                index++;
            }

            yield return CSharp.Token(SyntaxKind.SemicolonToken);
        }

        private static Func<SyntaxNode, Scope, SyntaxNode> LinkSet(SetSyntax set)
        {
            return (node, scope) =>
            {
                throw new NotImplementedException();
            };
        }
    }
}
