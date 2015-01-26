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

        public BaseSyntacticalMatchResult(TNode node, Scope scope, IEventBus events)
        {
            Node = node;
            Scope = scope;
            Events = events;
        }

        public dynamic context()
        {
            return Scope;
        }

        public TNode schedule(string pass, TNode node, Func<TNode, TNode> handler)
        {
            string id;
            TNode result = markNode(node, out id);
            Events.schedule(pass, new SyntacticalNodeEvent<TNode>(id, handler, pass));
            return result;
        }

        protected abstract TNode markNode(TNode node, out string id);
    }

    public abstract class BaseSyntacticalMatch<TNode> : ISyntacticalMatch<TNode>
    {
        ISyntaxAnalysis<TNode> _syntax;

        public BaseSyntacticalMatch(ISyntaxAnalysis<TNode> syntax)
        {
            _syntax = syntax;
        }

        private List<Func<ISyntacticalMatchResult<TNode>, bool>> _matchers = new List<Func<ISyntacticalMatchResult<TNode>, bool>>();
        public void addMatcher(Func<TNode, bool> matcher)
        {
            _matchers.Add(result => matcher(result.Node));
        }

        public void addMatcher(Func<ISyntacticalMatchResult<TNode>, bool> matcher)
        {
            _matchers.Add(matcher);
        }

        public void addMatcher<T>(Func<T, bool> matcher) where T : TNode
        {
            _matchers.Add( result => result.Node is T && matcher((T)result.Node));
        }

        public void addMatcher<T>(Func<ISyntacticalMatchResult<TNode>, bool> matcher) where T : TNode
        {
            _matchers.Add(result => result.Node is T && matcher(result));
        }

        protected abstract IEnumerable<TNode> children(TNode node);
        protected abstract IEnumerable<TNode> descendants(TNode node);

        private Func<ISyntacticalMatchResult<TNode>, bool> MatchChildren(Func<TNode, bool> selector, string named)
        {
            return result =>
            {
                var nodes = children(result.Node)
                    .Where(node => selector(node));

                if (named != null)
                    result.Scope.set(named, nodes);

                return true;
            };
        }

        private Func<ISyntacticalMatchResult<TNode>, bool> MatchDescendants(Func<TNode, bool> selector, string named)
        {
            return result =>
            {
                var nodes = descendants(result.Node)
                    .Where(node => selector(node));

                if (named != null)
                    result.Scope.set(named, nodes);

                return true;
            };
        }

        public ISyntacticalMatch<TNode> children(Func<TNode, bool> selector, string named)
        {
            _matchers.Add(MatchChildren(selector, named));
            return this;
        }

        public ISyntacticalMatch<TNode> children<T>(Func<T, bool> selector, string named) where T : TNode
        {
            _matchers.Add(MatchChildren(node => (node is T) && selector((T)node), named));
            return this;
        }

        public ISyntacticalMatch<TNode> descendants(Func<TNode, bool> selector, string named)
        {
            _matchers.Add(MatchDescendants(selector, named));
            return this;
        }

        public ISyntacticalMatch<TNode> descendants<T>(Func<T, bool> selector, string named) where T : TNode
        {
            if (selector != null)
                _matchers.Add(MatchDescendants(node => (node is T) && selector((T)node), named));
            else
                _matchers.Add(MatchDescendants(node => node is T, named));

            return this;
        }

        public ISyntaxAnalysis<TNode> then(Func<TNode, TNode> handler)
        {
            Debug.Assert(_then == null);
            _then = new FunctorSyntaxTransform<TNode>(handler);
            return _syntax;
        }

        public ISyntaxAnalysis<TNode> then(Func<ISyntacticalMatchResult<TNode>, TNode> handler)
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
                result.Node = node;
                if (!matcher(result))
                    return false;
            }


            return true;
        }

        ISyntaxTransform<TNode> _then;
        public TNode transform(TNode node, ISyntacticalMatchResult<TNode> result)
        {
            if (_then != null)
            {
                result.Node = node;
                return _then.transform(result);
            }

            return node;
        }
    }

    public abstract class BaseSyntaxAnalysis<TNode> : ISyntaxAnalysis<TNode>
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

        List<SyntacticExtensionEvent<TNode>> _extensions = new List<SyntacticExtensionEvent<TNode>>();

        public ISyntaxAnalysis<TNode> extension(string keyword, ExtensionKind kind, Func<ISyntacticalMatchResult<TNode>, SyntacticalExtension<TNode>, IEnumerable<TNode>> handler)
        {
            _extensions.Add(new SyntacticExtensionEvent<TNode>(keyword, kind, handler));
            return this;
        }

        public ISyntaxAnalysis<TNode> extension(string keyword, ExtensionKind kind, Func<TNode, SyntacticalExtension<TNode>, IEnumerable<TNode>> handler)
        {
            _extensions.Add(new SyntacticExtensionEvent<TNode>(keyword, kind, (result, ext) => handler(result.Node, ext)));
            return this;
        }

        protected abstract ISyntacticalMatch<TNode> createMatch(Func<TNode, bool> selector);

        List<ISyntacticalMatch<TNode>> _matchers = new List<ISyntacticalMatch<TNode>>();
        public ISyntacticalMatch<TNode> match(Func<TNode, bool> selector)
        {
            var matcher = createMatch(selector);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TNode> match<T>(Func<T, bool> selector) where T : TNode
        {
            var matcher = createMatch(node => node is T && selector((T)node));
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TNode> match<T>() where T : TNode
        {
            var matcher = createMatch(node => node is T);
            _matchers.Add(matcher);

            return matcher;
        }

        protected abstract ISyntaxTransform<TNode> createTransform();
        protected abstract ISyntaxTransform<TNode> createTransform(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode> handler);

        public ISyntaxTransform<TNode> transform()
        {
            return createTransform(); 
        }

        public ISyntaxTransform<TNode> transform(Func<TNode, TNode> handler)
        {
            return transform(result => handler(result.Node));
        }

        public ISyntaxTransform<TNode> transform(Func<ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            return createTransform((result, children) => handler(result));
        }

        public IEnumerable<CompilerEvent> produce()
        {
            IEnumerable<CompilerEvent> matchEvents =  new[] { new SyntacticalMatchEvent<TNode>(_matchers) };
            return matchEvents.Union(_extensions);
        }
    }
}
