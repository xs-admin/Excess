using Excess.Compiler.Extrapolators;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public static class IndentationExtensions
    {
        //default
        private static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            Func<string, Scope, T> parser,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new()
        {
            return self.match<TParent, T>((text, parent, scope) =>
            {
                var result = parser(text, scope);
                if (!result.Equals(default(T)) && then != null)
                    then(parent, result);

                return result;
            }, children);
        }

        //razor-like extrapolator
        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            string pattern,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new() 
                => match<GNode, TParent, T>(self, RazorParser.Create<T>(pattern), children, then);

        //regex
        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            Regex pattern,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new()
                => match<GNode, TParent, T>(self, RegexParser.Create<T>(pattern), children, then);
    }
}
