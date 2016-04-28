using System;

namespace Excess.Compiler
{
    public interface ICompilationMatch<TToken, TNode, TCompilation>
    {
        ICompilationAnalysis<TToken, TNode, TCompilation> then(Action<TNode, TCompilation, Scope> handler);

        bool matched(TNode node, TCompilation compilation, Scope scope);
    }

    public interface ICompilationAnalysis<TToken, TNode, TCompilation>
    {
        ICompilationMatch<TToken, TNode, TCompilation> match<T>(Func<T, TCompilation, Scope, bool> matcher) where T : TNode;
        ICompilationAnalysis<TToken, TNode, TCompilation> after(Action<TCompilation, Scope> handler);
    }
}
