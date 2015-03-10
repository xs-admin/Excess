using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface IGrammar<TToken, TNode, GNode>
    {
        GNode parse(IEnumerable<TToken> tokens, Scope scope);
    }

    public interface IGrammarAnalysis<TGrammar, GNode, TToken, TNode>
    {
        IGrammarAnalysis<TGrammar, GNode, TToken, TNode> transform<T>(Func<T, Func<GNode, Scope, TNode>, Scope, TNode> handler) where T : GNode;

        //
        void then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler);
    }
}
