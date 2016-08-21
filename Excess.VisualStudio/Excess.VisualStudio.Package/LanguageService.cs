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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

namespace Excess.VisualStudio.VSPackage
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
        public void ensureDTE()
        {
            if (_dte == null)
            {
                _dte = (DTE2)GetService(typeof(DTE));
                _dteEvents = _dte.Events;
                _buildEvents = _dteEvents.BuildEvents;

                _buildEvents.OnBuildBegin += BeforeBuild;
                _buildEvents.OnBuildDone += AfterBuild;
            }
        }

        private void BeforeBuild(vsBuildScope buildScope, vsBuildAction buildAction)
        {
            if (buildAction == vsBuildAction.vsBuildActionClean)
                throw new NotImplementedException(); //td:

            var compilation = new RoslynCompilationAnalysis();
            var scope = new Scope(null);
            scope.set(loadExtensions(compilation));

            //get all the documents needing compiling
            var documents = compile(
                filter: doc => buildAction == vsBuildAction.vsBuildActionRebuildAll
                    ? true
                    : isDirty(doc), 
                scope : scope);

            //find the cs code behind files
            var documentIds = new Dictionary<string, DocumentId>();
            foreach (var path in documentIds.Keys)
            {
                var csFile = path + ".cs";
                var docId = _workspace
                    .CurrentSolution
                    .GetDocumentIdsWithFilePath(path)
                    .FirstOrDefault();

                if (docId != null)
                {
                    documentIds[path] = docId;
                }
                else
                {
                    //td: add to project
                }
            }

            //now we must join all files for semantic stuff
            while (documents.Any())
            {
                var temp = new Dictionary<string, RoslynDocument>(documents);
                documents.Clear();

                foreach (var request in temp)
                {
                    var docId = documentIds[request.Key];
                    saveCodeBehind(request.Value, docId, false);

                    if (!request.Value.HasSemanticalChanges())
                        continue;

                    documents[request.Key] = request.Value;
                }

                if (documents.Any())
                    link(documents, documentIds, scope);
            }
        }

        private void link(Dictionary<string, RoslynDocument> documents, Dictionary<string, DocumentId> ids, Scope scope)
        {
            var requestCount = documents.Count;
            var wait = new ManualResetEvent(false);
            foreach (var request in documents)
            {
                var docId = ids[request.Key];
                var doc = _workspace.CurrentSolution.GetDocument(docId);
                var xs = request.Value;
                doc.GetSyntaxRootAsync()
                    .ContinueWith(t => 
                    {
                        var semanticRoot = t.Result;
                        doc.GetSemanticModelAsync()
                            .ContinueWith((tt) => 
                            {
                                var model = tt.Result;
                                xs.SyntaxRoot = semanticRoot;
                                xs.Model = model;
                                xs.applyChanges(CompilerStage.Semantical);

                                requestCount--;
                                if (requestCount <= 0)
                                    wait.Set();
                            });
                    });
            }

            wait.WaitOne();
        }

        private Dictionary<string, RoslynDocument> compile(Func<ProjectItem, bool> filter, Scope scope)
        {
            var documents = new Dictionary<string, RoslynDocument>();
            var requestCount = 0;
            var wait = new ManualResetEvent(false);
            var projects = _dte.Solution.Projects.Cast<EnvDTE.Project>();
            var xsFiles = new List<ProjectItem>();

            foreach (var project in projects)
            {
                foreach (var item in project.ProjectItems.Cast<ProjectItem>())
                {
                    collectXsFiles(item, xsFiles);
                }
            }

            foreach (var file in xsFiles)
            {
                if (!filter(file))
                    continue;

                var fileName = file.FileNames[0];
                requestCount++;
                var text = File.ReadAllText(fileName);
                var doc = VSCompiler.Parse(text, scope);
                documents[fileName] = doc;
            }

            return documents;
        }

        private void collectXsFiles(ProjectItem item, List<ProjectItem> result)
        {
            if (isXsFile(item))
                result.Add(item);
            else if (item.ProjectItems != null)
            {
                foreach (var nested in item.ProjectItems.Cast<ProjectItem>())
                    collectXsFiles(nested, result);
            }
        }

        private bool isXsFile(ProjectItem item)
        {
            return item.Name.EndsWith(".xs");
        }

        private bool isDirty(ProjectItem file)
        {
            return true; //td:
        }

        private Dictionary<string, CompilerFunc> loadExtensions(RoslynCompilationAnalysis compilation)
        {
            var result = new Dictionary<string, CompilerFunc>();

            ensureDTE();
            ensureNuget();

            var projects = _dte.ActiveSolutionProjects;
            var packages = _nuget.GetInstalledPackages();
            foreach (var package in packages)
            {
                var toolPath = Path.Combine(package.InstallPath, "tools");
                if (!Directory.Exists(toolPath))
                    continue; //td: ask to restore packages

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
                            if (compilation == null)
                                compilationFunc(compilation);
                        }

                        result[name] = extension;
                    }
                }
            }

            return result;
        }

        private bool saveCodeBehind(RoslynDocument doc, DocumentId id, bool mapLines)
        {
            doc.SyntaxRoot = doc.SyntaxRoot
                .NormalizeWhitespace(elasticTrivia: true); //td: optimize

            var solution = _workspace.CurrentSolution;
            if (mapLines)
            {
                var mapper = doc.Mapper;
                var vsDocument = solution.GetDocument(id);
                var filePath = vsDocument?.FilePath;
                if (filePath != null)
                    filePath = filePath.Remove(filePath.Length - ".cs".Length);

                solution = solution.WithDocumentText(id, SourceText.From(doc.Mapper
                    .RenderMapping(doc.SyntaxRoot, filePath)));
            }
            else
                solution = solution.WithDocumentSyntaxRoot(id, doc.SyntaxRoot);

            return _workspace.TryApplyChanges(solution);
        }

        private void AfterBuild(vsBuildScope Scope, vsBuildAction Action)
        {
            throw new NotImplementedException(); //td: generate js files, for instance
            //if (Scope == vsBuildScope.vsBuildScopeSolution)
            //{
            //    foreach (var cache in _projects)
            //    {
            //        cache.Value.ApplyCompilation();
            //    }
            //}
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

        private DocumentId GetDocumentId(IVsTextLines buffer)
        {
            var filePath = FilePathUtilities.GetFilePath(buffer) + ".cs";
            var documents = _workspace
                .CurrentSolution
                .GetDocumentIdsWithFilePath(filePath);

            return documents.SingleOrDefault();
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return new Scanner(XSLanguage.Keywords); //td:
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return new ExcessAuthoringScope();
        }
    }
}
