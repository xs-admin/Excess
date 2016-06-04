using System.Collections.Generic;

namespace Excess.Compiler.GraphInstances
{
    public static class GraphScopeExtensions
    {
        internal static void InitGraph(this Scope @this, IDictionary<object, Node> allNodes, IEnumerable<Node> rootNodes)
        {
            @this.set<IExcessGraph>(new ExcessGraph(allNodes, rootNodes));
        }

        public static Node GetNode(this Scope @this, object model)
        {
            var nodes = @this.get<IExcessGraph>().Nodes;
            return nodes[model];
        }

        public static IEnumerable<Node> GetRootNodes(this Scope @this)
        {
            return @this.get<IExcessGraph>().Root;
        }
    }
}
