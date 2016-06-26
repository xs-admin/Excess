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

        List<IndentationGrammarMatchBase<TToken, TNode>> _matchers = new List<IndentationGrammarMatchBase<TToken, TNode>>();
        public IIndentationGrammarMatch<TToken, TNode> match(Func<string, TNode> handler)
        {
            var result = createMatch(handler);
            _matchers.Add(result);
            return result;
        }

        public IIndentationGrammarMatch<TToken, TNode> match<T>(Func<string, T> parser, Func<T, TNode> transform) =>
            match(text => transform(parser(text)));

        public IIndentationGrammarMatch<TToken, TNode> match<T>(Func<T, bool> matcher) where T : TNode =>
            match(
                parser: text => parseNode<T>(text),
                transform: node => node);

        public IIndentationGrammarMatch<TToken, TNode> match(Func<string, bool> parser, Func<TNode> transform) =>
            match(text => parser(text)
                ? transform()
                : default(TNode));

        public abstract IIndentationGrammarTransform<TNode> transform(TNode node, Scope scope, LexicalExtension<TToken> data);

        protected abstract IndentationGrammarMatchBase<TToken, TNode> createMatch(Func<string, TNode> handler);
        protected abstract T parseNode<T>(string text) where T : TNode;
    }

    public abstract class IndentationGrammarMatchBase<TToken, TNode> : IIndentationGrammarMatch<TToken, TNode>
    {
        Func<string, TNode> _matcher;
        public IndentationGrammarMatchBase(Func<string, TNode> matcher)
        {
            _matcher = matcher;
        }

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

        Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> _then;
        public IIndentationGrammarAnalysis<TToken, TNode> then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            _then = handler;
            return _owner;
        }

        protected abstract IndentationGrammarAnalysisBase<TToken, TNode> createChildren();

        //internals 
        public TNode transform(string text, Scope scope, Func<IEnumerable<TNode>> children)
        {
            var node = _matcher(text);
            if (node == null)
                return node;

            //td: !! revise the signature
            return _then == null ? node : _then(node, node, scope, null);
        }
    }

}