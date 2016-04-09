using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.VS
{
    class ExcessSource : Source
    {
        VisualStudioWorkspace _workspace;
        DocumentId _id;
        RoslynDocument _document;
        public ExcessSource(LanguageService service, IVsTextLines textLines, Colorizer colorizer, VisualStudioWorkspace workspace, DocumentId id) :
            base(service, textLines, colorizer)
        {
            _workspace = workspace;
            _id = id;
        }

        public override void BeginParse()
        {
            if (_document != null)
            {
                //td: cancel 
            }

            ExcessLanguageService service = (ExcessLanguageService)this.LanguageService;
            _document = service.CreateExcessDocument(GetText(), _id);
            _document.applyChanges(CompilerStage.Syntactical);

            if (!_document.HasSemanticalChanges())
            {
                saveCodeBehind(_document); //td: check error
                return; 
            }

            var doc = _workspace.CurrentSolution.GetDocument(_id);
            var semanticRoot = doc.GetSyntaxRootAsync().Result;

            _document.Mapper.SemanticalChange(_document.SyntaxRoot, semanticRoot);

            var model = doc.GetSemanticModelAsync().Result;
            _document.Model = model;
            _document.applyChanges(CompilerStage.Semantical);

            saveCodeBehind(_document); //td: check error
            base.BeginParse();
        }

        private bool saveCodeBehind(RoslynDocument doc)
        {
            doc.SyntaxRoot = doc.SyntaxRoot
                .NormalizeWhitespace(elasticTrivia: true); //td: optimize

            var solution = _workspace.CurrentSolution.WithDocumentSyntaxRoot(_id, _document.SyntaxRoot);
            return _workspace.TryApplyChanges(solution);
        }

        public override void OnIdle(bool periodic)
        {
            if (this.CompletedFirstParse)
            {
                base.OnIdle(periodic);
                LastParseTime = 0;
            }
            else if (!periodic || this.LanguageService == null || this.LanguageService.LastActiveTextView == null || (this.IsCompletorActive) || (!this.IsDirty || this.LanguageService.IsParsing))
            {
                //compile the first time
                this.BeginParse();
            }
        }
    }
}
