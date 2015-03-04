using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseGrammarAnalysis<TToken, TNode, TModel, GNode, TGrammar> : IGrammarAnalysis<TGrammar, TNode, GNode> where TGrammar : IGrammar<TToken, TNode, GNode>, new()
    {
        TGrammar _grammar;
        public BaseGrammarAnalysis(ILexicalAnalysis<TToken, TNode, TModel> lexical, string keyword, ExtensionKind kind)
        {
            lexical.extension(keyword, kind, ParseExtension);
            _grammar = new TGrammar();
        }

        private TNode ParseExtension(TNode node, Scope scope, LexicalExtension<TToken> extension)
        {
            var g = _grammar.parse(extension.Body, scope);
            if (g.Equals(default(GNode)))
                return node; //errors added to the scope already

            return doTransform(g, node, scope);
        }

        Dictionary<Type, Func<GNode, TNode, Func<GNode, TNode, Scope, TNode>, Scope, TNode>> _transformers = new Dictionary<Type, Func<GNode, TNode, Func<GNode, TNode, Scope, TNode>, Scope, TNode>>();
        public IGrammarAnalysis<TGrammar, TNode, GNode> transform<T>(Func<T, TNode, Func<GNode, TNode, Scope, TNode>, Scope, TNode> handler) where T : GNode
        {
            var type = typeof(T);
            if (!_transformers.ContainsKey(type))
                throw new InvalidOperationException("multiple type handlers");

            _transformers[type] = (node, parent, grammar, scope) => handler((T)node, parent, doTransform, scope);
            return this;
        }

        private TNode doTransform(GNode g, TNode t, Scope scope)
        {
            var type = g.GetType();
            var handler = null as Func<GNode, TNode, Func<GNode, TNode, Scope, TNode>, Scope, TNode>;
            if (_transformers.TryGetValue(type, out handler))
                return handler(g, t, doTransform, scope);

            return default(TNode);
        }
    }
}
