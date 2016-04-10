using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Excess.Compiler.Roslyn;
using Excess.Entensions.XS;

namespace Excess.VS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    internal static class XSKeywords            
    {
        public static string[] Values = new[]
        {
            "function",
            "method",
            "property",
            "on",
        };
    }


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
            Dictionary<long, IEnumerable<string>> _keywordCache = new Dictionary<long, IEnumerable<string>>();
            Dictionary<DocumentId, long> _documentExtensions = new Dictionary<DocumentId, long>();
            Dictionary<string, Action<RoslynCompiler, IList<string>>> _extensions = new Dictionary<string, Action<RoslynCompiler, IList<string>>>();

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

                long hashCode = 0;
                foreach (var extension in extensionNames)
                    hashCode += extension.GetHashCode();

                //test the cache for some combination of extensions
                var result = null as RoslynCompiler;
                if (_cache.TryGetValue(hashCode, out result))
                    return result;

                result = new RoslynCompiler();
                XSLang.Apply(result);

                var keywords = new List<string>();
                foreach (var extension in extensions.ToArray())
                {
                    var extensionName = extension.Name.ToString();
                    var extensionFunc = null as Action<RoslynCompiler, IList<string>>;
                    if (_extensions.TryGetValue(extensionName, out extensionFunc))
                        extensionFunc(result, keywords);
                    else
                        extensions.Remove(extension);
                }

                _cache[hashCode] = result;
                _keywordCache[hashCode] = keywords;
                return result;
            }

            public IEnumerable<string> Keywords(DocumentId documentId)
            {
                long iid;
                if (!_documentExtensions.TryGetValue(documentId, out iid))
                    return null;

                IEnumerable<string> result = XSKeywords.Values;
                IEnumerable<string> cache;
                if (_keywordCache.TryGetValue(iid, out cache))
                    result = result.Union(cache);

                return result;
            }

            private void updateExtensions()
            {
                Debug.Assert(_project != null);

                var newDict = new Dictionary<string, Action<RoslynCompiler, IList<string>>>();
                foreach (var reference in _project.AnalyzerReferences)
                {
                    string name;
                    if (isExtension(reference, out name))
                    {
                        var function = null as Action<RoslynCompiler, IList<string>>;
                        if (!_extensions.TryGetValue(name, out function))
                            function = loadReference(reference);

                        newDict[name] = function;
                    }
                }

                _extensions = newDict;
            }

            private Action<RoslynCompiler, IList<string>> loadReference(AnalyzerReference reference)
            {
                throw new NotImplementedException();
            }

            private bool isExtension(AnalyzerReference reference, out string name)
            {
                throw new NotImplementedException();
            }

            private bool isExtension(UsingDirectiveSyntax @using) => @using.Name.ToString().StartsWith("xs.");

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
            var result = new RoslynDocument(
                compiler.Scope, 
                compilationUnit
                    .RemoveNodes(extensions, SyntaxRemoveOptions.KeepEndOfLine));

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
                _preferences = new LanguagePreferences(Site, typeof(ExcessLanguageService).GUID, Name);

            return _preferences;
        }

        Dictionary<DocumentId, Scanner> _scannerCache = new Dictionary<DocumentId, Scanner>();
        Scanner _scanner;
        public override IScanner GetScanner(IVsTextLines buffer)
        {
            var documentId = GetDocumentId(buffer);
            if (documentId != null)
            {
                var result = null as Scanner;
                if (_scannerCache.TryGetValue(documentId, out result))
                    return result;

                var scanner = new Scanner(XSKeywords.Values);
                _scannerCache[documentId] = scanner;
                return scanner;
            }

            if (_scanner == null) //td: ?
                _scanner = new Scanner(XSKeywords.Values);

            return _scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new ExcessAuthoringScope();
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            var scanner = GetScanner(buffer) as Scanner;
            Debug.Assert(scanner != null);
            Debug.Assert(scanner.Keywords == XSKeywords.Values);

            var documentId = GetDocumentId(buffer);
            scanner.Keywords = GetKeywords(documentId);
            return new ExcessSource(this, buffer, new Colorizer(this, buffer, scanner), _workspace, documentId);
        }

        private IEnumerable<string> GetKeywords(DocumentId documentId)
        {
            var project = null as ProjectCache;
            if (_projects.TryGetValue(documentId.ProjectId, out project))
                return project.Keywords(documentId);

            return XSKeywords.Values;
        }

        private DocumentId GetDocumentId(IVsTextLines buffer)
        {
            var filePath = FilePathUtilities.GetFilePath(buffer) + ".cs";
            var documents = _workspace
                .CurrentSolution
                .GetDocumentIdsWithFilePath(filePath);

            return documents.Single();
        }
    }
}
