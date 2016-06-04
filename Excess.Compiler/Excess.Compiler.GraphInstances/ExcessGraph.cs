using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.GraphInstances
{
    public interface IExcessGraph
    {
        IDictionary<object, Node> Nodes { get; }
        IEnumerable<Node> Root { get; }
        IEnumerable<StatementSyntax> RenderNodes(IEnumerable<Node> nodes);
        Node GetNode(object key);
    }

    internal class ExcessGraph : IExcessGraph
    {
        public ExcessGraph(IDictionary<object, Node> nodes, IEnumerable<Node> root)
        {
            Nodes = nodes;
            Root = root;
        }

        public IDictionary<object, Node> Nodes { get; private set; }
        public IEnumerable<Node> Root { get; private set; }

        public Node GetNode(object key)
        {
            var result = null as Node;
            if (Nodes != null && Nodes.TryGetValue(key, out result))
                return result;

            return null;
        }
        public IEnumerable<StatementSyntax> RenderNodes(IEnumerable<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Before == null)
                    continue;

                foreach (var syntax in node.Before)
                    yield return syntax;
            }

            foreach (var node in nodes)
            {
                if (node.Execute == null)
                    continue;

                foreach (var syntax in node.Execute)
                    yield return syntax;
            }

            foreach (var node in nodes)
            {
                if (node.After == null)
                    continue;

                foreach (var syntax in node.After)
                    yield return syntax;
            }
        }
    }
}
