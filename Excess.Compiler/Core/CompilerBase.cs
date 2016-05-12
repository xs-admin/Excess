using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class CompilerBase<TToken, TNode, TModel> 
        : ICompiler<TToken, TNode, TModel>,
          IDocumentInjector<TToken, TNode, TModel>
    {
        protected ILexicalAnalysis<TToken, TNode, TModel>  _lexical;
        protected ISyntaxAnalysis<TToken, TNode, TModel>   _syntax;
        protected ISemanticAnalysis<TToken, TNode, TModel> _semantics;
        protected IInstanceAnalisys<TNode>                 _instance;     
        protected ICompilerEnvironment                     _environment;
        protected CompilerStage                            _stage  = CompilerStage.Started;
        protected IDocument<TToken, TNode, TModel>         _document;
        protected Scope                                    _scope;

        public CompilerBase(ILexicalAnalysis<TToken, TNode, TModel> lexical, 
                            ISyntaxAnalysis<TToken, TNode, TModel> syntax, 
                            ISemanticAnalysis<TToken, TNode, TModel> semantics,
                            ICompilerEnvironment environment,
                            IInstanceAnalisys<TNode> instance,
                            Scope scope)
        {
            _lexical  = lexical;
            _syntax = syntax;
            _semantics = semantics;
            _environment = environment;
            _instance = instance;
            _scope = new Scope(scope); 
        }

        public Scope Scope { get { return _scope; } }

        public ILexicalAnalysis<TToken, TNode, TModel> Lexical()
        {
            return _lexical;
        }

        public ISyntaxAnalysis<TToken, TNode, TModel> Syntax()
        {
            return _syntax;
        }

        public ISemanticAnalysis<TToken, TNode, TModel> Semantics()
        {
            return _semantics;
        }

        public IInstanceAnalisys<TNode> Instance()
        {
            return _instance;
        }

        public ICompilerEnvironment Environment()
        {
            return _environment;
        }

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            var iLexical = _lexical as IDocumentInjector<TToken, TNode, TModel>;
            if (iLexical != null)
                iLexical.apply(document);

            var iSyntax = _syntax as IDocumentInjector<TToken, TNode, TModel>;
            if (iSyntax != null)
                iSyntax.apply(document);

            var iSemantics = _semantics as IDocumentInjector<TToken, TNode, TModel>;
            if (iSemantics != null)
                iSemantics.apply(document);

            if (document is IInstanceDocument<TNode>)
            {
                var iInstance = _instance as IDocumentInjector<TToken, TNode, TModel>;
                if (iInstance != null)
                    iInstance.apply(document);
            }

            var iEnvironment = _environment as IDocumentInjector<TToken, TNode, TModel>;
            if (iEnvironment != null)
                iEnvironment.apply(document);
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

    public class DelegateInjector<TToken, TNode, TModel> : ICompilerInjector<TToken, TNode, TModel>
    {
        Action<ICompiler<TToken, TNode, TModel>> _delegate;
        public DelegateInjector(Action<ICompiler<TToken, TNode, TModel>> @delegate)
        {
            _delegate = @delegate;
        }

        public void apply(ICompiler<TToken, TNode, TModel> compiler)
        {
            _delegate(compiler);
        }
    }

    public class CompositeInjector<TToken, TNode, TModel> : ICompilerInjector<TToken, TNode, TModel>
    {
        IEnumerable<ICompilerInjector<TToken, TNode, TModel>> _injectors;
        public CompositeInjector(IEnumerable<ICompilerInjector<TToken, TNode, TModel>> injectors)
        {
            _injectors = injectors;
        }

        public void apply(ICompiler<TToken, TNode, TModel> compiler)
        {
            foreach (var injector in _injectors)
                injector.apply(compiler);
        }
    }
    
}
