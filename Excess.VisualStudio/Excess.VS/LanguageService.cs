using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using NuGet.VisualStudio;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Reflection;
using Excess.Compiler;
using Excess.Compiler.Core;
using xslang;

namespace Excess.VS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using RoslynCompilationAnalysis = CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompilerFunc = Action<RoslynCompiler, Scope>;

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
            ProjectId _project;
            string _path;
            RoslynCompilationAnalysis _compilation;
            public ProjectCache(VisualStudioWorkspace workspace, ProjectId projectId, IVsPackageInstallerServices nuget, IVsPackageInstallerEvents nugetEvents)
            {
                _project = projectId;
                var project = workspace.CurrentSolution.GetProject(projectId);

                _workspace = workspace;
                _path = Path.GetDirectoryName(project.FilePath);

                registerPackages(nuget.GetInstalledPackages());
                nugetEvents.PackageInstalled += nugetEventsPackageInstalled;
            }

            private void registerPackages(IEnumerable<IVsPackageMetadata> packages)
            {
                foreach (var package in packages)
                    addExtension(package);
            }

            private void nugetEventsPackageInstalled(IVsPackageMetadata package)
            {
                addExtension(package);
            }

            private void addExtension(IVsPackageMetadata package)
            {
                //td: determine if the package is installed in this project


                var toolPath = Path.Combine(package.InstallPath, "tools");
                if (!Directory.Exists(toolPath))
                    return; //td: ask to restore packages

                var dlls = Directory.EnumerateFiles(toolPath)
                    .Where(file => Path.GetExtension(file) == ".dll");

                foreach (var dll in dlls)
                {
                    var assembly = Assembly.LoadFrom(dll);
                    if (assembly == null)
                        continue;

                    //var extension = loadReference(dll, name);
                    var name = string.Empty;
                    var compilationFunc = null as Action<RoslynCompilationAnalysis>;
                    var extension = Loader<RoslynCompiler, RoslynCompilationAnalysis>.CreateFrom(assembly, out name, out compilationFunc);
                    if (extension != null)
                    {
                        if (compilationFunc != null)
                        {
                            if (_compilation == null)
                                _compilation = new CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>();

                            compilationFunc(_compilation);
                        }

                        _extensions[name] = extension;
                    }
                }
            }

            Dictionary<long, RoslynCompiler> _cache = new Dictionary<long, RoslynCompiler>();
            Dictionary<long, IEnumerable<string>> _keywordCache = new Dictionary<long, IEnumerable<string>>();
            Dictionary<DocumentId, long> _documentExtensions = new Dictionary<DocumentId, long>();
            Dictionary<string, CompilerFunc> _extensions = new Dictionary<string, CompilerFunc>();

            public RoslynCompiler GetCompiler(DocumentId documentId, ICollection<UsingDirectiveSyntax> extensions, out IEnumerable<string> keywords)
            {
                keywords = null;

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
                {
                    keywords = _keywordCache[hashCode];
                    return result;
                }

                result = (RoslynCompiler)XSLanguage.CreateCompiler();

                var keywordList = new List<string>();
                var props = new Scope(null);
                props.set("keywords", keywordList);

                foreach (var extension in extensions.ToArray())
                {
                    if (isExtension(extension))
                    {
                        var extensionName = extension
                            .Name
                            .ToString()
                            .Substring("xs.".Length);

                        var compilerFunc = null as CompilerFunc;
                        if (_extensions.TryGetValue(extensionName, out compilerFunc))
                        {
                            try
                            {
                                compilerFunc(result, props);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.Message);
                            }
                            continue;
                        }
                    }

                    extensions.Remove(extension);
                }

                keywords = keywordList;
                _cache[hashCode] = result;
                _keywordCache[hashCode] = keywords;
                return result;
            }

            public IEnumerable<string> Keywords(DocumentId documentId)
            {
                long iid;
                if (!_documentExtensions.TryGetValue(documentId, out iid))
                    return null;

                IEnumerable<string> result = XSLanguage.Keywords;
                IEnumerable<string> cache;
                if (_keywordCache.TryGetValue(iid, out cache))
                    result = result.Union(cache);

                return result;
            }

            private bool isExtension(UsingDirectiveSyntax @using) => @using.Name.ToString().StartsWith("xs.");

            public void ApplyCompilation()
            {
                if (_compilation != null)
                {
                    var solution = _workspace.CurrentSolution;
                    var project = solution.GetProject(_project);
                    var scope = new Scope(null); //td: !!! keep it
                    var compilation = new VSCompilation(project, scope);

                    compilation.PerformAnalysis(_compilation);

                    _workspace.TryApplyChanges(compilation.Project.Solution);
                }
            }
        }

        Dictionary<ProjectId, ProjectCache> _projects = new Dictionary<ProjectId, ProjectCache>();
        public RoslynDocument CreateExcessDocument(string text, DocumentId document)
        {
            ProjectCache cache;
            if (!_projects.TryGetValue(document.ProjectId, out cache))
            {
                ensureNuget();
                ensureDTE();

                cache = new ProjectCache(_workspace, document.ProjectId, _nuget, _nugetEvents);
                _projects[document.ProjectId] = cache;
            }

            //td: we need the using list in order to deduct the extensions
            //however, we don't need to parse the whole document.
            //We must optimize this (maybe a custom using parser?)
            var compilationUnit = CSharp.ParseCompilationUnit(text);
            var extensions = new List<UsingDirectiveSyntax>(compilationUnit.Usings);
            var keywords = null as IEnumerable<string>;
            var compiler = cache.GetCompiler(document, extensions, out keywords);

            //build a new document
            var result = new RoslynDocument(compiler.Scope, text);
            result.Mapper = new MappingService();
            compiler.apply(result);

            var scanner = null as Scanner;
            if (keywords != null && keywords.Any()  && _scannerCache.TryGetValue(document, out scanner))
                scanner.Keywords = XSLanguage.Keywords.Union(keywords);

            return result;
        }

        IVsPackageInstallerServices _nuget;
        IVsPackageInstallerEvents _nugetEvents;
        private void ensureNuget()
        {
            if (_nuget == null)
            {
                var componentModel = (IComponentModel)GetService(typeof(SComponentModel));
                _nuget = componentModel.GetService<IVsPackageInstallerServices>();
                _nugetEvents = componentModel.GetService<IVsPackageInstallerEvents>();
            }

            if (_nuget == null || _nugetEvents == null)
                throw new InvalidOperationException("nuget");
        }

        DTE2 _dte;
        private EnvDTE.Events _dteEvents;
        private BuildEvents _buildEvents;
        private void ensureDTE()
        {
            if (_dte == null)
            {
                _dte = (DTE2)GetService(typeof(DTE));
                _dteEvents = _dte.Events;
                _buildEvents = _dteEvents.BuildEvents;

                _buildEvents.OnBuildDone += BuildDone;
            }
        }

        private void BuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            if (Scope == vsBuildScope.vsBuildScopeSolution)
            {
                foreach (var cache in _projects)
                {
                    cache.Value.ApplyCompilation();
                }
            }
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
        public override IScanner GetScanner(IVsTextLines buffer)
        {
            var documentId = GetDocumentId(buffer);
            if (documentId == null)
                throw new ArgumentException("buffer");

            var result = null as Scanner;
            if (_scannerCache.TryGetValue(documentId, out result))
                return result;

            var scanner = new Scanner(XSLanguage.Keywords);
            _scannerCache[documentId] = scanner;
            return scanner;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new ExcessAuthoringScope();
        }

        public override Source CreateSource(IVsTextLines buffer)
        {
            var scanner = GetScanner(buffer) as Scanner;
            Debug.Assert(scanner != null);

            var documentId = GetDocumentId(buffer);
            scanner.Keywords = GetKeywords(documentId);
            return new ExcessSource(this, buffer, new Colorizer(this, buffer, scanner), _workspace, documentId);
        }

        private IEnumerable<string> GetKeywords(DocumentId documentId)
        {
            var project = null as ProjectCache;
            if (_projects.TryGetValue(documentId.ProjectId, out project))
                return project.Keywords(documentId);

            return XSLanguage.Keywords;
        }

        private DocumentId GetDocumentId(IVsTextLines buffer)
        {
            var filePath = FilePathUtilities.GetFilePath(buffer) + ".cs";
            var documents = _workspace
                .CurrentSolution
                .GetDocumentIdsWithFilePath(filePath);

            var result = documents.SingleOrDefault();


            return result;
        }
    }
}
