using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class Connection
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public string OutputConnector { get; set; }
        public string InputConnector { get; set; }
    }

    public class Instance<TNode>
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public object Value { get; set; }
        public TNode Node { get; set; }

        internal IInstanceTransform<TNode> Transform { get; set; }
        internal List<InstanceConnection<TNode>> Connections { get; set; }
    }

    public class InstanceMatchBase<TNode> : IInstanceMatch<TNode>
    {
        IInstanceAnalisys<TNode> _owner;
        Func<string, object, Scope, bool> _match;
        public InstanceMatchBase(IInstanceAnalisys<TNode> owner, Func<string, object, Scope, bool> match)
        {
            _owner = owner;
            _match = match;
        }

        Dictionary<string, InstanceConnector> _input = new Dictionary<string, InstanceConnector>();
        Dictionary<string, InstanceConnector> _output = new Dictionary<string, InstanceConnector>();
        Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> _dataTransform = new Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>>();
        Dictionary<InstanceConnector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode>> _nodeTransform = new Dictionary<InstanceConnector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode>>();
        public IInstanceMatch<TNode> input(InstanceConnector connector, Action<InstanceConnector, InstanceConnection<TNode>, Scope> handler)
        {
            var id = connector.Id;
            if (_input.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            _input[id] = connector;

            Debug.Assert(handler != null);
            _dataTransform[connector] = handler;
            return this;
        }

        public IInstanceMatch<TNode> input(InstanceConnector connector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode> handler)
        {
            var id = connector.Id;
            if (_input.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            _input[id] = connector;

            Debug.Assert(handler != null);
            _nodeTransform[connector] = handler;
            return this;
        }

        public IInstanceMatch<TNode> output(InstanceConnector connector, Action<InstanceConnector, InstanceConnection<TNode>, Scope> handler)
        {
            var id = connector.Id;
            if (_output.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            _output[id] = connector;

            Debug.Assert(handler != null);
            _dataTransform[connector] = handler;
            return this;
        }

        public IInstanceMatch<TNode> output(InstanceConnector connector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode> handler)
        {
            var id = connector.Id;
            if (_output.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            _output[id] = connector;

            Debug.Assert(handler != null);
            _nodeTransform[connector] = handler;
            return this;
        }

        IInstanceTransform<TNode> _transform;
        public IInstanceAnalisys<TNode> then(Func<string, object, TNode, Scope, TNode> handler)
        {
            return then((id, instance, node, connections, scope) => handler(id, instance, node, scope));
        }

        public IInstanceAnalisys<TNode> then(Func<string, object, Scope, TNode> handler)
        {
            return then((id, instance, node, connections, scope) => handler(id, instance, scope));
        }

        public IInstanceAnalisys<TNode> then(Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler)
        {
            return then(new FunctorInstanceTransform<TNode>(_input, _output, _dataTransform, _nodeTransform, handler));
        }

        public IInstanceAnalisys<TNode> then(IInstanceTransform<TNode> transform)
        {
            Debug.Assert(_transform == null);
            _transform = transform;
            return _owner;
        }
    }

    public abstract class InstanceTransformBase<TNode> : IInstanceTransform<TNode>
    {
        Dictionary<string, InstanceConnector> _input;
        Dictionary<string, InstanceConnector> _output;
        Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> _connectorDataTransform;
        Dictionary<InstanceConnector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode>> _connectionTransform;
        public InstanceTransformBase(
            Dictionary<string, InstanceConnector> input,
            Dictionary<string, InstanceConnector> output,
            Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> connectorDataTransform,
            Dictionary<InstanceConnector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode>> connectionTransform)
        {
            _input = input;
            _output = output;
            _connectorDataTransform = connectorDataTransform;
            _connectionTransform = connectionTransform;
        }

        public TNode transform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope)
        {
            TNode result = doTransform(id, instance, node, connections, scope);
            foreach (var connection in connections)
            {
                bool isInput = connection.InputModel == instance;
                if (isInput)
                    connection.InputTransform = _connectionTransform[connection.Input];
                else
                    connection.OutputTransform = _connectionTransform[connection.Output];
            }

            return result;
        }

        protected abstract TNode doTransform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope);

        public InstanceConnector output(string connector, out Action<InstanceConnector, InstanceConnection<TNode>, Scope> dt)
        {
            dt = null;

            var result = null as InstanceConnector;
            if (_output.TryGetValue(connector, out result))
                _connectorDataTransform.TryGetValue(result, out dt);

            return result;
        }

        public InstanceConnector input(string connector, out Action<InstanceConnector, InstanceConnection<TNode>, Scope> dt)
        {
            dt = null;

            var result = null as InstanceConnector;
            if (_input.TryGetValue(connector, out result))
                _connectorDataTransform.TryGetValue(result, out dt);

            return result;
        }
    }

    public class FunctorInstanceTransform<TNode> : InstanceTransformBase<TNode>
    {
        Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> _handler;
        public FunctorInstanceTransform(
            Dictionary<string, InstanceConnector> input,
            Dictionary<string, InstanceConnector> output,
            Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> connectorDataTransform,
            Dictionary<InstanceConnector, Func<TNode, InstanceConnector, InstanceConnection<TNode>, Scope, TNode>> connectionTransform,
            Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler) : base(input, output, connectorDataTransform, connectionTransform)
        {
            _handler = handler;
        }

        protected override TNode doTransform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope)
        {
            return _handler(id, instance, node, connections, scope);
        }
    }

    public abstract class InstanceDocumentBase<TToken, TNode, TModel> : BaseDocument<TToken, TNode, TModel>, IInstanceDocument<TNode>
    {
        public InstanceDocumentBase(Scope scope) : base(scope)
        {
        }

        Dictionary<Func<object, Scope, bool>, IInstanceTransform<TNode>> _mt = new Dictionary<Func<object, Scope, bool>, IInstanceTransform<TNode>>();
        public void change(Func<object, Scope, bool> match, IInstanceTransform<TNode> transform)
        {
            _mt[match] = transform;
        }

        bool _hasErrors = false;
        protected override void applyLexical()
        {
            IEnumerable<Instance<TNode>> instances;
            IEnumerable<Connection> connections;

            if (parse(_text, _scope, out instances, out connections))
            {
                //collect valid instances
                var namedInstances = new Dictionary<string, Instance<TNode>>();
                foreach (var instance in instances)
                {
                    if (validate(instance, _scope, namedInstances))
                    {
                        Debug.Assert(instance.Value != null);

                        foreach (var mt in _mt)
                        {
                            var match = mt.Key;
                            var transform = mt.Value;

                            if (match(instance.Value, _scope))
                            {
                                var iid = instance.Id;
                                namedInstances[iid] = instance;

                                Debug.Assert(instance.Transform == null);
                                instance.Transform = transform;
                            }
                        }
                    }
                    else
                        _hasErrors = true;
                }

                //fill instance connections
                foreach (var connection in connections)
                {
                    var source = null as Instance<TNode>;
                    var target = null as Instance<TNode>;
                    if (!namedInstances.TryGetValue(connection.Source, out source))
                    {
                        //td: error, invalid source
                        continue;
                    }

                    if (!namedInstances.TryGetValue(connection.Target, out target))
                    {
                        //td: error, invalid source
                        continue;
                    }

                    var outputDT = null as Action<InstanceConnector, InstanceConnection<TNode>, Scope>;
                    var output = source.Transform.output(connection.OutputConnector, out outputDT);

                    var inputDT = null as Action<InstanceConnector, InstanceConnection<TNode>, Scope>;
                    var input = target.Transform.input(connection.InputConnector, out inputDT);

                    var iconn = new InstanceConnection<TNode>()
                    {
                        Source = source.Id,
                        Output = output,
                        OutputModel = source.Value,
                        OutputModelNode = source.Node,
                        Target = target.Id,
                        Input = input,
                        InputModel = target.Value,
                        InputModelNode = target.Node,
                    };

                    if (outputDT != null)
                        outputDT(iconn.Output, iconn, _scope);

                    if (inputDT != null)
                        inputDT(iconn.Input, iconn, _scope);

                    source.Connections.Add(iconn);
                    target.Connections.Add(iconn);
                }

                foreach (var instance in namedInstances.Values)
                {
                    instance.Node = instance.Transform.transform(instance.Id, instance.Value, instance.Node, instance.Connections, _scope);
                    foreach (var conn in instance.Connections)
                    {
                        if (!conn.Ready)
                            continue;

                        bool isInput = instance.Id == conn.Source;
                        var inputInstance = instance;
                        var outputInstance = instance;
                        if (isInput)
                            outputInstance = namedInstances[conn.Target];
                        else
                            inputInstance = namedInstances[conn.Source];

                        applyConnection(conn, inputInstance, outputInstance, _scope);
                    }
                }
            }
            else
                _hasErrors = true;

            if (_hasErrors)
                return;
        }

        private void applyConnection(InstanceConnection<TNode> connection, Instance<TNode> input, Instance<TNode> output, Scope scope)
        {
            if (connection.OutputTransform != null)
            {
                output.Node = connection.OutputTransform(output.Node, connection.Output, connection, scope);
                connection.OutputModelNode = output.Node;
            }

            if (connection.InputTransform != null)
                input.Node = connection.InputTransform(input.Node, connection.Input, connection, scope);
        }

        internal abstract bool validate(Instance<TNode> instance, Scope scope, Dictionary<string, Instance<TNode>> namedInstances);
        protected abstract bool parse(string text, Scope scope, out IEnumerable<Instance<TNode>> instances, out IEnumerable<Connection> connections);
    }


    public class InstanceAnalisysBase<TNode> : IInstanceAnalisys<TNode>
    {
        List<InstanceMatchBase<TNode>> _matchers = new List<InstanceMatchBase<TNode>>();
        public IInstanceMatch<TNode> match(Func<string, object, Scope, bool> handler)
        {
            var result = new InstanceMatchBase<TNode>(this, handler);
            _matchers.Add(result);
            return result;
        }

        public IInstanceMatch<TNode> match<Model>()
        {
            return match((id, instance, scope) => instance is Model);
        }

        public IInstanceMatch<TNode> match(string id)
        {
            return match((iid, instance, scope) => iid == id);
        }

    }

}

