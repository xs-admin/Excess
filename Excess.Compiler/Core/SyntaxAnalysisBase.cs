using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntacticalMatch<TToken, TNode, TModel> : ISyntacticalMatch<TToken, TNode, TModel>,
                                                                        IDocumentInjector<TToken, TNode, TModel>
    {
        ISyntaxAnalysis<TToken, TNode, TModel> _syntax;
        string _when;
        public BaseSyntacticalMatch(ISyntaxAnalysis<TToken, TNode, TModel> syntax, string when)
        {
            _syntax = syntax;
            _when = when;
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

        Func<TNode, Scope, TNode> _syntactical;
        Func<TNode, TNode, TModel, Scope, TNode> _semantical;
        public ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode> handler)
        {
            return then((node, scope) => handler(node));
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(ISyntaxTransform<TNode> transform)
        {
            return then((node, scope) => transform.transform(node, scope));
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler)
        {
            Debug.Assert(_syntactical == null && _semantical == null);
            _syntactical = handler;
            return _syntax;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler)
        {
            Debug.Assert(_syntactical == null && _semantical == null);
            _semantical = handler;
            return _syntax;
        }

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            document.change(transform, _when);
        }

        private TNode transform(TNode node, Scope scope)
        {
            if (_syntactical == null)
                return node;

            foreach (var matcher in _matchers)
            {
                if (!matcher(node, scope))
                    return node;
            }

            if (_syntactical != null)
                return _syntactical(node, scope);

            Debug.Assert(_semantical != null);
            var document = scope.GetDocument<TToken, TNode, TModel>();
            return document.change(node, _semantical);
        }
    }

    public abstract class BaseSyntaxAnalysis<TToken, TNode, TModel> : ISyntaxAnalysis<TToken, TNode, TModel>,
                                                                      IDocumentInjector<TToken, TNode, TModel>
    {
        protected List<SyntacticalExtension<TNode>> _extensions = new List<SyntacticalExtension<TNode>>();

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

        protected abstract ISyntacticalMatch<TToken, TNode, TModel> createMatch(Func<TNode, bool> selector, string when);

        protected List<ISyntacticalMatch<TToken, TNode, TModel>> _matchers = new List<ISyntacticalMatch<TToken, TNode, TModel>>();

        public ISyntacticalMatch<TToken, TNode, TModel> match(Func<TNode, bool> selector, string when)
        {
            var matcher = createMatch(selector, when);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> match<T>(Func<T, bool> selector, string when) where T : TNode
        {
            var matcher = createMatch(node => node is T && selector((T)node), when);
            _matchers.Add(matcher);

            return matcher;
        }

        public ISyntacticalMatch<TToken, TNode, TModel> match<T>(string when) where T : TNode
        {
            var matcher = createMatch(node => node is T, when);
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
            return createTransform((node, scope, childre) => handler(node, scope));
        }

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            if (_extensions.Any())
                document.change(extensions, "syntactical-extensions");

            foreach(var matcher in _matchers)
            {
                var handler = matcher as IDocumentInjector<TToken, TNode, TModel>;
                Debug.Assert(handler != null);

                handler.apply(document);
            }
        }

        protected abstract TNode extensions(TNode node, Scope scope);
    }
}
