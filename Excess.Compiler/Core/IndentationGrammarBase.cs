using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class IndentationNode
    {
        public IndentationNode(IndentationNode parent, int depth)
        {
            Parent = parent;
            Depth = depth;
            Children = new List<IndentationNode>();
        }

        public int Depth { get; private set; }
        public string Value { get; private set; }
        public IndentationNode Parent { get; private set; }
        public IEnumerable<IndentationNode> Children { get; private set; }

        public void SetValue(string value)
        {
            Value = value;
        }

        public void AddChild(IndentationNode result)
        {
            ((List<IndentationNode>)Children).Add(result);
        }
    }

    public abstract class IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode> : IIndentationGrammarAnalysis<TToken, TNode, GNode>
    {
        ILexicalAnalysis<TToken, TNode, TModel> _owner;
        string _keyword;
        ExtensionKind _kind;
        public IndentationGrammarAnalysisBase(ILexicalAnalysis<TToken, TNode, TModel> owner, string keyword, ExtensionKind kind)
        {
        }

        IGrammarAnalysis<GNode, TToken, TNode> _grammar;
        public IIndentationGrammarMatch<TToken, TNode, GNode> match<TRoot>(Func<string, Scope, TRoot> handler) where TRoot : GNode, new()
        {
            if (_grammar != null)
                throw new InvalidOperationException("already has a grammar");

            _grammar = _owner.grammar<Grammar, GNode>(_keyword, _kind, new Grammar(
                (tokens, scope) =>
                {
                    var indentRoot = parse(tokens, scope);
                    var root = new TRoot();
                    foreach (var child in indentRoot.Children)
                    {
                        throw new NotImplementedException();
                    }

                    return root;
                }));

            return match<object, TRoot>((text, parent, scope) => handler(text, scope));
        }

        public IIndentationGrammarMatch<TToken, TNode, GNode> match<TParent, T>(Func<string, TParent, Scope, T> handler) where T : GNode
        {
            return newMatch(this, (text, parent, scope) => handler(text, (TParent)parent, scope));
        }

        public IGrammarAnalysis<GNode, TToken, TNode> then() 
        {
            return _grammar;
        }

        class Grammar : IGrammar<TToken, TNode, GNode>
        {
            Func<IEnumerable<TToken>, Scope, GNode> _parse;
            public Grammar(Func<IEnumerable<TToken>, Scope, GNode> parse)
            {
                _parse = parse;
            }

            public GNode Parse(IEnumerable<TToken> tokens, Scope scope, int offset) => _parse(tokens, scope);
        }

        //implementors
        protected abstract T parseNode<T>(string text) where T : TNode;
        protected abstract IndentationNode parse(IEnumerable<TToken> data, Scope scope);
        protected abstract IIndentationGrammarMatch<TToken, TNode, GNode> newMatch<T>(
            IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode> owner,
            Func<string, object, Scope, T> handler);
    }

    public abstract class IndentationGrammarMatchBase<TToken, TNode, TModel, GNode> : IIndentationGrammarMatch<TToken, TNode, GNode>
    {
        Func<string, object, Scope, object> _matcher;
        IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode> _owner;
        public IndentationGrammarMatchBase(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode> owner, Func<string, object, Scope, object> matcher)
        {
            _matcher = matcher;
            _owner = owner;
        }

        public IndentationGrammarMatchBase(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode> owner)
        {
            _owner = owner;
        }

        public IIndentationGrammarAnalysis<TToken, TNode, GNode> children(Action<IIndentationGrammarMatchChildren<TToken, TNode, GNode>> builder)
        {
            var matcher = newChildrenMatch();
            builder(matcher);
            return _owner;
        }

        //internals 
        List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode>> _children = new List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode>>();
        public object transform(object parent, IndentationNode node, Scope scope)
        {
            var result = _matcher(node.Value, parent, scope);
            if (result == null)
                return default(TNode);

            foreach (var child in node.Children)
            {
                foreach (var analysis in _children)
                {
                    analysis.transform(result, child, scope);
                }
            }

            return result;
        }

        //for implementors
        protected abstract IIndentationGrammarMatchChildren<TToken, TNode, GNode> newChildrenMatch();
    }

}