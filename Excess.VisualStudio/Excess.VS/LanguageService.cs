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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Excess.VS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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

        class ProjectCache
        {
            VisualStudioWorkspace _workspace;
            public ProjectCache(VisualStudioWorkspace workspace)
            {
                _workspace = workspace;
            }

            Project _project;
            Dictionary<long, RoslynCompiler> _cache = new Dictionary<long, RoslynCompiler>();
            Dictionary<string, Action<RoslynCompiler>> _extensions = new Dictionary<string, Action<RoslynCompiler>>();


            public RoslynCompiler GetCompiler(
                ProjectId projectId, 
                DocumentId documentId, 
                ICollection<UsingDirectiveSyntax> extensions)
            {
                var project = _workspace.CurrentSolution.GetProject(projectId);
                if (_project != project)
                {
                    _project = project;
                    updateExtensions();
                }

                //get an unique id 
                var extensionNames = extensions
                    .Where(@using => isExtension(@using))
                    .Select(@using => @using.Name.ToString());

                var hashCode = 0;
                foreach (var extension in extensionNames)
                    hashCode += extension.GetHashCode();

                //test the cache for some combination of extensions
                var result = null as RoslynCompiler;
                if (_cache.TryGetValue(hashCode, out result))
                    return result;

                result = new RoslynCompiler();
                XSLang.Apply(result);

                foreach (var extension in extensions.ToArray())
                {
                    var extensionName = extension.Name.ToString();
                    var extensionFunc = null as Action<RoslynCompiler>;
                    if (_extensions.TryGetValue(extensionName, out extensionFunc))
                        extensionFunc(result);
                    else
                        extensions.Remove(extension);
                }

                return result;
            }

            private void updateExtensions()
            {
                Debug.Assert(_project != null);

                var newDict = new Dictionary<string, Action<RoslynCompiler>>();
                foreach (var reference in _project.AnalyzerReferences)
                {
                    string name;
                    if (isExtension(reference, out name))
                    {
                        var function = null as Action<RoslynCompiler>;
                        if (!_extensions.TryGetValue(name, out function))
                            function = loadReference(reference);

                        newDict[name] = function;
                    }
                }

                _extensions = newDict;
            }

            private Action<RoslynCompiler> loadReference(AnalyzerReference reference)
            {
                throw new NotImplementedException();
            }

            private bool isExtension(AnalyzerReference reference, out string name)
            {
                throw new NotImplementedException();
            }

            private bool isExtension(UsingDirectiveSyntax @using)
            {
                throw new NotImplementedException();
            }
        }

        Dictionary<ProjectId, ProjectCache> _projects = new Dictionary<ProjectId, ProjectCache>();
        public RoslynDocument CreateExcessDocument(string text, DocumentId document)
        {
            ProjectCache cache;
            if (!_projects.TryGetValue(document.ProjectId, out cache))
            {
                cache = new ProjectCache(_workspace);
                _projects[document.ProjectId] = cache;
            }

            var compilationUnit = CSharp.ParseCompilationUnit(text);
            var extensions = new List<UsingDirectiveSyntax>(compilationUnit.Usings);
            var compiler = cache.GetCompiler(document.ProjectId, document, extensions);
            var result = new RoslynDocument(compiler.Scope, compilationUnit);
            result.Mapper = new MappingService();
            compiler.apply(result);
            return result;
        }

        public override string GetFormatFilterList()
        {
            return "XS files (*.xs)\n*.xs\n";
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
