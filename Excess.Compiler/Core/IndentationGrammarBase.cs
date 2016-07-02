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

    public abstract class IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> : IIndentationGrammarAnalysis<TToken, TNode, GNode> where TRoot : GNode, new ()
    {
        IGrammarAnalysis<GNode, TToken, TNode> _grammar;
        public IndentationGrammarAnalysisBase(ILexicalAnalysis<TToken, TNode, TModel> owner, string keyword, ExtensionKind kind)
        {
            _grammar = owner.grammar<IndentedGrammar, GNode>(keyword, kind, new IndentedGrammar(
                (tokens, scope) =>
                {
                    var indentRoot = parse(tokens, scope);
                    var root = new TRoot();
                    foreach (var child in indentRoot.Children)
                    {
                        var matched = false;
                        foreach (var matcher in _matchers)
                        {
                            var matchResult = matcher.transform(root, child, scope);
                            if (matchResult != null)
                            {
                                matched = true;
                                break;
                            }
                        }

                        if (!matched)
                            return default(GNode);
                    }

                    return root;
                }));
        }


        List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot>> _matchers = new List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot>>();
        public IIndentationGrammarMatch<TToken, TNode, GNode> match<TParent, T>(Func<string, TParent, Scope, T> handler) where T : GNode
        {
            var matcher = newMatch((text, parent, scope) => handler(text, (TParent)parent, scope));
            _matchers.Add(matcher);
            return matcher;
        }

        public IGrammarAnalysis<GNode, TToken, TNode> then() 
        {
            return _grammar;
        }

        class IndentedGrammar : IGrammar<TToken, TNode, GNode>
        {
            Func<IEnumerable<TToken>, Scope, GNode> _parse;
            public IndentedGrammar(Func<IEnumerable<TToken>, Scope, GNode> parse)
            {
                _parse = parse;
            }

            public GNode Parse(IEnumerable<TToken> tokens, Scope scope, int offset) => _parse(tokens, scope);
        }

        //implementors
        protected abstract T parseNode<T>(string text) where T : TNode;
        protected abstract IndentationNode parse(IEnumerable<TToken> data, Scope scope);

        public abstract IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot> newMatch<T>(Func<string, object, Scope, T> handler);
    }

    public abstract class IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot> : IIndentationGrammarMatch<TToken, TNode, GNode> where TRoot : GNode, new()
    {
        Func<string, object, Scope, object> _matcher;
        IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> _owner;
        public IndentationGrammarMatchBase(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> owner, Func<string, object, Scope, object> matcher)
        {
            _matcher = matcher;
            _owner = owner;
        }

        public IndentationGrammarMatchBase(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> owner)
        {
            _owner = owner;
        }

        List<IndentationGrammarMatchChildrenBase<TToken, TNode, TModel, GNode, TRoot>> _children = new List<IndentationGrammarMatchChildrenBase<TToken, TNode, TModel, GNode, TRoot>>();
        public IIndentationGrammarAnalysis<TToken, TNode, GNode> children(Action<IIndentationGrammarMatchChildren<TToken, TNode, GNode>> builder)
        {
            var matcher = newChildrenMatch(_owner);
            builder(matcher);
            _children.Add(matcher);
            return _owner;
        }

        //internals 
        public object transform(object parent, IndentationNode node, Scope scope)
        {
            var result = _matcher(node.Value, parent, scope);
            if (result == null)
                return default(TNode);

            foreach (var child in node.Children)
            {
                var matched = false;
                foreach (var childMatcher in _children)
                {
                    var childResult = childMatcher.transform(result, child, scope);
                    if (childResult != null)
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    return null;
            }

            return result;
        }

        //for implementors
        protected abstract IndentationGrammarMatchChildrenBase<TToken, TNode, TModel, GNode, TRoot> newChildrenMatch(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> owner);
    }

    public class IndentationGrammarMatchChildrenBase<TToken, TNode, TModel, GNode, TRoot> : IIndentationGrammarMatchChildren<TToken, TNode, GNode> where TRoot : GNode, new()
    {
        IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> _owner;
        public IndentationGrammarMatchChildrenBase(IndentationGrammarAnalysisBase<TToken, TNode, TModel, GNode, TRoot> owner)
        {
            _owner = owner;
        }

        List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot>> _matchers = new List<IndentationGrammarMatchBase<TToken, TNode, TModel, GNode, TRoot>>();
        public IIndentationGrammarMatch<TToken, TNode, GNode> match<TParent, T>(Func<string, TParent, Scope, T> handler) where T : GNode
        {
            var match = _owner.newMatch<T>((text, parent, scope) => handler(text, (TParent)parent, scope));
            _matchers.Add(match);
            return match;
        }

        //internals
        public object transform(object parent, IndentationNode child, Scope scope)
        {
            foreach (var matcher in _matchers)
            {
                var result = matcher.transform(parent, child, scope);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}