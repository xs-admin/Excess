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
        ISyntaxAnalysis<TNode> then(Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> then(ISyntaxTransform<TNode> transform);

        bool matches(TNode node, ISyntacticalMatchResult<TNode> result);
        TNode transform(TNode node, ISyntacticalMatchResult<TNode> result);
    }

    public interface ISyntacticalMatchResult<TNode>
    {
        TNode Node { get; set; }
        bool Preprocess { get; set; }

        void matchChildren(ISyntacticalMatch<TNode> match);
        void matchDescendants(ISyntacticalMatch<TNode> match);
        dynamic context();

        void   set(string name, object value);
        object get(string name);
    }

    public interface ISyntaxTransform<TNode>
    {
        ISyntaxTransform<TNode> insert();
        ISyntaxTransform<TNode> replace();
        ISyntaxTransform<TNode> remove();

        TNode transform(TNode node, ISyntacticalMatchResult<TNode> result);
    }

    public interface ISyntaxAnalysis<TNode>
    {
        ISyntaxAnalysis<TNode> looseStatements(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseMembers(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseTypes(Func<IEnumerable<TNode>, TNode> handler);

        ISyntacticalMatch<TNode> match(Func<TNode, bool> handler, string named = null, string add = null);
        ISyntacticalMatch<TNode> match<T>(Func<T, bool> handler, string named = null, string add = null) where T : TNode;
        ISyntacticalMatch<TNode> match<T>(string named = null, string add = null) where T : TNode;
        ISyntacticalMatch<TNode> match(string named = null, string add = null);

        IEnumerable<ISyntacticalMatch<TNode>> consume();

        ISyntaxTransform<TNode> transform();

        TNode normalize(TNode node);
    }
}
