using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Razor
{
    class InstanceTransform : InstanceTransformBase<SyntaxNode>
    {
        public InstanceTransform(
            Dictionary<string, InstanceConnector> input,
            Dictionary<string, InstanceConnector> output,
            Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>> connectorDataTransform,
            Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<SyntaxNode>, Scope>> connectionTransform,
            Func<string, object, SyntaxNode, IEnumerable<InstanceConnection<SyntaxNode>>, Scope, SyntaxNode> handler) : 
                base(input, output, connectorDataTransform, connectionTransform)
        {
        }

        protected override SyntaxNode doTransform(string id, object instance, SyntaxNode node, IEnumerable<InstanceConnection<SyntaxNode>> connections, Scope scope)
        {
            throw new NotImplementedException();
        }
    }
}
