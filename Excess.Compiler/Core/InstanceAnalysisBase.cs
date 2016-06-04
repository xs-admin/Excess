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
        public Instance()
        {
            Connections = new List<InstanceConnection<TNode>>();
        }

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
        Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>> _dataTransform = new Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>>();
        Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> _nodeTransform = new Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>>();
        Action<InstanceConnection<TNode>, Scope> _defaultInputTransform;
        Action<InstanceConnection<TNode>, Scope> _defaultOutputTransform;

        public IInstanceMatch<TNode> input(Action<InstanceConnection<TNode>, Scope> transform)
        {
            if (_defaultInputTransform != null)
                throw new InvalidOperationException("duplicate default transform");

            _defaultInputTransform = transform;
            return this;
        }

        public IInstanceMatch<TNode> output(Action<InstanceConnection<TNode>, Scope> transform)
        {
            if (_defaultOutputTransform != null)
                throw new InvalidOperationException("duplicate default transform");

            _defaultOutputTransform = transform;
            return this;
        }

        public IInstanceMatch<TNode> input(string connectorId, Action<InstanceConnector, object, object, Scope> dt, Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            var connector = null as InstanceConnector;
            _input.TryGetValue(connectorId, out connector);
            return input(connector ?? new InstanceConnector { Id = connectorId }, dt, transform);
        }

        public IInstanceMatch<TNode> input(InstanceConnector connector, Action<InstanceConnector, object, object, Scope> dt, Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            var id = connector.Id;
            if (_input.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            if (dt != null)
                _dataTransform[connector] = dt;

            if (transform != null)
                _nodeTransform[connector] = transform;

            _input[id] = connector;
            return this;
        }

        public IInstanceMatch<TNode> output(string connectorId, Action<InstanceConnector, object, object, Scope> dt, Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            var connector = null as InstanceConnector;
            _output.TryGetValue(connectorId, out connector);
            return output(connector ?? new InstanceConnector { Id = connectorId }, dt, transform);
        }

        public IInstanceMatch<TNode> output(InstanceConnector connector, Action<InstanceConnector, object, object, Scope> dt, Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            var id = connector.Id;
            if (_output.ContainsKey(id))
                throw new InvalidOperationException("duplicate input");

            if (dt != null)
                _dataTransform[connector] = dt;

            if (transform != null)
                _nodeTransform[connector] = transform;

            _output[id] = connector;
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

        public IInstanceAnalisys<TNode> then(Func<string, object, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler)
        {
            return then((id, instance, node, connections, scope) => handler(id, instance, connections, scope));
        }

        public IInstanceAnalisys<TNode> then(Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler)
        {
            return then(new FunctorInstanceTransform<TNode>(_input, _output, _dataTransform, _nodeTransform, _defaultInputTransform, _defaultOutputTransform, handler));
        }

        public IInstanceAnalisys<TNode> then(IInstanceTransform<TNode> transform)
        {
            Debug.Assert(_transform == null);
            _transform = transform;
            return _owner;
        }

        internal void apply(IInstanceDocument<TNode> document)
        {
            Debug.Assert(_match != null);
            if (_transform == null)
                _transform = new FunctorInstanceTransform<TNode>(_input, _output, _dataTransform, _nodeTransform, _defaultInputTransform, _defaultOutputTransform, null);

            document.change(_match, _transform);
        }
    }

    public abstract class InstanceTransformBase<TNode> : IInstanceTransform<TNode>
    {
        Dictionary<string, InstanceConnector> _input;
        Dictionary<string, InstanceConnector> _output;
        Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>> _connectorDataTransform;
        Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> _connectionTransform;
        Action<InstanceConnection<TNode>, Scope> _defaultInputTransform;
        Action<InstanceConnection<TNode>, Scope> _defaultOutputTransform;

        public InstanceTransformBase(
            Dictionary<string, InstanceConnector> input,
            Dictionary<string, InstanceConnector> output,
            Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>> connectorDataTransform,
            Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> connectionTransform,
            Action<InstanceConnection<TNode>, Scope> defaultInputTransform,
            Action<InstanceConnection<TNode>, Scope> defaultOutputTransform)
        {
            _input = input;
            _output = output;
            _connectorDataTransform = connectorDataTransform;
            _connectionTransform = connectionTransform;
            _defaultInputTransform = defaultInputTransform;
            _defaultOutputTransform = defaultOutputTransform;
        }

        public TNode transform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope)
        {
            TNode result = doTransform(id, instance, node, connections, scope);
            foreach (var connection in connections)
            {
                bool isInput = connection.InputModel == instance;
                if (isInput)
                {
                    var transform = connection.InputTransform;
                    var found = false;
                    if (connection.Input != null)
                        found = _connectionTransform.TryGetValue(connection.Input, out transform);

                    if (!found && _defaultInputTransform != null)
                        transform = (connector, connection_, scope_) => _defaultInputTransform(connection_, scope_);

                    connection.InputTransform = transform;
                }
                else
                {
                    var transform = connection.InputTransform;
                    var found = false;
                    if (connection.Output != null)
                        found = _connectionTransform.TryGetValue(connection.Output, out transform);

                    if (!found && _defaultOutputTransform != null)
                        transform = (connector, connection_, scope_) => _defaultOutputTransform(connection_, scope_);

                    connection.InputTransform = transform;
                }
            }

            return result;
        }

        protected abstract TNode doTransform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope);

        public InstanceConnector output(
            string connector, 
            out Action<InstanceConnector, object, object, Scope> dt, 
            out Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            dt = null;
            transform = null;

            var result = null as InstanceConnector;
            if (_output.TryGetValue(connector, out result))
            {
                _connectorDataTransform.TryGetValue(result, out dt);
                _connectionTransform.TryGetValue(result, out transform);
            }

            return result;
        }

        public InstanceConnector input(
            string connector,
            out Action<InstanceConnector, object, object, Scope> dt,
            out Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform)
        {
            dt = null;
            transform = null;

            var result = null as InstanceConnector;
            if (_input.TryGetValue(connector, out result))
            {
                _connectorDataTransform.TryGetValue(result, out dt);
                _connectionTransform.TryGetValue(result, out transform);
            }

            return result;
        }
    }

    public class FunctorInstanceTransform<TNode> : InstanceTransformBase<TNode>
    {
        Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> _handler;
        public FunctorInstanceTransform(
            Dictionary<string, InstanceConnector> input,
            Dictionary<string, InstanceConnector> output,
            Dictionary<InstanceConnector, Action<InstanceConnector, object, object, Scope>> connectorDataTransform,
            Dictionary<InstanceConnector, Action<InstanceConnector, InstanceConnection<TNode>, Scope>> connectionTransform,
            Action<InstanceConnection<TNode>, Scope> defaultInputTransform,
            Action<InstanceConnection<TNode>, Scope> defaultOutputTransform,
            Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler) : base(input, output, connectorDataTransform, connectionTransform, defaultInputTransform, defaultOutputTransform)
        {
            _handler = handler;
        }

        protected override TNode doTransform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope)
        {
            if (_handler == null)
                return default(TNode);

            return _handler(id, instance, node, connections, scope);
        }
    }

    public class InstanceDocumentBase<TNode> : IInstanceDocument<TNode>
    {
        public InstanceDocumentBase()
        {
        }

        Dictionary<Func<string, object, Scope, bool>, IInstanceTransform<TNode>> _mt = new Dictionary<Func<string, object, Scope, bool>, IInstanceTransform<TNode>>();
        public void change(Func<string, object, Scope, bool> match, IInstanceTransform<TNode> transform)
        {
            _mt[match] = transform;
        }

        Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> _transform;
        public void change(Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> transform)
        {
            Debug.Assert(_transform == null);
            _transform = transform;
        }

        bool _hasErrors = false;
        public TNode transform(IDictionary<string, object> instances, IEnumerable<Connection> connections, Scope scope)
        {
            Debug.Assert(_transform != null);

            //match
            var namedInstances = new Dictionary<string, Instance<TNode>>();
            foreach (var i in instances)
            {
                Debug.Assert(i.Value != null);
                var instance = new Instance<TNode>
                {
                    Id = i.Key,
                    Value = i.Value
                };

                if (!_mt.Any())
                {
                    namedInstances[instance.Id] = instance;
                    continue;
                }

                foreach (var mt in _mt)
                {
                    var match = mt.Key;
                    var transform = mt.Value;

                    if (match(i.Key, i.Value, scope))
                    {
                        namedInstances[instance.Id] = instance;

                        Debug.Assert(instance.Transform == null);
                        instance.Transform = transform;
                            break;
                    }
                }
            }

            //apply connections, save 
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

                var outputDT = null as Action<InstanceConnector, object, object, Scope>;
                var outputTransform = null as Action<InstanceConnector, InstanceConnection<TNode>, Scope>;
                var output = source.Transform?.output(connection.OutputConnector, out outputDT, out outputTransform);
                if (output == null)
                    output = new InstanceConnector { Id = connection.OutputConnector };

                var inputDT = null as Action<InstanceConnector, object, object, Scope>;
                var inputTransform = null as Action<InstanceConnector, InstanceConnection<TNode>, Scope>;
                var input = target.Transform?.input(connection.InputConnector, out inputDT, out inputTransform);
                if (input == null)
                    input = new InstanceConnector { Id = connection.InputConnector };

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
                    outputDT(iconn.Output, iconn.InputModel, iconn.OutputModel, scope);

                if (inputDT != null)
                    inputDT(iconn.Input, iconn.OutputModel, iconn.InputModel, scope);

                iconn.InputTransform = inputTransform;
                iconn.OutputTransform = outputTransform;
                source.Connections.Add(iconn);
                target.Connections.Add(iconn);
            }

            foreach (var instance in namedInstances.Values)
            {
                transformInstance(instance, scope);
            }


            foreach (var instance in namedInstances.Values)
            {
                foreach (var conn in instance.Connections)
                {
                    bool isInput = instance.Id == conn.Target;
                    var inputInstance = instance;
                    var outputInstance = instance;
                    if (isInput)
                    {
                        outputInstance = namedInstances[conn.Target];
                        applyConnection(conn, outputInstance, isInput, conn.InputTransform, scope);
                    }
                    else
                    {
                        inputInstance = namedInstances[conn.Source];
                        applyConnection(conn, inputInstance, isInput, conn.OutputTransform, scope);
                    }
                }
            }

            if (_hasErrors)
                return default(TNode);

            var items = new Dictionary<string, Tuple<object, TNode>>();
            foreach (var item in namedInstances)
                items[item.Key] = new Tuple<object, TNode>(item.Value.Value, item.Value.Node);

            return _transform(items, scope);
        }

        private void transformInstance(Instance<TNode> instance, Scope scope)
        {
            instance.Node = instance.Transform.transform(instance.Id, instance.Value, instance.Node, instance.Connections, scope);

            foreach (var conn in instance.Connections)
            {
                bool isInput = instance.Id == conn.Target;
                if (isInput)
                    conn.InputModelNode = instance.Node;
                else
                    conn.OutputModelNode = instance.Node;
            }
        }

        private void applyConnection(
            InstanceConnection<TNode> connection,
            Instance<TNode> instance,
            bool isInput,
            Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform, 
            Scope scope)
        {

            if (transform != null)
            {
                var connector = isInput ? connection.Input : connection.Output;

                transform(connector, connection, scope);
                instance.Node = isInput ? connection.InputModelNode : connection.OutputModelNode;
            }
        }
    }


    public class InstanceAnalisysBase<TToken, TNode, TModel> : IInstanceAnalisys<TNode>, 
                                                               IDocumentInjector<TToken, TNode, TModel>
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

        Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> _transform;
        public void then(Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> transform)
        {
            Debug.Assert(_transform == null);
            _transform = transform;
        }

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            var doc = document as IInstanceDocument<TNode>;

            Debug.Assert(doc != null);
            Debug.Assert(_transform != null);
            doc.change(_transform);

            foreach (var matcher in _matchers)
                matcher.apply(doc);
        }
    }

}

