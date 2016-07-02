using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface IGrammar<TToken, TNode, GNode>
    {
        GNode Parse(IEnumerable<TToken> tokens, Scope scope, int offset);
    }

    public interface IGrammarAnalysis<GNode, TToken, TNode>
    {
        IGrammarAnalysis<GNode, TToken, TNode> transform<T>(Func<T, Func<GNode, Scope, TNode>, Scope, TNode> handler) where T : GNode;

        void then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler);
    }

    public interface IIndentationGrammarMatch<TToken, TNode, GNode>
    {
        IIndentationGrammarAnalysis<TToken, TNode, GNode> children(Action<IIndentationGrammarMatchChildren<TToken, TNode, GNode>> builder);
    }

    public interface IIndentationGrammarMatchChildren<TToken, TNode, GNode>
    {
        IIndentationGrammarMatch<TToken, TNode, GNode> match<TParent, T>(Func<string, TParent, Scope, T> handler) where T : GNode;
    }

    public interface IIndentationGrammarAnalysis<TToken, TNode, GNode> : IIndentationGrammarMatchChildren<TToken, TNode, GNode>
    {
        IGrammarAnalysis<GNode, TToken, TNode> then();
    }
}
