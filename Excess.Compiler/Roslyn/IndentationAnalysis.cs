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
        public override IIndentationGrammarTransform<SyntaxNode> transform(SyntaxNode node, Scope scope, LexicalExtension<SyntaxToken> data)
        {
            var root = IndentationParser.Parse(data.Body);
            return new RoslynIndentationGrammarTransform(this, root);
        }

        protected override IndentationGrammarMatchBase<SyntaxToken, SyntaxNode> createMatch(Func<string, SyntaxNode> handler)
        {
            return new RoslynIndentationGrammarMatch(handler);
        }

        protected override T parseNode<T>(string text)
        {
            throw new NotImplementedException();
        }
    }

    public class RoslynIndentationGrammarTransform : IIndentationGrammarTransform<SyntaxNode>
    {
        RoslynIndentationGrammarAnalysis _owner;
        IndentationNode _root;
        public RoslynIndentationGrammarTransform(RoslynIndentationGrammarAnalysis owner, IndentationNode root)
        {
            _owner = owner;
            _root = root;
        }

        public SyntaxNode transform()
        {
            throw new NotImplementedException();
        }
    }

    public class RoslynIndentationGrammarMatch : IndentationGrammarMatchBase<SyntaxToken, SyntaxNode>
    {
        public RoslynIndentationGrammarMatch(Func<string, SyntaxNode> matcher) : base(matcher)
        {
        }

        protected override IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode> createChildren()
        {
            throw new NotImplementedException();
        }
    }
}
