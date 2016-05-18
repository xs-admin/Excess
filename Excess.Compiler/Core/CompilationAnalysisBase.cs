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
        Func<TNode, ICompilation<TToken, TNode, TModel>, Scope, bool> _eval;
        public CompilationMatch(ICompilationAnalysis<TToken, TNode, TModel> owner, Func<TNode, ICompilation<TToken, TNode, TModel>, Scope, bool> eval)
        {
            _owner = owner;
            _eval = eval;
        }

        Action<TNode, ICompilation<TToken, TNode, TModel>, Scope> _exec;
        public ICompilationAnalysis<TToken, TNode, TModel> then(Action<TNode, ICompilation<TToken, TNode, TModel>, Scope> handler)
        {
            Debug.Assert(_exec == null);
            _exec = handler;
            return _owner;
        }

        public bool matched(TNode node, ICompilation<TToken, TNode, TModel> compilation, Scope scope)
        {
            if (_eval(node, compilation, scope))
            {
                _exec?.Invoke(node, compilation, scope);
                return true;
            }

            return false;
        }
    }

    public class CompilationAnalysisBase<TToken, TNode, TModel> : ICompilationAnalysis<TToken, TNode, TModel>
    {
        public CompilationAnalysisBase()
        {
        }

        protected List<ICompilationMatch<TToken, TNode, TModel>> _matchers = new List<ICompilationMatch<TToken, TNode, TModel>>();
        protected List<Action<ICompilation<TToken, TNode, TModel>, Scope>> _after = new List<Action<ICompilation<TToken, TNode, TModel>, Scope>>();
        ICompilationMatch<TToken, TNode, TModel> ICompilationAnalysis<TToken, TNode, TModel>.match<T>(Func<T, ICompilation<TToken, TNode, TModel>, Scope, bool> matcher) 
        {
            var result = new CompilationMatch<TToken, TNode, TModel>(this, (node, compilation, scope) =>
            {
                if (node is T)
                    return matcher((T)node, compilation, scope);

                return false;
            });

            _matchers.Add(result);
            return result;
        }


        public ICompilationAnalysis<TToken, TNode, TModel> after(Action<ICompilation<TToken, TNode, TModel>, Scope> handler)
        {
            if (!_after.Contains(handler))
                _after.Add(handler); //td: !!! multiples
            return this;
        }

        //internal use
        public bool Analyze(TNode node, ICompilation<TToken, TNode, TModel> compilation, Scope scope)
        {
            var result = false;
            foreach (var matcher in _matchers)
            {
                result = result || matcher.matched(node, compilation, scope);
            }

            return result;
        }

        public bool isNeeded()
        {
            return _matchers.Any() || _after.Any();
        }

        public void Finish(ICompilation<TToken, TNode, TModel> compilation, Scope scope)
        {
            foreach (var after in _after)
                after(compilation, scope);
        }
    }
}
