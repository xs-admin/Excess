using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class CompilerBase<TToken, TNode> : ICompiler<TToken, TNode>
    {
        protected ILexicalAnalysis<TToken> _lexical;
        protected ISyntaxAnalysis<TNode>   _sintaxis;
        protected CompilerStage            _stage  = CompilerStage.Started;
        protected EventBus                 _events = new EventBus();

        public CompilerBase(ILexicalAnalysis<TToken> lexical, ISyntaxAnalysis<TNode> sintaxis)
        {
            _lexical  = lexical;
            _sintaxis = sintaxis;
        }

        public ILexicalAnalysis<TToken> Lexical()
        {
            return _lexical;
        }

        public ISyntaxAnalysis<TNode> Syntaxis()
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
