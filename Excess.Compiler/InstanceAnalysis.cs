using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public class InstanceConnector
    {
        public string Id { get; set; }
    }

    public class InstanceConnection<TNode>
    {
        public string Source { get; set; }
        public string Target { get; set; }
        public InstanceConnector Output { get; set; }
        public InstanceConnector Input { get; set; }

        public object OutputModel { get; set; }
        public TNode OutputNode { get; set; }
        public TNode OutputModelNode { get; set; }
        public object InputModel { get; set; }
        public TNode InputNode { get; set; }
        public TNode InputModelNode { get; set; }

        public Action<InstanceConnector, object, object, Scope> InputDt { get; set; }
        public Action<InstanceConnector, object, object, Scope> OutputDt { get; set; }
        public Action<InstanceConnector, InstanceConnection<TNode>, Scope> InputTransform { get; set; }
        public Action<InstanceConnector, InstanceConnection<TNode>, Scope> OutputTransform { get; set; }
    }

    public interface IInstanceTransform<TNode>
    {
        TNode transform(string id, object instance, TNode node, IEnumerable<InstanceConnection<TNode>> connections, Scope scope);
        InstanceConnector output(
            string connector,
            out Action<InstanceConnector, object, object, Scope> dt,
            out Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform);
        InstanceConnector input(
            string connector,
            out Action<InstanceConnector, object, object, Scope> dt,
            out Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform);
    }

    public interface IInstanceMatch<TNode>
    {
        IInstanceMatch<TNode> input(
            InstanceConnector connector, 
            Action<InstanceConnector, object, object, Scope> dt = null,
            Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform = null);
        IInstanceMatch<TNode> output(
            InstanceConnector connector,
            Action<InstanceConnector, object, object, Scope> dt = null,
            Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform = null);

        IInstanceMatch<TNode> input(
            string connectorId,
            Action<InstanceConnector, object, object, Scope> dt = null,
            Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform = null);
        IInstanceMatch<TNode> output(
            string connectorId,
            Action<InstanceConnector, object, object, Scope> dt = null,
            Action<InstanceConnector, InstanceConnection<TNode>, Scope> transform = null);

        IInstanceMatch<TNode> input(Action<InstanceConnection<TNode>, Scope> transform);
        IInstanceMatch<TNode> output(Action<InstanceConnection<TNode>, Scope> transform);

        IInstanceAnalisys<TNode> then(Func<string, object, TNode, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler);
        IInstanceAnalisys<TNode> then(Func<string, object, IEnumerable<InstanceConnection<TNode>>, Scope, TNode> handler);
        IInstanceAnalisys<TNode> then(Func<string, object, TNode, Scope, TNode> handler);
        IInstanceAnalisys<TNode> then(Func<string, object, Scope, TNode> handler);
        IInstanceAnalisys<TNode> then(IInstanceTransform<TNode> transform);
    }


    public interface IInstanceAnalisys<TNode>
    {
        IInstanceMatch<TNode> match(Func<string, object, Scope, bool> handler);
        IInstanceMatch<TNode> match<Model>();
        IInstanceMatch<TNode> match(string id);
        void then(Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> transform);
    }

    public interface IInstanceDocument<TNode>
    {
        void change(Func<string, object, Scope, bool> match, IInstanceTransform<TNode> transform);
        void change(Func<IDictionary<string, Tuple<object, TNode>>, Scope, TNode> transform);
    }

}
