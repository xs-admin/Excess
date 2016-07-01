using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using System.Linq;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynIndentationGrammarAnalysis<GNode> : IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode>
    {
        public RoslynIndentationGrammarAnalysis(ILexicalAnalysis<SyntaxToken, SyntaxNode, SemanticModel> owner, string keyword, ExtensionKind kind) : base(owner, keyword, kind)
        {
        }

        protected override IIndentationGrammarMatch<SyntaxToken, SyntaxNode, GNode> newMatch<T>(IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode> owner, Func<string, object, Scope, T> handler)
        {
            throw new NotImplementedException();
        }

        protected override IndentationNode parse(IEnumerable<SyntaxToken> data, Scope scope)
        {
            return IndentationParser.Parse(data, 4); //td: !!!
        }

        protected override T parseNode<T>(string text)
        {
            throw new NotImplementedException();
        }
    }
}
