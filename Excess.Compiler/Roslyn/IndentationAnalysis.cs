using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class RoslynIndentationGrammarAnalysis : IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode>
    {
        protected override T parseNode<T>(string text)
        {
            throw new NotImplementedException();
        }
    }

    public class RoslynIndentationParser
    {
        public static SyntaxNode Parse(SyntaxNode node, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            throw new NotImplementedException();
        }
    }
}
