using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface ICompilationMatch<TToken, TNode, TModel>
    {
        ICompilationAnalysis<TToken, TNode, TModel> then(Action<TNode, TModel, Scope> handler);

        bool matched(TNode node, TModel model, Scope scope);
    }

    public interface ICompilationAnalysis<TToken, TNode, TModel>
    {
        ICompilationMatch<TToken, TNode, TModel> match<T>(Func<T, TModel, Scope, bool> matcher) where T : TNode;
    } 
}
