using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class LexicalPass : BaseLexicalPass<SyntaxToken>
    {
        public LexicalPass(string text) :
            base(text)
        {
        }

        internal SyntaxNode Root { get; private set; }
        internal string NewText { get; private set; }

        protected override string passId()
        {
            return "lexical-pass";
        }

        protected override CompilerStage passStage()
        {
            return CompilerStage.Lexical;
        }

        protected override ICompilerPass continuation(string transformed)
        {
            NewText = transformed;
            Root = CSharp.ParseCompilationUnit(transformed);

            return new SyntacticalPass(Root);
        }

        protected override IEnumerable<SyntaxToken> parseTokens(string text)
        {
            return CSharp.ParseTokens(text);
        }

        protected override string tokensToString(IEnumerable<SyntaxToken> tokens)
        {
            StringBuilder newText = new StringBuilder();
            foreach (var token in tokens)
                newText.Append(token.ToFullString());

            return newText.ToString();
        }
    }
}
