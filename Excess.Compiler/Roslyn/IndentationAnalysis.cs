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
        protected override IndentationGrammarMatchBase<SyntaxToken, SyntaxNode> createMatch(Func<string, SyntaxNode> handler)
        {
            return new RoslynIndentationGrammarMatch(handler);
        }

        protected override IIndentationGrammarTransform<SyntaxNode> createTransform(IndentationNode node, Scope scope)
        {
            return new RoslynIndentationGrammarTransform(this, node);
        }

        protected override IndentationNode parse(LexicalExtension<SyntaxToken> data)
        {
            return IndentationParser.Parse(data.Body);
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

        public SyntaxNode transform(SyntaxNode node, Scope scope)
        {
            var result = null as SyntaxNode;
            var matchers = _owner.matchers();
            foreach (var matcher in matchers)
            {
                result = matcher.matches(_root, scope);
                if (result == null)
                    continue;


            }

            return result;
        }
    }

    public class RoslynIndentationGrammarMatch : IndentationGrammarMatchBase<SyntaxToken, SyntaxNode>
    {
        public RoslynIndentationGrammarMatch(Func<string, SyntaxNode> matcher) : base(matcher)
        {
        }

        protected override IndentationGrammarAnalysisBase<SyntaxToken, SyntaxNode> createChildren(Func<SyntaxNode, IEnumerable<SyntaxNode>, SyntaxNode> transform)
        {
            //td: !!! use transform
            return new RoslynIndentationGrammarAnalysis();
        }
    }
}
