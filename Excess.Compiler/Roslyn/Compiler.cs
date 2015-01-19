using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class Compiler : CompilerBase<SyntaxToken, SyntaxNode>
    {
        public Compiler() : base(new RoslynLexicalAnalysis(), new SyntaxAnalysisBase<SyntaxNode>())
        {
        }

        string                   _text;
        IEnumerable<SyntaxToken> _tokens;

        protected override CompilerPassResult LexicalPass(IEnumerable<ILexicalMatch<SyntaxToken>> matchers)
        {
            if (_tokens == null)
            {
                _tokens = CSharp.ParseTokens(_text);
            }

            SyntaxToken[] flatTokens = _tokens.ToArray();
            _tokens = transformTokens(flatTokens, 0, flatTokens.Length, matchers);

            return CompilerPassResult.Success; 
        }

        private static IEnumerable<SyntaxToken> Range(SyntaxToken[] tokens, int begin, int end)
        {
            for (int i = begin; i < end; i++)
                yield return tokens[i];
        }

        private IEnumerable<SyntaxToken> transformTokens(SyntaxToken[] tokens, int begin, int end, IEnumerable<ILexicalMatch<SyntaxToken>> matchers)
        {
            for (int token = 0; token < end; token++)
            {
                IEnumerable<SyntaxToken> transformed = null;
                int consumed = 0;
                foreach (var matcher in matchers)
                {
                    transformed = matcher.transform(Range(tokens, token, end), out consumed);
                    if (transformed != null)
                        break;
                }

                if (transformed == null)
                    yield return tokens[token];
                else
                {
                    foreach(var tt in transformed)
                        yield return tt;

                    token += consumed;
                }
            }
        }

        SyntaxTree _tree;
        protected override CompilerPassResult SyntacticalPass(IEnumerable<ISyntacticalMatch<SyntaxNode>> matchers)
        {
            if (_tree == null)
            {
                Debug.Assert(_tokens != null);
                _tree = ParseTokens(_tokens);
            }

            SyntaxRewriter pass = new SyntaxRewriter(matchers);
            var transformed = pass.Visit(_tree.GetRoot());
            _tree = transformed.SyntaxTree;
            return CompilerPassResult.Success;
        }

        private SyntaxTree ParseTokens(IEnumerable<SyntaxToken> tokens)
        {
            StringBuilder newText = new StringBuilder();
            foreach (var token in tokens)
                newText.Append(token.ToFullString());

            return CSharp.ParseCompilationUnit(newText.ToString()).SyntaxTree;
        }
    }
}
