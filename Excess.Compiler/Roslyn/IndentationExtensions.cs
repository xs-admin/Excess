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
        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self, 
            string pattern,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null) where T : GNode, new()
        {
            var parser = RazorParser.Create<T>(pattern); //td: scope for errors?
            return self.match<TParent, T>((text, parent, scope) =>
            {
                return parser(text, scope);
            }, children);
        }

        //public static IIndentationGrammarMatch<SyntaxToken, SyntaxNode, T> match<TParent, T>(
        //this IIndentationGrammarMatchChildren<SyntaxToken, SyntaxNode, T> self,
        //Regex pattern) where T : new()
        //{
        //    var parser = RegexParser.Create<T>(pattern); //td: scope for errors?
        //    return self.match<TParent, T>((text, parent, scope) =>
        //    {
        //        return parser(text, scope);
        //    });
        //}
    }
}
