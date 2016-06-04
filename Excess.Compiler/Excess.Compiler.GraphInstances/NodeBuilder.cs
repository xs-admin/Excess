using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Excess.Compiler.Core;

namespace Excess.Compiler.GraphInstances
{
    public class NodeBuilder
    {
        IDictionary<string, object> _instances;
        Dictionary<object, Node> _steps;
        public NodeBuilder(Dictionary<object, Node> steps, IDictionary<string, object> instances)
        {
            _steps = steps;
            _instances = instances;
        }


        public NodeList Result { get { return buildResult(); } }

        List<NodeChain> _chains = new List<NodeChain>();
        public void AddConnection(Connection connection)
        {
            NodeChain left = null;
            NodeChain right = null;
            foreach (var chain in _chains)
            {
                if (chain.Append(connection))
                {
                    if (left == null)
                        left = chain;
                    else
                    {
                        Debug.Assert(right == null);
                        right = chain;
                    }
                }
            }

            if (left != null && right != null)
            {
                if (left.Append(right))
                    _chains.Remove(right);
                else if (right.Append(left))
                    _chains.Remove(left);
                else
                    Debug.Assert(false);
            }
            else if (left == null && right == null)
                _chains.Add(new NodeChain(connection, this));
        }

        public void StartChain(Connection link)
        {
            var step = GetStep(link.Source);
            _chains.Add(new NodeChain(link, this, step, link.InputConnector));
        }

        private Node GetStep(string node)
        {
            return _steps[_instances[node]];
        }

        private NodeList buildResult()
        {
            NodeList result = null;
            HashSet<object> found = new HashSet<object>();
            foreach (var chain in _chains)
            {
                var container = new NodeList(chain.Nodes);
                if (chain.Parent == null)
                {
                    Debug.Assert(result == null);
                    result = container;
                }
                else
                {
                    chain.Parent.Inner[chain.ParentConnector] = container;
                }
            }

            Debug.Assert(result != null);
            var unconnected = new List<Node>();
            foreach (var node in _steps)
            {
                if (!found.Contains(node.Key))
                    unconnected.Add(node.Value);
            }

            result.Nodes = result.Nodes.Union(unconnected);
            return result;
        }

        private class NodeChain
        {
            NodeBuilder _builder;

            public Node Parent { get; private set; }
            public string ParentConnector { get; private set; }

            public string Left { get; private set; }
            public string Right { get; private set; }
            public IEnumerable<Node> Nodes { get { return _nodes; } }

            List<Node> _nodes = new List<Node>();

            public NodeChain(Connection link, NodeBuilder builder)
            {
                Left = link.Source;
                Right = link.Target;

                _builder = builder;
                _nodes.Add(_builder.GetStep(Left));
                _nodes.Add(_builder.GetStep(Right));
            }

            public NodeChain(Connection link, NodeBuilder builder, Node parent, string parentConnector)
            {
                Parent = parent;
                ParentConnector = parentConnector;

                Left = null;
                Right = link.Target;

                _builder = builder;
                _nodes.Add(_builder.GetStep(Right));
            }

            public bool Append(Connection link)
            {
                if (checkLeft(link))
                {
                    Left = link.Source;
                    _nodes.Insert(0, _builder.GetStep(Left));
                }
                else if (checkRight(link))
                {
                    Right = link.Target;
                    _nodes.Add(_builder.GetStep(Right));
                }
                else
                    return false;

                return true;
            }

            public bool Append(NodeChain chain)
            {
                if (chain.Left != Right)
                    return false;

                Right = chain.Right;
                _nodes.AddRange(chain.Nodes.Skip(1));

                return true;
            }

            private bool checkLeft(Connection link)
            {
                return link.Target == Left;
            }

            private bool checkRight(Connection link)
            {
                return link.Source == Right;
            }

        }
    }
}
