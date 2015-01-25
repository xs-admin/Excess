using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class RoslynSyntacticalMatchResult : BaseSyntacticalMatchResult<SyntaxNode>
    {
        public RoslynSyntacticalMatchResult(Scope scope, IEventBus events, SyntaxNode node = null)
            : base(node, scope, events)
        {
            Preprocess = false;
        }

        protected override SyntaxNode markNode(SyntaxNode node, int id)
        {
            return RoslynCompiler.SetSyntaxId(node, id);
        }
    }

    public class RoslynSyntaxTransform : BaseSyntaxTransform<SyntaxNode>
    {

        public RoslynSyntaxTransform()
        {
        }

        public RoslynSyntaxTransform(Func<ISyntacticalMatchResult<SyntaxNode>, IEnumerable<SyntaxNode>, SyntaxNode> handler)
        {
            _selectors.Add(null);
            _transformers.Add(handler);
        }

        protected override SyntaxNode addToNode(SyntaxNode node, IEnumerable<SyntaxNode> nodes)
        {
            throw new NotImplementedException(); //td: 
        }

        protected override SyntaxNode removeNodes(SyntaxNode node, IEnumerable<SyntaxNode> nodes)
        {
            return node.RemoveNodes(nodes, SyntaxRemoveOptions.KeepEndOfLine);
        }

        protected override SyntaxNode replaceNodes(ISyntacticalMatchResult<SyntaxNode> result, IEnumerable<SyntaxNode> nodes, Func<ISyntacticalMatchResult<SyntaxNode>, SyntaxNode> handler)
        {
            return result.Node.ReplaceNodes(nodes, (oldNode, newNode) =>
            {
                //change the result temporarily, this might need revisiting
                var oldResultNode = result.Node;
                result.Node = newNode;
                var returnValue = handler(result);
                result.Node = oldResultNode;
                return returnValue;
            });
        }

        protected override SyntaxNode resolveScope(SyntaxNode node, bool type, bool @namespace)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<SyntaxNode> findNodes(SyntaxNode parent, string annotation, string data)
        {
            return parent.GetAnnotatedNodes(annotation)
                .Where(node => node.GetAnnotations(annotation).First().Data == data);
        }

        protected override SyntaxNode markNodes(SyntaxNode parent, string annotation, Dictionary<SyntaxNode, string> nodeIds)
        {
            return parent.ReplaceNodes(nodeIds.Keys, (oldNode, newNode) => {
                return newNode.WithAdditionalAnnotations(new SyntaxAnnotation(annotation, nodeIds[oldNode]));
            });
        }
    }

    public class RoslynSyntacticalMatch : BaseSyntacticalMatch<SyntaxNode>
    {
        public RoslynSyntacticalMatch(ISyntaxAnalysis<SyntaxNode> syntax) :
            base(syntax)
        {
        }

        protected override IEnumerable<SyntaxNode> children(SyntaxNode node)
        {
            return node.ChildNodes();
        }

        protected override IEnumerable<SyntaxNode> descendants(SyntaxNode node)
        {
            return node.DescendantNodes();
        }
    }

    public class RoslynSyntaxAnalysis : BaseSyntaxAnalysis<SyntaxNode>
    {
        protected override ISyntaxTransform<SyntaxNode> createTransform()
        {
            return new RoslynSyntaxTransform();
        }

        protected override ISyntacticalMatch<SyntaxNode> createMatch(Func<SyntaxNode, bool> selector)
        {
            RoslynSyntacticalMatch result = new RoslynSyntacticalMatch(this);
            result.addMatcher(selector);
            return result;
        }

        protected override ISyntaxTransform<SyntaxNode> createTransform(Func<ISyntacticalMatchResult<SyntaxNode>, IEnumerable<SyntaxNode>, SyntaxNode> handler)
        {
            return new RoslynSyntaxTransform(handler);
        }
    }
}
