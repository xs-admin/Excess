using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntaxTransform<TNode> : ISyntaxTransform<TNode>
    {
        List<Func<TNode, ISyntacticalMatchResult<TNode>, TNode>> _transformers = new List<Func<TNode, ISyntacticalMatchResult<TNode>, TNode>>();

        Func<TNode, ISyntacticalMatchResult<TNode>, TNode> AddToScope(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler, bool type, bool @namespace)
        {
            return (node, result) =>
            {
                var nodes = handler(result);
                if (nodes == null)
                    return node;

                TNode scope = resolveScope(node, type, @namespace);
                if (scope != null)
                    return result.schedule("syntactical-pass", scope, n => addToNode(n, nodes));

                return node;
            };
        }

        Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> SelectFromScope(string nodes)
        {
            return result =>
            {
                return result.Scope.get<IEnumerable<TNode>>(nodes);
            };
        }

        public ISyntaxTransform<TNode> addToScope(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false)
        {
            _transformers.Add(AddToScope(handler, type, @namespace));
            return this;
        }

        public ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false)
        {
            _transformers.Add(AddToScope(SelectFromScope(nodes), type, @namespace));
            return this;
        }

        Func<TNode, ISyntacticalMatchResult<TNode>, TNode> RemoveNodes(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector)
        {
            return (node, result) =>
            {
                var nodes = selector(result);
                if (nodes == null)
                    return node;

                return removeNodes(node, nodes);
            };
        }

        public ISyntaxTransform<TNode> remove(string nodes)
        {
            _transformers.Add(RemoveNodes(SelectFromScope(nodes)));
            return this;
        }

        public ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector)
        {
            _transformers.Add(RemoveNodes(selector));
            return this;
        }

        Func<TNode, ISyntacticalMatchResult<TNode>, TNode> ReplaceNodes(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            return (node, result) =>
            {
                var nodes = selector(result);
                if (nodes == null)
                    return node;

                return replaceNodes(node, nodes, handler);
            };
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            _transformers.Add(ReplaceNodes(SelectFromScope(nodes), (node, result) => handler(node)));
            return this;
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            _transformers.Add(ReplaceNodes(SelectFromScope(nodes), handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            _transformers.Add(ReplaceNodes(selector, handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, TNode> handler)
        {
            _transformers.Add(ReplaceNodes(selector, (node, result) => handler(node)));
            return this;
        }

        public TNode transform(TNode node, ISyntacticalMatchResult<TNode> result)
        {
            foreach (var transformer in _transformers)
            {
                node = transformer(node, result);
                if (node == null)
                    return default(TNode);
            }

            return node;
        }

        protected abstract TNode resolveScope(TNode node, bool type, bool @namespace);
        protected abstract TNode removeNodes(TNode node, object nodes);
        protected abstract TNode replaceNodes(TNode node, IEnumerable<TNode> nodes, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler);
        protected abstract TNode addToNode(TNode n, IEnumerable<TNode> nodes);

    }

    public class FunctorSyntaxTransform<TNode> : ISyntaxTransform<TNode>
    {
        Func<TNode, TNode> _functor;
        Func<TNode, ISyntacticalMatchResult<TNode>, TNode> _functorExtended;

        public FunctorSyntaxTransform(Func<TNode, TNode> handler)
        {
            _functor = handler;
        }

        public FunctorSyntaxTransform(Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            _functorExtended = handler;
        }

        public ISyntaxTransform<TNode> remove(string nodes)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> addToScope(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false)
        {
            throw new InvalidOperationException();
        }

        public TNode transform(TNode node, ISyntacticalMatchResult<TNode> result)
        {
            if (_functorExtended != null)
                return _functorExtended(node, result);

            return _functor(node);
        }

    }

}
