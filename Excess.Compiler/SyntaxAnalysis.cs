using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface ISyntacticalMatch<TNode>
    {
        ISyntacticalMatch<TNode> children(Func<TNode, bool> handler, string named = null);
        ISyntacticalMatch<TNode> children<T>(Func<T, bool> handler = null, string named = null) where T : TNode;
        ISyntacticalMatch<TNode> descendants(Func<TNode, bool> handler, string named = null);
        ISyntacticalMatch<TNode> descendants<T>(Func<T, bool> handler = null, string named = null) where T : TNode;

        ISyntaxAnalysis<TNode> then(Func<TNode, TNode> handler);
        ISyntaxAnalysis<TNode> then(Func<ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> then(ISyntaxTransform<TNode> transform);

        bool matches(TNode node, ISyntacticalMatchResult<TNode> result);
        TNode transform(TNode node, ISyntacticalMatchResult<TNode> result);
    }

    public interface ISyntacticalMatchResult<TNode>
    {
        TNode Node { get; set; }
        Scope Scope { get; set; }
        IEventBus Events { get; set; }
        bool Preprocess { get; set; }

        TNode schedule(string pass, TNode node, Func<TNode, TNode> handler);
    }

    public interface ISyntaxTransform<TNode>
    {
        ISyntaxTransform<TNode> remove(string nodes);
        ISyntaxTransform<TNode> remove(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxTransform<TNode> replace(string nodes, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxTransform<TNode> replace(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> selector, Func<TNode, TNode> handler);
        ISyntaxTransform<TNode> addToScope(string nodes, bool type = false, bool @namespace = false);
        ISyntaxTransform<TNode> addToScope(Func<ISyntacticalMatchResult<TNode>, IEnumerable<TNode>> handler, bool type = false, bool @namespace = false);

        TNode transform(ISyntacticalMatchResult<TNode> result);
    }

    public class SyntacticalExtension<TNode>
    {
        public ExtensionKind Kind { get; set; }
        public string Keyword { get; set; }
        public string Identifier { get; set; }
        public TNode Arguments { get; set; }
        public TNode Body { get; set; }
        public Func<ISyntacticalMatchResult<TNode>, SyntacticalExtension<TNode>, TNode> Handler { get; set; }
    }

    public interface ISyntaxAnalysis<TNode>
    {
        ISyntaxAnalysis<TNode> looseStatements(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseMembers(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> looseTypes(Func<IEnumerable<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> extension(string keyword, ExtensionKind kind, Func<ISyntacticalMatchResult<TNode>, SyntacticalExtension<TNode>, TNode> handler);
        ISyntaxAnalysis<TNode> extension(string keyword, ExtensionKind kind, Func<TNode, SyntacticalExtension<TNode>, TNode> handler);

        ISyntacticalMatch<TNode> match(Func<TNode, bool> selector);
        ISyntacticalMatch<TNode> match<T>(Func<T, bool> selector) where T : TNode;
        ISyntacticalMatch<TNode> match<T>() where T : TNode;

        ISyntaxTransform<TNode> transform();
        ISyntaxTransform<TNode> transform(Func<ISyntacticalMatchResult<TNode>, TNode> handler);
        ISyntaxTransform<TNode> transform(Func<TNode, TNode> handler);

        IEnumerable<CompilerEvent> produce();
    }
}
