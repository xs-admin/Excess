using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Linq;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynIndentationGrammarAnalysis<GNode, GRoot> : IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> where GRoot : GNode, new()
    {
        protected override IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> createChildren()
        {
            return new RoslynIndentationGrammarAnalysis<GNode, GRoot>();
        }

        protected override T parseNode<T>(string text)
        {
            throw new NotImplementedException();
        }
    }
}
