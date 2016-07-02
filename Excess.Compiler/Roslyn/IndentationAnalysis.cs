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
        public RoslynIndentationGrammarAnalysis(ILexicalAnalysis<SyntaxToken, SyntaxNode, SemanticModel> owner, string keyword, ExtensionKind kind) : base(owner, keyword, kind)
        {
        }

        public override IndentationGrammarMatchBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> newMatch<T>(Func<string, object, Scope, T> handler)
        {
            return new RoslynIndentationGrammarMatch<GNode, GRoot>(this, (text, parent, scope) => handler(text, parent, scope));
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

    public class RoslynIndentationGrammarMatch<GNode, GRoot> : IndentationGrammarMatchBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> where GRoot : GNode, new()
    {
        public RoslynIndentationGrammarMatch(IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> owner, Func<string, object, Scope, object> matcher):
            base(owner, matcher)
        {
        }

        protected override IndentationGrammarMatchChildrenBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> newChildrenMatch(IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> owner)
        {
            return new RoslynIndentationGrammarMatchChildren<GNode, GRoot>(owner);
        }
    }

    public class RoslynIndentationGrammarMatchChildren<GNode, GRoot> : IndentationGrammarMatchChildrenBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> where GRoot : GNode, new ()
    {
        public RoslynIndentationGrammarMatchChildren(IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel, GNode, GRoot> owner) : base(owner)
        {
        }
    }
}
