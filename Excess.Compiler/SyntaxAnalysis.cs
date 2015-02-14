using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface ISyntacticalMatch<TToken, TNode, TModel>
    {
        ISyntacticalMatch<TToken, TNode, TModel> children(Func<TNode, bool> handler, string named = null);
        ISyntacticalMatch<TToken, TNode, TModel> children<T>(Func<T, bool> handler = null, string named = null) where T : TNode;
        ISyntacticalMatch<TToken, TNode, TModel> descendants(Func<TNode, bool> handler, string named = null);
        ISyntacticalMatch<TToken, TNode, TModel> descendants<T>(Func<T, bool> handler = null, string named = null) where T : TNode;

        ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode> handler);
        ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler);
        ISyntaxAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler);
        ISyntaxAnalysis<TToken, TNode, TModel> then(ISyntaxTransform<TNode> transform);
    }

    public interface ISyntaxTransform<TNode>
    {
        ISyntaxTransform<TNode> remove(string nodes);
        ISyntaxTransform<TNode> remove(Func<TNode, Scope, IEnumerable<TNode>> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<TNode, Scope, TNode> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, Scope, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<TNode, Scope, IEnumerable<TNode>> selector, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false);
        ISyntaxTransform<TNode> addToScope(Func<TNode, Scope, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false);

        TNode transform(TNode node, Scope scope);

    }

    public class SyntacticalExtension<TNode>
    {
        public SyntacticalExtension()
        {
        }

        public SyntacticalExtension(string keyword, ExtensionKind kind, Func<TNode, Scope, SyntacticalExtension<TNode>, TNode> handler)
        {
            Keyword = keyword;
            Kind = kind;
            Handler = handler;
        }

        public ExtensionKind Kind { get; set; }
        public string Keyword { get; set; }
        public string Identifier { get; set; }
        public TNode Arguments { get; set; }
        public TNode Body { get; set; }
        public Func<TNode, Scope, SyntacticalExtension<TNode>, TNode> Handler { get; set; }
    }

    public interface ISyntaxAnalysis<TToken, TNode, TModel>
    {
        ISyntaxAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, Scope, SyntacticalExtension<TNode>, TNode> handler);
        ISyntaxAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, SyntacticalExtension<TNode>, TNode> handler);

        ISyntacticalMatch<TToken, TNode, TModel> match(Func<TNode, bool> selector);
        ISyntacticalMatch<TToken, TNode, TModel> match<T>(Func<T, bool> selector) where T : TNode;
        ISyntacticalMatch<TToken, TNode, TModel> match<T>() where T : TNode;

        ISyntaxTransform<TNode> transform();
        ISyntaxTransform<TNode> transform(Func<TNode, Scope, TNode> handler);
        ISyntaxTransform<TNode> transform(Func<TNode, TNode> handler);
    }
}
