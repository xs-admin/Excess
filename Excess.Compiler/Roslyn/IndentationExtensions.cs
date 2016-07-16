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
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class IndentationExtensions
    {
        //default
        private static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            Func<string, Scope, T> parser,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children,
            Action<TParent, T> then) where T : GNode, new()
        {
            return self.match<TParent, T>((text, parent, scope) =>
            {
                var result = parser(text, scope);
                if (result != null && then != null)
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

        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            IEnumerable<string> patterns,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new()
        {
            foreach (var pattern in patterns)
            {
                match<GNode, TParent, T>(self, RazorParser.Create<T>(pattern), children, then);
            }

            return self;
        }

        //regex
        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            Regex pattern,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new()
                => match<GNode, TParent, T>(self, RegexParser.Create<T>(pattern), children, then);

        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            IEnumerable<Regex> patterns,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null,
            Action<TParent, T> then = null) where T : GNode, new()
        {
            foreach (var pattern in patterns)
            {
                match<GNode, TParent, T>(self, RegexParser.Create<T>(pattern), children, then);
            }

            return self;
        }

        //roslyn
        public static IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> match<GNode, TParent, T>(
            this IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode> self,
            Action<TParent, T> then,
            Action<IIndentationGrammarAnalysis<SyntaxToken, SyntaxNode, GNode>> children = null) 
                where T : SyntaxNode 
                where GNode : new()
        {
            return self.match<TParent, GNode>((text, parent, scope) =>
            {
                var expr = CSharp.ParseExpression(text) as T;
                var statement = expr == null
                    ? CSharp.ParseStatement(text) as T
                    : default(T);

                var result = statement ?? expr;
                if (result != null && !result.ContainsDiagnostics)
                {
                    then(parent, result);
                    return new GNode();
                }

                return default(GNode);
            }, children);
        }
    }
}
