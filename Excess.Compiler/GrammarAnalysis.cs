using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface IGrammar<TToken, TNode, GNode>
    {
        GNode Parse(LexicalExtension<TToken> tokens, Scope scope);
    }

    public interface IGrammarAnalysis<GNode, TToken, TNode>
    {
        IGrammarAnalysis<GNode, TToken, TNode> transform<T>(Func<T, Func<GNode, Scope, TNode>, Scope, TNode> handler) where T : GNode;

        void then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler);
    }

    public interface IIndentationGrammarAnalysis<TToken, TNode, GNode>
    {
        IIndentationGrammarAnalysis<TToken, TNode, GNode> match<TParent, T>(
            Func<string, TParent, Scope, T> handler,
            Action<IIndentationGrammarAnalysis<TToken, TNode, GNode>> children = null) where T : GNode;

        void match_parent();

        IGrammarAnalysis<GNode, TToken, TNode> then();
    }
}
