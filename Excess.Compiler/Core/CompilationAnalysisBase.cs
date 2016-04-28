using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class CompilationMatch<TToken, TNode, TModel> : ICompilationMatch<TToken, TNode, TModel>
    {
        ICompilationAnalysis<TToken, TNode, TModel> _owner;
        Func<TNode, TModel, Scope, bool> _eval;
        public CompilationMatch(ICompilationAnalysis<TToken, TNode, TModel> owner, Func<TNode, TModel, Scope, bool> eval)
        {
            _owner = owner;
            _eval = eval;
        }

        Action<TNode, TModel, Scope> _exec;
        public ICompilationAnalysis<TToken, TNode, TModel> then(Action<TNode, TModel, Scope> handler)
        {
            Debug.Assert(_exec == null);
            _exec = handler;
            return _owner;
        }

        public bool matched(TNode node, TModel model, Scope scope)
        {
            if (_eval(node, model, scope))
            {
                if (_exec != null)
                    _exec(node, model, scope);

                return true;
            }

            return false;
        }
    }

    public abstract class CompilationAnalysisBase<TToken, TNode, TCompilation> : ICompilationAnalysis<TToken, TNode, TCompilation>
    {
        protected List<ICompilationMatch<TToken, TNode, TCompilation>> _matchers = new List<ICompilationMatch<TToken, TNode, TCompilation>>();
        protected List<Action<TCompilation, Scope>> _after = new List<Action<TCompilation, Scope>>();
        ICompilationMatch<TToken, TNode, TCompilation> ICompilationAnalysis<TToken, TNode, TCompilation>.match<T>(Func<T, TCompilation, Scope, bool> matcher) 
        {
            var result = new CompilationMatch<TToken, TNode, TCompilation>(this, (node, model, scope) =>
            {
                if (node is T)
                    return matcher((T)node, model, scope);

                return false;
            });

            _matchers.Add(result);
            return result;
        }


        public ICompilationAnalysis<TToken, TNode, TCompilation> after(Action<TCompilation, Scope> handler)
        {
            if (!_after.Contains(handler))
                _after.Add(handler); //td: !!! multiples
            return this;
        }
    }
}
