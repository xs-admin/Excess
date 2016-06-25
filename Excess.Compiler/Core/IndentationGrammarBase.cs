using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class IndentationGrammarAnalysisBase<TToken, TNode> : IIndentationGrammarAnalysis<TToken, TNode>
    {
        List<Func<TNode, TNode, Scope, TNode>> _after = new List<Func<TNode, TNode, Scope, TNode>>();
        public IIndentationGrammarAnalysis<TToken, TNode> after(Func<TNode, TNode, Scope, TNode> handler)
        {
            _after.Add(handler);
            return this;
        }

        List<Func<TNode, Scope, LexicalExtension<TToken>, TNode>> _before = new List<Func<TNode, Scope, LexicalExtension<TToken>, TNode>>();
        public IIndentationGrammarAnalysis<TToken, TNode> before(Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            _before.Add(handler);
            return this;
        }

        public IIndentationGrammarMatch<TToken, TNode> match<T>(Func<T, bool> matcher) where T : TNode
        {
            return match(
                parser: text => parseNode<T>(text),
                transform: node => node);

        }

        public IIndentationGrammarMatch<TToken, TNode> match<T>(Func<string, T> parser, Func<T, TNode> transform)
        {
            throw new NotImplementedException();
        }

        public IIndentationGrammarMatch<TToken, TNode> match(Func<string, bool> parser, Func<TNode> transform)
        {
            throw new NotImplementedException();
        }

        public IIndentationGrammarMatch<TToken, TNode> match(Func<string, TNode> linker)
        {
            throw new NotImplementedException();
        }

        protected abstract T parseNode<T>(string text) where T : TNode;
    }

    public abstract class IndentationGrammarMatchBase<TToken, TNode> : IIndentationGrammarMatch<TToken, TNode>
    {
        IndentationGrammarAnalysisBase<TToken, TNode> _owner;
        public IndentationGrammarMatchBase(IndentationGrammarAnalysisBase<TToken, TNode> owner)
        {
            _owner = owner;
        }

        public IIndentationGrammarAnalysis<TToken, TNode> children(Action<IIndentationGrammarAnalysis<TToken, TNode>> handler)
        {
            var inner = createChildren();
            handler?.Invoke(inner);
            return _owner;
        }

        public IIndentationGrammarAnalysis<TToken, TNode> then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            return _owner;
        }

        protected abstract IndentationGrammarAnalysisBase<TToken, TNode> createChildren();
    }

}