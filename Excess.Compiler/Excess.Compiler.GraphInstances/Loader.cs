using Excess.Compiler.Core;
using Excess.Compiler.Roslyn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Excess.Compiler.GraphInstances
{
    public class GraphLoader
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


                BuildGraph(instances, connections, scope);
                return true;
            });
        }

        public static RoslynInstanceDocument FromWebNodes(JObject root, Scope docScope, IEnumerable<Type> types = null, IDictionary<string, Func<JToken, object>> serializers = null)
        {
            if (serializers == null)
                serializers = new Dictionary<string, Func<JToken, object>>();

            if (types != null)
            {
                foreach (var type in types)
                {
                    var typeName = type.Name;
                    if (serializers.ContainsKey(typeName))
                        throw new InvalidOperationException($"duplicate {typeName}");

                    serializers[typeName] = jtoken => jtoken.ToObject(type);
                }
            }

            return new RoslynInstanceDocument((_, instances, connections, scope) =>
            {
                var nodes = root
                    .Property("nodes")
                    .Value as JArray;

                var links = root
                    .Property("links")
                    .Value as JArray;

                //load instances
                var idx = 0;
                foreach (var node in nodes
                    .Children()
                    .Select(n => (JObject)n))
                {
                    var id = idx.ToString(); idx++;
                    var data = node.Property("data")?.Value
                        ??     node.Property("name")?.Value;
                    var type = node.Property("typeName")?.Value.ToString();
                    var model = serializers[type](data);

                    instances[id] = model;
                }

                //load connections
                foreach (var link in links
                    .Children()
                    .Select(n => (JObject)n))
                {
                    var result = new Connection
                    {
                        InputConnector = link.Property("inputSocketName")?.Value.ToString(),
                        OutputConnector = link.Property("outputSocketName")?.Value.ToString(),
                        Source = link.Property("outputNode")?.Value.ToString(),
                        Target = link.Property("inputNode")?.Value.ToString(),
                    };

                    connections.Add(result);
                }

                BuildGraph(instances, connections, scope);
                return true;
            }, docScope);
        }

        private static void BuildGraph(IDictionary<string, object> instances, IEnumerable<Connection> connections, Scope scope)
        {
            var allNodes = new Dictionary<object, Node>();
            var indices = new Dictionary<string, int>();
            foreach (var node in instances)
            {
                allNodes[node.Value] = new Node(node.Key, node.Value);
            }

            var builder = new NodeBuilder(allNodes, instances);
            foreach (var link in connections)
            {
                if (link.InputConnector.ToLowerInvariant() == "previous")
                {
                    if (link.OutputConnector.ToLowerInvariant() == "next")
                        builder.AddConnection(link);
                    else
                        builder.StartChain(link);
                }
            }


            scope.InitGraph(allNodes, builder.Result.Nodes);
        }
    }
}
