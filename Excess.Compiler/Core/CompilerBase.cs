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
        protected ILexicalAnalysis<TToken, TNode, TModel>  _lexical;
        protected ISyntaxAnalysis<TToken, TNode, TModel>   _sintaxis;
        protected ISemanticAnalysis<TToken, TNode, TModel> _semantics;
        protected CompilerStage                            _stage  = CompilerStage.Started;
        protected IDocument<TToken, TNode, TModel>         _document;
        protected Scope                                    _scope;

        public CompilerBase(ILexicalAnalysis<TToken, TNode, TModel> lexical, ISyntaxAnalysis<TToken, TNode, TModel> sintaxis, ISemanticAnalysis<TToken, TNode, TModel> semantics, Scope scope)
        {
            _lexical  = lexical;
            _sintaxis = sintaxis;
            _semantics = semantics;

            _scope = new Scope(scope); 
        }

        public ILexicalAnalysis<TToken, TNode, TModel> Lexical()
        {
            return _lexical;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> Sintaxis()
        {
            return _sintaxis;
        }

        public ISemanticAnalysis<TToken, TNode, TModel> Semantics()
        {
            return _semantics;
        }


        public bool Compile(string text, CompilerStage stage)
        {
            Debug.Assert(_document == null); //td:
            _document = createDocument();

            _document.applyChanges(stage);
            return _document.hasErrors();
        }

        protected abstract IDocument<TToken, TNode, TModel> createDocument();

        public bool CompileAll(string text)
        {
            return Compile(text, CompilerStage.Finished);
        }

        public bool Advance(CompilerStage stage)
        {
            _document.applyChanges(stage);
            return _document.hasErrors();
        }
    }
}
