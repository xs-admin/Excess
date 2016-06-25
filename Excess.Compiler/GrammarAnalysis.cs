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

    public interface IGrammarAnalysis<TGrammar, GNode, TToken, TNode>
    {
        IGrammarAnalysis<TGrammar, GNode, TToken, TNode> transform<T>(Func<T, Func<GNode, Scope, TNode>, Scope, TNode> handler) where T : GNode;

        void then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler);
    }

    public interface IIndentationGrammarMatch<TToken, TNode>
    {
        IIndentationGrammarAnalysis<TToken, TNode> children(Action<IIndentationGrammarAnalysis<TToken, TNode>> handler);
        IIndentationGrammarAnalysis<TToken, TNode> then(Func<TNode, TNode, Scope, LexicalExtension<TToken>, TNode> handler);
    }

    public interface IIndentationGrammarAnalysis<TToken, TNode>
    {
        IIndentationGrammarMatch<TToken, TNode> match(Func<string, TNode> linker);
        IIndentationGrammarMatch<TToken, TNode> match<T>(Func<T, bool> matcher) where T : TNode;
        IIndentationGrammarMatch<TToken, TNode> match<T>(Func<string, T> parser, Func<T, TNode> transform);
        IIndentationGrammarMatch<TToken, TNode> match(Func<string, bool> parser, Func<TNode> transform);

        IIndentationGrammarAnalysis<TToken, TNode> before(Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler);
        IIndentationGrammarAnalysis<TToken, TNode> after(Func<TNode, TNode, Scope, TNode> handler);
    }
}
