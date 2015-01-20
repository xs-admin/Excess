using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntacticalMatchResult<TNode> : ISyntacticalMatchResult<TNode>
    {
        public TNode Node { get; set; }
        public Scope Scope { get; set; }
        public IEventBus Events { get; set; }
        public bool Preprocess { get; set; }

        ISyntacticalMatch<TNode> _matchChildren;
        public void matchChildren(ISyntacticalMatch<TNode> match)
        {
            _matchChildren = match;
        }

        ISyntacticalMatch<TNode> _matchDescendants;
        public void matchDescendants(ISyntacticalMatch<TNode> match)
        {
            _matchDescendants = match;
        }

        ExpandoObject _context;
        public dynamic context()
        {
            if (_context == null && Node != null)
            {
                _context = new ExpandoObject();
                if (_matchChildren != null)
                    applyMatch(children(Node), _matchChildren, this);

                if (_matchDescendants != null)
                    applyMatch(descendants(Node), _matchDescendants, this);
            }

            return _context;
        }

        public void set(string name, object value)
        {
            var dict = _context as IDictionary<string, object>;
            Debug.Assert(dict != null);

            dict[name] = value;
        }

        public void add(string name, object value)
        {
            var dict = _context as IDictionary<string, object>;
            Debug.Assert(dict != null);

            object current = dict[name];
            if (current == null)
            {
                var arr = new List<object>();
                arr.Add(value);
                dict[name] = arr;
            }
            else
            {
                var arr = (List<object>)current;
                arr.Add(value);
            }
        }

        public object get(string name)
        {
            var dict = _context as IDictionary<string, object>;
            Debug.Assert(dict != null);
            return dict[name];
        }

        private void applyMatch(IEnumerable<TNode> nodes, ISyntacticalMatch<TNode> match, ISyntacticalMatchResult<TNode> result)
        {
            foreach (var node in nodes)
            {

            }
        }

        public TNode schedule(string pass, TNode node, Func<TNode, TNode> handler)
        {
            int id = node.GetHashCode();
            TNode result = markNode(node, id);
            Events.schedule(pass, new SyntacticalNodeEvent<TNode>(id, handler, pass));
            return result;
        }

        protected abstract IEnumerable<TNode> children(TNode node);
        protected abstract IEnumerable<TNode> descendants(TNode node);
        protected abstract TNode markNode(TNode node, int id);
    }

    public class BaseSyntacticalMatch<TNode> : ISyntacticalMatch<TNode>
    {
        ISyntaxAnalysis<TNode>   _syntax;
        ISyntacticalMatch<TNode> _parent;
        string                   _name;
        string                   _array;

        public BaseSyntacticalMatch(ISyntaxAnalysis<TNode> syntax, ISyntacticalMatch<TNode> parent)
        {
            _syntax = syntax;
            _parent = parent;
        }

        public BaseSyntacticalMatch(ISyntaxAnalysis<TNode> syntax, string named, string add)
        {
            _syntax = syntax;
            _name   = named;
            _array  = add;
        }

        private List<Func<TNode, ISyntacticalMatchResult<TNode>, bool>> _matchers = new List<Func<TNode, ISyntacticalMatchResult<TNode>, bool>>();
        public void addMatcher(Func<TNode, bool> matcher)
        {
            _matchers.Add((node, result) => matcher(node));
        }

        public void addMatcher(Func<TNode, ISyntacticalMatchResult<TNode>, bool> matcher)
        {
            _matchers.Add(matcher);
        }

        public void addMatcher<T>(Func<T, bool> matcher) where T : TNode
        {
            _matchers.Add((node, result) => node is T && matcher((T)node));
        }

        public void addMatcher<T>(Func<T, ISyntacticalMatchResult<TNode>, bool> matcher) where T : TNode
        {
            _matchers.Add((node, result) => node is T && matcher((T)node, result));
        }

        private static Func<TNode, ISyntacticalMatchResult<TNode>, bool> MatchChildren(ISyntacticalMatch<TNode> match)
        {
            return (node, result) =>
            {
                result.matchChildren(match);
                return true;
            };
        }

        private static Func<TNode, ISyntacticalMatchResult<TNode>, bool> MatchDescendants(ISyntacticalMatch<TNode> match)
        {
            return (node, result) =>
            {
                result.matchDescendants(match);
                return true;
            };
        }

        public ISyntacticalMatch<TNode> children()
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            _matchers.Add(MatchChildren(result));
            return result;
        }

        public ISyntacticalMatch<TNode> children(Func<TNode, bool> handler)
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => handler(node));

            _matchers.Add(MatchChildren(result));
            return result;
        }

        public ISyntacticalMatch<TNode> children<T>(Func<T, bool> handler) where T : TNode
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchChildren(result));

            return result;
        }

        public ISyntacticalMatch<TNode> descendants()
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntacticalMatch<TNode> descendants(Func<TNode, bool> handler)
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => handler(node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntacticalMatch<TNode> descendants<T>(Func<T, bool> handler) where T : TNode
        {
            BaseSyntacticalMatch<TNode> result = new BaseSyntacticalMatch<TNode>(_syntax, this);
            result.addMatcher(node => (node is T) && handler((T)node));
            _matchers.Add(MatchDescendants(result));

            return result;
        }

        public ISyntacticalMatch<TNode> parent()
        {
            return _parent;
        }

        public ISyntaxAnalysis<TNode> then(Func<TNode, TNode> handler)
        {
            Debug.Assert(_then == null);
            _then = new FunctorSyntaxTransform<TNode>(handler);
            return _syntax;
        }

        public ISyntaxAnalysis<TNode> then(Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            Debug.Assert(_then == null);
            _then = new FunctorSyntaxTransform<TNode>(handler);
            return _syntax;
        }

        public ISyntaxAnalysis<TNode> then(ISyntaxTransform<TNode> transform)
        {
            Debug.Assert(_then == null);
            _then = transform;
            return _syntax;
        }

        public bool matches(TNode node, ISyntacticalMatchResult<TNode> result)
        {
            foreach (var matcher in _matchers)
            {
                if (!matcher(node, result))
                    return false;
            }

            if (_name != null)
                result.Scope.set(_name, node);

            return true;
        }

        ISyntaxTransform<TNode> _then;
        public TNode transform(TNode node, ISyntacticalMatchResult<TNode> result)
        {
            if (_then != null)
                return _then.transform(node, result);

            return node;
        }
    }

    public class SyntaxAnalysisBase<TNode> : ISyntaxAnalysis<TNode>
    {
        private Func<IEnumerable<TNode>, TNode> _looseMembers;
        private Func<IEnumerable<TNode>, TNode> _looseStatements;
        private Func<IEnumerable<TNode>, TNode> _looseTypes;

        public ISyntaxAnalysis<TNode> looseMembers(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseMembers = handler;
            return this;
        }

        public ISyntaxAnalysis<TNode> looseStatements(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseStatements = handler;
            return this;
        }

        public ISyntaxAnalysis<TNode> looseTypes(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseTypes = handler;
            return this;
        }

        List<ISyntacticalMatch<TNode>> _matchers = new List<ISyntacticalMatch<TNode>>();
        public ISyntacticalMatch<TNode> match(Func<TNode, bool> handler, string named, string add)
        {
            BaseSyntacticalMatch<TNode> matcher = new BaseSyntacticalMatch<TNode>(this, named, add);
            matcher.addMatcher(handler);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TNode> match<T>(Func<T, bool> handler, string named, string add) where T : TNode
        {
            BaseSyntacticalMatch<TNode> matcher = new BaseSyntacticalMatch<TNode>(this, named, add);
            matcher.addMatcher<T>(handler);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TNode> match<T>(string named = null, string add = null) where T : TNode
        {
            BaseSyntacticalMatch<TNode> matcher = new BaseSyntacticalMatch<TNode>(this, named, add);
            matcher.addMatcher<T>((node, result) => true);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TNode> match(string named = null, string add = null)
        {
            BaseSyntacticalMatch<TNode> matcher = new BaseSyntacticalMatch<TNode>(this, named, add);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntaxTransform<TNode> transform()
        {
            throw new NotImplementedException();
        }

        public TNode normalize(TNode node)
        {
            throw new NotImplementedException();
        }
    }
}
