using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseGrammarAnalysis<TToken, TNode, TModel, GNode, TGrammar> : IGrammarAnalysis<TGrammar, GNode, TToken, TNode> where TGrammar : IGrammar<TToken, TNode, GNode>, new()
    {
        TGrammar _grammar;
        public BaseGrammarAnalysis(ILexicalAnalysis<TToken, TNode, TModel> lexical, string keyword, ExtensionKind kind)
        {
            lexical.extension(keyword, kind, ParseExtension);
            _grammar = new TGrammar();
        }

        Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> _then;
        public void then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            _then = handler;
        }

        private TNode ParseExtension(TNode node, Scope scope, LexicalExtension<TToken> extension)
        {
            var allTokens = extension.Body.ToArray();
            var withoutBraces = Range(allTokens, 1, allTokens.Length - 1);
            if (!withoutBraces.Any())
                return node; 

            var compiler = scope.GetService<TToken, TNode, TModel>();
            var g        = _grammar.Parse(withoutBraces, scope, compiler.GetOffset(withoutBraces.First()));

            if (g == null || g.Equals(default(GNode)))
                return node; //errors added to the scope already

            var result = doTransform(g, scope);
            if (_then != null)
                result = _then(node, result, scope, extension);

            return result;
        }

        private IEnumerable<TToken> Range(TToken[] tokens, int from, int to)
        {
            for (int i = from; i < to; i++)
                yield return tokens[i];
        }

        Dictionary<Type, Func<GNode, Func<GNode, Scope, TNode>, Scope, TNode>> _transformers = new Dictionary<Type, Func<GNode, Func<GNode, Scope, TNode>, Scope, TNode>>();
        public IGrammarAnalysis<TGrammar, GNode, TToken, TNode> transform<T>(Func<T, Func<GNode, Scope, TNode>, Scope, TNode> handler) where T : GNode
        {
            var type = typeof(T);
            if (_transformers.ContainsKey(type))
                throw new InvalidOperationException("multiple type handlers");

            _transformers[type] = (node, grammar, scope) => handler((T)node, doTransform, scope);
            return this;
        }

        private TNode doTransform(GNode g, Scope scope)
        {
            var type = g.GetType();
            var handler = null as Func<GNode, Func<GNode, Scope, TNode>, Scope, TNode>;
            if (_transformers.TryGetValue(type, out handler))
                return handler(g, doTransform, scope);

            return default(TNode);
        }
    }
}
