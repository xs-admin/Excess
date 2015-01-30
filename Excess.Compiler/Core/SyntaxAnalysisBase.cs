using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntacticalMatch<TToken, TNode, TModel> : ISyntacticalMatch<TToken, TNode, TModel>
    {
        ISyntaxAnalysis<TToken, TNode, TModel> _syntax;

        public BaseSyntacticalMatch(ISyntaxAnalysis<TToken, TNode, TModel> syntax)
        {
            _syntax = syntax;
        }

        private List<Func<TNode, Scope, bool>> _matchers = new List<Func<TNode, Scope, bool>>();
        public void addMatcher(Func<TNode, bool> matcher)
        {
            _matchers.Add((node, scope) => matcher(node));
        }

        public void addMatcher(Func<TNode, Scope, bool> matcher)
        {
            _matchers.Add(matcher);
        }

        public void addMatcher<T>(Func<T, Scope, bool> matcher) where T : TNode
        {
            _matchers.Add((node, scope) => node is T && matcher((T)node, scope));
        }

        protected abstract IEnumerable<TNode> children(TNode node);
        protected abstract IEnumerable<TNode> descendants(TNode node);

        private Func<TNode, Scope, bool> MatchChildren(Func<TNode, bool> selector, string named)
        {
            return (node, scope) =>
            {
                var nodes = children(node)
                    .Where(childNode => selector(childNode));

                if (named != null)
                    scope.set(named, nodes);

                return true;
            };
        }

        private Func<TNode, Scope, bool> MatchDescendants(Func<TNode, bool> selector, string named)
        {
            return (node, scope) =>
            {
                var nodes = descendants(node)
                    .Where(childNode => selector(childNode));

                if (named != null)
                    scope.set(named, nodes);

                return true;
            };
        }

        public ISyntacticalMatch<TToken, TNode, TModel> children(Func<TNode, bool> selector, string named)
        {
            _matchers.Add(MatchChildren(selector, named));
            return this;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> children<T>(Func<T, bool> selector, string named) where T : TNode
        {
            _matchers.Add(MatchChildren(node => (node is T) && selector((T)node), named));
            return this;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> descendants(Func<TNode, bool> selector, string named)
        {
            _matchers.Add(MatchDescendants(selector, named));
            return this;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> descendants<T>(Func<T, bool> selector, string named) where T : TNode
        {
            if (selector != null)
                _matchers.Add(MatchDescendants(node => (node is T) && selector((T)node), named));
            else
                _matchers.Add(MatchDescendants(node => node is T, named));

            return this;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode> handler)
        {
            Debug.Assert(_then == null);
            _then = new FunctorSyntaxTransform<TNode>(handler);
            return _syntax;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler)
        {
            Debug.Assert(_then == null);
            _then = new FunctorSyntaxTransform<TNode>(handler);
            return _syntax;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(ISyntaxTransform<TNode> transform)
        {
            Debug.Assert(_then == null);
            _then = transform;
            return _syntax;
        }

        public bool matches(TNode node, Scope scope)
        {
            foreach (var matcher in _matchers)
            {
                if (!matcher(node, scope))
                    return false;
            }

            return true;
        }

        ISyntaxTransform<TNode> _then;
    }

    public abstract class BaseSyntaxAnalysis<TToken, TNode, TModel> : ISyntaxAnalysis<TToken, TNode, TModel>
    {
        private Func<IEnumerable<TNode>, TNode> _looseMembers;
        private Func<IEnumerable<TNode>, TNode> _looseStatements;
        private Func<IEnumerable<TNode>, TNode> _looseTypes;

        public ISyntaxAnalysis<TToken, TNode, TModel> looseMembers(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseMembers = handler;
            return this;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> looseStatements(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseStatements = handler;
            return this;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> looseTypes(Func<IEnumerable<TNode>, TNode> handler)
        {
            _looseTypes = handler;
            return this;
        }

        List<SyntacticalExtension<TNode>> _extensions = new List<SyntacticalExtension<TNode>>();

        public ISyntaxAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, Scope, SyntacticalExtension<TNode>, TNode> handler)
        {
            _extensions.Add(new SyntacticalExtension<TNode>(keyword, kind, handler));
            return this;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, SyntacticalExtension<TNode>, TNode> handler)
        {
            _extensions.Add(new SyntacticalExtension<TNode>(keyword, kind, (node, scope, ext) => handler(node, ext)));
            return this;
        }

        protected abstract ISyntacticalMatch<TToken, TNode, TModel> createMatch(Func<TNode, bool> selector);

        List<ISyntacticalMatch<TToken, TNode, TModel>> _matchers = new List<ISyntacticalMatch<TToken, TNode, TModel>>();
        public ISyntacticalMatch<TToken, TNode, TModel> match(Func<TNode, bool> selector)
        {
            var matcher = createMatch(selector);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> match<T>(Func<T, bool> selector) where T : TNode
        {
            var matcher = createMatch(node => node is T && selector((T)node));
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> match<T>() where T : TNode
        {
            var matcher = createMatch(node => node is T);
            _matchers.Add(matcher);

            return matcher;
        }

        protected abstract ISyntaxTransform<TNode> createTransform();
        protected abstract ISyntaxTransform<TNode> createTransform(Func<TNode, Scope, IEnumerable<TNode>, TNode> handler);

        public ISyntaxTransform<TNode> transform()
        {
            return createTransform(); 
        }

        public ISyntaxTransform<TNode> transform(Func<TNode, TNode> handler)
        {
            return transform((node, scope) => handler(node));
        }

        public ISyntaxTransform<TNode> transform(Func<TNode, Scope, TNode> handler)
        {
            return transform(handler);
        }
    }
}
