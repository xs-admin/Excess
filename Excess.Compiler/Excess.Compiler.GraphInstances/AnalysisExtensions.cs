using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Excess.Compiler.GraphInstances
{
    public static class AnalysisExtensions
    {
        public static IInstanceAnalisys<SyntaxNode> then<T>(this IInstanceMatch<SyntaxNode> @this, Action<Node, T, Scope> handler)
        {
            return @this.then((id, value, scope) =>
            {
                if (value is T)
                {
                    var graph = scope.get<IExcessGraph>();
                    var node = graph.GetNode(value);
                    if (node != null)
                        handler(node, (T)value, scope);
                }

                return null;
            });
        }
    }
}
