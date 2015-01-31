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

        protected abstract TNode resolveScope(TNode node, bool type, bool @namespace);

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

        public ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, Scope, TNode> handler)
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

        protected abstract TNode removeNodes(TNode node, IEnumerable<TNode> nodes);
        protected abstract TNode replaceNodes(TNode node, Scope scope, IEnumerable<TNode> nodes, Func<TNode, Scope, TNode> handler);
        protected abstract TNode addToNode(TNode node, IEnumerable<TNode> nodes);

        public TNode transform(TNode node, Scope scope)
        {
            var compiler = scope.GetService<TToken, TNode, TModel>();

            Debug.Assert(compiler != null && _selectors.Count == _transformers.Count);
            switch (_transformers.Count)
            {
                case 0: return node;
                case 1:
                {
                    //do not track on single transformations
                    var selector = _selectors[0];
                    IEnumerable<TNode> nodes = selector != null? selector(node, scope) : new TNode[] { };
                    var resultNode = _transformers[0](node, scope, nodes);
                    return resultNode;
                }
                default:
                {
                    //problem here is there are dependency nodes obtained 
                    //during match, so tracking should be performed.
                    //I never got the roslyn one to work for some reason.
                    //so, td:

                    var selectorIds = new Dictionary<object, List<string>>();
                    foreach (var selector in _selectors)
                    {
                        var sNodes = selector(node, scope);
                        if (sNodes.Any())
                        {
                            foreach (var sNode in sNodes)
                            {
                                var xsid = compiler.GetExcessId(sNode).ToString();

                                List<string> selectorNodes;
                                if (!selectorIds.TryGetValue(selector, out selectorNodes))
                                {
                                    selectorNodes = new List<string>();
                                    selectorIds[selector] = selectorNodes;
                                }

                                selectorNodes.Add(xsid);
                            }
                        }
                    }

                    for (int i = 0;  i < _transformers.Count; i++)
                    {
                        var transformer = _transformers[i];
                        var selector    = _selectors[i];

                        IEnumerable<TNode> nodes = null;
                        List<string> nodeIds;
                        if (selectorIds.TryGetValue(selector, out nodeIds))
                            nodes = compiler.Find(node, nodeIds);

                        node = transformer(node, scope, nodes);
                        if (node == null)
                            return default(TNode);
                    }

                    return node;
                }
            }
        }    
    }
}
