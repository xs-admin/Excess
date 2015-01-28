using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseSyntaxTransform<TNode> : ISyntaxTransform<TNode>
    {
        protected List<Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode>> _transformers = new List<Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode>>();
        protected List<Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>>> _selectors = new List<Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>>>();

        Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode> AddToScope(bool type, bool @namespace)
        {
            return (result, nodes) =>
            {
                var node  = result.Node;
                if (nodes == null)
                    return node;

                TNode scopeNode = resolveScope(node, type, @namespace);
                if (scopeNode != null)
                    return result.schedule(scopeNode, n => addToNode(n, nodes));

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

        Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode> RemoveNodes()
        {
            return (result, nodes) =>
            {
                var node  = result.Node;
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

        public ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector)
        {
            _selectors.Add(selector);
            _transformers.Add(RemoveNodes());
            return this;
        }

        Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>, TNode> ReplaceNodes(Func<ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            return (result, nodes) =>
            {
                var node = result.Node;
                if (nodes == null || !nodes.Any())
                    return node;

                return replaceNodes(result, nodes, handler);
            };
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(ReplaceNodes(result => handler(result.Node)));
            return this;
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            _selectors.Add(SelectFromScope(nodes));
            _transformers.Add(ReplaceNodes(handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            _selectors.Add(selector);
            _transformers.Add(ReplaceNodes(handler));
            return this;
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, TNode> handler)
        {
            _selectors.Add(selector);
            _transformers.Add(ReplaceNodes(result => handler(result.Node)));
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

        public TNode mapTransform(TNode node)
        {
            if (_mapper != null)
            {
                TNode current = node;
                do
                {
                    if (_mapper(current))
                        return current;

                    current = getParent(current);
                }
                while (current != null);

                return current;
            }

            return node;
        }

        protected abstract TNode getParent(TNode node);

        public TNode transform(ISyntacticalMatchResult<TNode> result)
        {
            Debug.Assert(_selectors.Count == _transformers.Count);
            switch (_transformers.Count)
            {
                case 0: return result.Node;
                case 1:
                {
                    //do not track on single transformations
                    var selector = _selectors[0];
                    IEnumerable<TNode> nodes = selector != null? selector(result) : new TNode[] { };
                    var resultNode = _transformers[0](result, nodes);
                    result.Node = resultNode;
                    return resultNode;
                }
                default:
                {
                    var nodeIds     = new Dictionary<TNode, string>();
                    var selectorIds = new Dictionary<object, string>();
                    foreach (var selector in _selectors)
                    {
                        var sNodes = selector(result);
                        if (sNodes.Any())
                        {
                            string unique = _randId.Next().ToString();
                            foreach (var sNode in sNodes)
                            {
                                nodeIds[sNode] = unique;
                                selectorIds[selector] = unique;
                            }
                        }
                    }

                    result.Node = markNodes(result.Node , "xs-syntax-transform", nodeIds);

                    for (int i = 0;  i < _transformers.Count; i++)
                    {
                        var transformer = _transformers[i];
                        var selector    = _selectors[i];

                        string uid;
                        IEnumerable<TNode> nodes = null;
                        if (selectorIds.TryGetValue(selector, out uid))
                            nodes = findNodes(result.Node, "xs-syntax-transform", uid);

                        var node = transformer(result, nodes);
                        if (node == null)
                            return default(TNode);

                        result.Node = node;
                    }

                    return result.Node;
                }
            }
        }

        protected abstract TNode resolveScope(TNode node, bool type, bool @namespace);
        protected abstract TNode removeNodes(TNode node, IEnumerable<TNode> nodes);
        protected abstract TNode replaceNodes(ISyntacticalMatchResult<TNode> result, IEnumerable<TNode> nodes, Func<ISyntacticalMatchResult<TNode>, TNode> handler);
        protected abstract TNode addToNode(TNode node, IEnumerable<TNode> nodes);
        protected abstract IEnumerable<TNode> findNodes(TNode parent, string annotation, string data);
        protected abstract TNode markNodes(TNode parent, string annotation, Dictionary<TNode, string> nodeIds);
    }

    public class FunctorSyntaxTransform<TNode> : ISyntaxTransform<TNode>
    {
        Func<TNode, TNode> _functor;
        Func<ISyntacticalMatchResult<TNode>, TNode> _functorExtended;
        Func<TNode, TNode> _mapper;

        public FunctorSyntaxTransform(Func<TNode, TNode> handler, Func<TNode, TNode> mapper)
        {
            _functor = handler;
            _mapper = mapper;
        }

        public FunctorSyntaxTransform(Func<ISyntacticalMatchResult<TNode>, TNode> handler, Func<TNode, TNode> mapper)
        {
            _functorExtended = handler;
            _mapper = mapper;
        }

        public ISyntaxTransform<TNode> remove(string nodes)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<ISyntacticalMatchResult<TNode>, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<ISyntacticalMatchResult<TNode>, TNode> handler)
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

        public ISyntaxTransform<TNode> match(Func<TNode, bool> mapper)
        {
            throw new InvalidOperationException();
        }

        public TNode mapTransform(TNode node)
        {
            if (_mapper != null)
                return _mapper(node);

            return node;
        }

        public TNode transform(ISyntacticalMatchResult<TNode> result)
        {
            if (_functorExtended != null)
                return _functorExtended(result);

            return _functor(result.Node);
        }

    }

}
