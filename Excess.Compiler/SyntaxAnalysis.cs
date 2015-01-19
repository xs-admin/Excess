using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface ISyntacticalMatch<TNode>
    {
        ISyntacticalMatch<TNode> children(Func<TNode, bool> handler);
        ISyntacticalMatch<TNode> children<T>(Func<T, bool> handler = null) where T : TNode;
        ISyntacticalMatch<TNode> children();
        ISyntacticalMatch<TNode> descendants(Func<TNode, bool> handler);
        ISyntacticalMatch<TNode> descendants<T>(Func<T, bool> handler = null) where T : TNode;
        ISyntacticalMatch<TNode> descendants();
        ISyntacticalMatch<TNode> parent();

        ISyntaxAnalysis<TNode> then(Func<TNode, TNode> handler);
        ISyntaxAnalysis<TNode> then(ISyntaxTransform transform);
    }

    public interface ISyntacticalMatchResult<TNode>
    {
        void matchChildren(ISyntacticalMatch<TNode> match);
        void matchDescendants(ISyntacticalMatch<TNode> match);

        dynamic context(TNode node);
    }

    public interface ISyntaxTransform
    {
        ISyntaxTransform insert();
        ISyntaxTransform replace();
        ISyntaxTransform remove();
    }

    public interface ISyntaxAnalysis<TNode>
    {
        ISyntaxAnalysis<TNode> looseStatements(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseMembers(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseTypes(Func<IEnumerable<TNode>, TNode> handler);

        ISyntacticalMatch<TNode> match<T>(Func<T, bool> handler) where T : TNode;
        ISyntacticalMatch<TNode> match();
        ISyntacticalMatch<TNode> matchCodeDSL(string dsl);
        ISyntacticalMatch<TNode> matchTypeDSL(string dsl);
        ISyntacticalMatch<TNode> matchMemberDSL(string dsl);
        ISyntacticalMatch<TNode> matchNamespaceDSL(string dsl);

        IEnumerable<ISyntacticalMatch<TNode>> consume();

        ISyntaxTransform transform();

        TNode normalize(TNode node);
    }
}
