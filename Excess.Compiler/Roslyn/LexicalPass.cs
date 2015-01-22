using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.Text;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class LexicalPass : BaseLexicalPass<SyntaxToken, SyntaxNode>
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

        protected override ICompilerPass continuation(IEventBus events, Scope scope, string transformed, IEnumerable<PendingExtension<SyntaxToken, SyntaxNode>> extensions)
        {
            NewText = transformed;
            Root = CSharp.ParseCompilationUnit(transformed);

            foreach(var ext in extensions)
            {
                ext.Node = Root.FindNode(new TextSpan(ext.Span.Start, ext.Span.Length));
            }

            return new SyntacticalPass(Root, extensions);
        }

        protected override IEnumerable<SyntaxToken> parseTokens(string text)
        {
            //td: ! compiler service
            return CSharp.ParseTokens(text);
        }

        protected override string tokenToString(SyntaxToken token, out int lexicalId)
        {
            lexicalId = RoslynCompiler.GetLexicalId(token);
            return token.ToFullString();
        }
    }
}
