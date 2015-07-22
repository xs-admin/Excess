using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.LanguageServices;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Settings;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Entensions.XS;

namespace Excess.VS
{
    class ExcessLanguageService : LanguageService
    {
        VisualStudioWorkspace _workspace;
        public ExcessLanguageService(VisualStudioWorkspace workspace)
        {
            _workspace = workspace;
        }

        public override string Name
        {
            get
            {
                return "xs";
            }
        }


        Dictionary<ProjectId, RoslynCompiler> _projects = new Dictionary<ProjectId, RoslynCompiler>();
        public RoslynDocument CreateExcessDocument(string text, DocumentId document)
        {
            RoslynCompiler compiler;
            if (!_projects.TryGetValue(document.ProjectId, out compiler))
            {
                compiler = new RoslynCompiler();
                XSLang.Apply(compiler);
                _projects[document.ProjectId] = compiler;
            }

            var result = new RoslynDocument(compiler.Scope, text);
            result.Mapper = new MappingService();
            compiler.apply(result);
            return result;
        }

        public override string GetFormatFilterList()
        {
            throw new NotImplementedException();
        }

        private LanguagePreferences _preferences;
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_preferences == null)
                _preferences = new LanguagePreferences(
                    this.Site,
                    typeof(ExcessLanguageService).GUID,
                    this.Name);

            return _preferences;
        }

        Scanner _scanner;
        public override IScanner GetScanner(IVsTextLines buffer)
        {
            if (_scanner == null)
                _scanner = new Scanner(buffer);

            return _scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new ExcessAuthoringScope();
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            var filePath = FilePathUtilities.GetFilePath(buffer) + ".cs";
            ImmutableArray<DocumentId> documents = _workspace.CurrentSolution.GetDocumentIdsWithFilePath(filePath);

            var documentId = documents[0];
            return new ExcessSource(this, buffer, new Colorizer(this, buffer, GetScanner(buffer)), _workspace, documentId);
        }
    }
}
