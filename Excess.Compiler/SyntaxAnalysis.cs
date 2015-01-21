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
        Scope Scope { get; set; }
        bool Preprocess { get; set; }
        IEventBus Events { get; set; }
        void matchChildren(ISyntacticalMatch<TNode> match);
        void matchDescendants(ISyntacticalMatch<TNode> match);
        TNode schedule(string pass, TNode node, Func<TNode, TNode> handler);
    }

    public interface ISyntaxTransform<TNode>
    {
        ISyntaxTransform<TNode> remove(string nodes);
        ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false);
        ISyntaxTransform<TNode> addToScope(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false);

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

        IEnumerable<CompilerEvent> produce();
    }
}
