using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using System.IO;

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

            var service = (ExcessLanguageService)LanguageService;
            _document = service.CreateExcessDocument(GetText(), _id);
            _document.applyChanges(CompilerStage.Syntactical);

            //td: check saving error & SemanticalChanges
            saveCodeBehind(_document, false);

            if (_document.HasSemanticalChanges())
            {
                var doc = _workspace.CurrentSolution.GetDocument(_id);
                var semanticRoot = doc.GetSyntaxRootAsync().Result;

                _document.Mapper.SemanticalChange(_document.SyntaxRoot, semanticRoot);

                var model = doc.GetSemanticModelAsync().Result;
                _document.Model = model;
                _document.applyChanges(CompilerStage.Semantical);
            }

            saveCodeBehind(_document, true);
            base.BeginParse();
        }

        private bool saveCodeBehind(RoslynDocument doc, bool mapLines)
        {
            doc.SyntaxRoot = doc.SyntaxRoot
                .NormalizeWhitespace(elasticTrivia: true); //td: optimize

            var solution = _workspace.CurrentSolution;
            if (mapLines)
            {
                var mapper = doc.Mapper; 
                var vsDocument = solution.GetDocument(_id);
                var filePath = vsDocument?.FilePath;

                solution = solution.WithDocumentText(_id, SourceText.From(doc.Mapper
                    .MapLines(doc.SyntaxRoot, filePath)));
            }
            else
                solution = solution.WithDocumentSyntaxRoot(_id, _document.SyntaxRoot);

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
