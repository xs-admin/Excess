using Excess.Compiler.Core;
using Excess.Compiler.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Excess.Compiler.GraphInstances
{
    class GraphLoader
    {
        public static RoslynInstanceDocument FromNodeGraphView(XElement xmlRoot, Func<string, string, object> serializer)
        {
            return new RoslynInstanceDocument((_, instances, connections, scope) =>
            {
                var view = xmlRoot
                    .Descendants()
                    .Where(element => element.Name.LocalName == "NodeGraphControl.NodeGraphView")
                    .Single();

                var nodes = view
                    .Descendants()
                    .Where(element => element.Name.LocalName == "NodeGraphNodeCollection")
                    .Single();

                var xmlConnections = view
                    .Descendants()
                    .Where(element => element.Name.LocalName == "NodeGraphLinkCollection")
                    .Single();

                //load instances
                foreach (var node in nodes.Elements())
                {
                    var id = node.Attribute("id").Value;
                    var data = node.Attribute("Data").Value;
                    var type = node.Attribute("TypeName").Value;
                    var model = serializer(type, data);

                    instances[id] = model;
                }

                //load connections
                foreach (var node in xmlConnections.Elements())
                {
                    var result = new Connection
                    {
                        InputConnector = node.Attribute("InputConnectorName").Value,
                        OutputConnector = node.Attribute("OutputConnectorName").Value,
                        Source = node.Attribute("InputNodeId").Value,
                        Target = node.Attribute("OutputNodeId").Value,
                    };

                    connections.Add(result);
                }

                var steps = BuildSteps(instances, connections);
                return false;
            });
        }

        private static StepContainer BuildSteps(IDictionary<string, object> instances, IEnumerable<Connection> connections)
        {
            var steps = new Dictionary<object, Step>();
            var indices = new Dictionary<string, int>();
            foreach (var node in instances)
            {
                var typeName = node.Value.GetType().Name;
                if (!indices.ContainsKey(typeName))
                    indices[typeName] = 1;

                var idx = indices[typeName];
                var iid = typeName + idx;

                indices[typeName] = idx + 1;
                instances[iid] = node.Key;
                steps[node.Key] = new Step(iid);
            }

            StepBuilder builder = new StepBuilder(steps);
            foreach (var link in connections)
            {
                if (link.OutputConnector == "Previous")
                {
                    if (link.InputConnector == "Next")
                        builder.AddConnection(link);
                    else
                        builder.StartChain(link);
                    continue;
                }
            }

            return builder.Result;
        }
    }
}
