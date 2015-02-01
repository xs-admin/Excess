using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public interface ISemanticAnalysis<TToken, TNode, TModel>
    {
        ISemanticAnalysis<TToken, TNode, TModel> error(string error, Action<TNode, Scope> handler);
        ISemanticAnalysis<TToken, TNode, TModel> error(string error, Action<TNode, TModel, Scope> handler);
    }
}
