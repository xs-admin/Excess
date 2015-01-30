using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntaxTransform<TToken, TNode, TModel> : ISyntaxTransform<TNode>
    {
        protected List<Func<TNode, Scope, IEnumerable<TNode>, TNode>> _transformers = new List<Func<TNode, Scope, IEnumerable<TNode>, TNode>>();
        protected List<Func<TNode, Scope, IEnumerable<TNode>>> _selectors = new List<Func<TNode, Scope, IEnumerable<TNode>>>();

        Func<TNode, Scope, IEnumerable<TNode>, TNode> AddToScope(bool type, bool @namespace)
        {
            return (node, scope, nodes) =>
            {
                if (nodes == null)
                    return node;

                TNode scopeNode = resolveScope(node, type, @namespace);
                if (scopeNode != null)
                {
                    var document = scope.GetDocument<TToken, TNode, TModel>();
                    return document.change(scopeNode, (n, s) => addToNode(n, nodes));
                }

                return node;
            };
        }

        Func<TNode, Scope, IEnumerable<TNode>> SelectFromScope(string nodes)
        {
            return (node, scope) =>
            {
                return scope.get<IEnumerable<TNode>>(nodes);
            };
        }

        public ISyntaxTransform<TNode> addToScope(Func<TNode, Scope, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false)
        {
            _selectors.Add(handler);
            _transformers.Add(AddToScope(type, @namespace));
            return this;
        }

        public ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(AddToScope(type, @namespace));
            return this;
        }

        Func<TNode, Scope, IEnumerable<TNode>, TNode> RemoveNodes()
        {
            return (node, scope, nodes) =>
            {
                if (nodes == null)
                    return node;

                return removeNodes(node, nodes);
            };
        }

        public ISyntaxTransform<TNode> remove(string nodes)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(RemoveNodes());
            return this;
        }

        public ISyntaxTransform<TNode> remove(Func<TNode, Scope, IEnumerable<TNode>> selector)
        {
            _selectors.Add(selector);
            _transformers.Add(RemoveNodes());
            return this;
        }

        Func<TNode, Scope, IEnumerable<TNode>, TNode> ReplaceNodes(Func<TNode, Scope , TNode> handler)
        {
            return (node, scope, nodes) =>
            {
                if (nodes == null || !nodes.Any())
                    return node;

                return replaceNodes(node, scope, nodes, handler);
            };
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(ReplaceNodes((node, scope) => handler(node)));
            return this;
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, Scope , TNode> handler)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(ReplaceNodes(handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, Scope , TNode> handler)
        {
            _selectors.Add(selector);
            _transformers.Add(ReplaceNodes(handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, TNode> handler)
        {
            _selectors.Add(selector);
            _transformers.Add(ReplaceNodes((node, scope) => handler(node)));
            return this;
        }

        private static Random _randId = new Random();

        Func<TNode, bool> _mapper;
        public ISyntaxTransform<TNode> match(Func<TNode, bool> mapper)
        {
            Debug.Assert(_mapper == null);
            _mapper = mapper;
            return this;
        }

        protected abstract TNode removeNodes(TNode node, IEnumerable<TNode> nodes);
        protected abstract TNode replaceNodes(TNode node, Scope scope, IEnumerable<TNode> nodes, Func<TNode, Scope, TNode> handler);
        protected abstract TNode addToNode(TNode node, IEnumerable<TNode> nodes);
    }

    public class FunctorSyntaxTransform<TNode> : ISyntaxTransform<TNode>
    {
        Func<TNode, TNode>         _functor;
        Func<TNode, Scope , TNode> _functorExtended;

        public FunctorSyntaxTransform(Func<TNode, TNode> handler)
        {
            _functor = handler;
        }

        public FunctorSyntaxTransform(Func<TNode, Scope , TNode> handler)
        {
            _functorExtended = handler;
        }

        public ISyntaxTransform<TNode> remove(string nodes)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> remove(Func<TNode, Scope, IEnumerable<TNode>> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, Scope , TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, Scope , TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> addToScope(Func<TNode, Scope, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> match(Func<TNode, bool> mapper)
        {
            throw new InvalidOperationException();
        }

        public TNode transform(TNode node, Scope scope)
        {
            if (_functorExtended != null)
                return _functorExtended(node, scope);

            return _functor(node);
        }

    }

}
