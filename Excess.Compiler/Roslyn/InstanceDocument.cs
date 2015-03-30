using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    public class RoslynInstanceDocument : RoslynDocument, IInstanceDocument<SyntaxNode>
    {

        Func<string, IDictionary<string, object>, ICollection<Connection>, Scope, bool> _parser;
        public RoslynInstanceDocument(Func<string, IDictionary<string, object>, ICollection<Connection>, Scope, bool> parser, Scope scope = null) : base(scope)
        {
            _parser = parser;
        }

        InstanceDocumentBase<SyntaxNode> _instance = new InstanceDocumentBase<SyntaxNode>();
        public void change(Func<string, object, Scope, bool> match, IInstanceTransform<SyntaxNode> transform)
        {
            _instance.change(match, transform);
        }

        public void change(Func<IDictionary<string, Tuple<object, SyntaxNode>>, Scope, SyntaxNode> transform)
        {
            _instance.change(transform);
        }

        protected override void applyLexical()
        {
            Debug.Assert(_parser != null);
            var instances = new Dictionary<string, object>();
            var connections = new List<Connection>();
            if (_parser(_text, instances, connections, _scope))
                _root = _instance.transform(instances, connections, _scope);
        }
    }
}
