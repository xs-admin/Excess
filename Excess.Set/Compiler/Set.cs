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
            }

            throw new NotImplementedException();
        }
    }
}
