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

    public interface IGrammarAnalysis<TGrammar, TNode, GNode>
    {
        IGrammarAnalysis<TGrammar, TNode, GNode> transform<T>(Func<T, TNode, Func<GNode, TNode, Scope, TNode>, Scope, TNode> handler) where T : GNode;
    }
}
