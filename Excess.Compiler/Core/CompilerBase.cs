using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class CompilerBase<TToken, TNode, TModel> : ICompiler<TToken, TNode, TModel>
    {
        protected ILexicalAnalysis<TToken, TNode, TModel> _lexical;
        protected ISyntaxAnalysis<TToken, TNode, TModel>  _sintaxis;
        protected IEventBus                _events = new BaseEventBus();
        protected CompilerStage            _stage  = CompilerStage.Started;

        public CompilerBase(ILexicalAnalysis<TToken, TNode, TModel> lexical, ISyntaxAnalysis<TToken, TNode, TModel> sintaxis)
        {
            _lexical  = lexical;
            _sintaxis = sintaxis;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> Lexical()
        {
            return _lexical;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> Sintaxis()
        {
            return _sintaxis;
        }

        ICompilerPass _pass;
        public ICompilerPass Compile(string text, CompilerStage stage)
        {
            Debug.Assert(_pass == null);

            _pass = initialPass(text);
            if (_pass.Stage < stage)
                _pass = Advance(stage);

            return _pass;
        }

        public ICompilerPass CompileAll(string text)
        {
            return Compile(text, CompilerStage.Finished);
        }

        Scope _scope = new Scope(); //td: !! scope tree
        public ICompilerPass Advance(CompilerStage stage)
        {
            while (_pass != null && _pass.Stage < stage)
            {
                _pass = _pass.Compile(_events, _scope);
            }

            return _pass;
        }

        public abstract ICompilerPass initialPass(string text);
    }
}
