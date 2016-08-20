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

namespace Excess.VisualStudio.VSPackage
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using RoslynCompilationAnalysis = CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>;
    using CompilerFunc = Action<RoslynCompiler, Scope>;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.Text;

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
        private void ensureDTE()
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

            var scope = new Scope(null);
            scope.set(loadExtensions());

            //get all the documents needing compiling
            Dictionary<DocumentId, RoslynDocument> documents = compile(
                filter: doc => buildAction == vsBuildAction.vsBuildActionRebuildAll
                    ? false
                    : isUptoDate(doc), 
                scope : scope);

            //now we must join all files for semantic stuff
            while (documents.Any())
            {
                var temp = new Dictionary<DocumentId, RoslynDocument>(documents);
                documents.Clear();

                foreach (var request in temp)
                {
                    saveCodeBehind(request.Value, request.Key, false);

                    if (!request.Value.HasSemanticalChanges())
                        continue;

                    documents[request.Key] = request.Value;
                }

                if (documents.Any())
                    link(documents, scope);
            }
        }

        private void link(Dictionary<DocumentId, RoslynDocument> documents, Scope scope)
        {
            var requestCount = documents.Count;
            var wait = new ManualResetEvent(false);
            foreach (var request in documents)
            {
                var doc = _workspace.CurrentSolution.GetDocument(request.Key);
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

        private Dictionary<DocumentId, RoslynDocument> compile(Func<Microsoft.CodeAnalysis.Document, bool> filter, Scope scope)
        {
            var documents = new Dictionary<DocumentId, RoslynDocument>();
            var requestCount = 0;
            var wait = new ManualResetEvent(false);
            foreach (var project in _workspace.CurrentSolution.Projects)
            {
                var xsFiles = project
                    .Documents
                    .Where(document => Path.GetExtension(document.FilePath).ToLower() == ".xs");

                foreach (var file in xsFiles)
                {
                    if (!filter(file))
                        continue;

                    var id = file.Id;
                    documents[id] = null;

                    var source = file.GetTextAsync().Result;
                    VSCompiler.Parse(source.ToString(), scope)
                        .ContinueWith(t =>
                        {
                            if (!t.IsFaulted)
                                documents[id] = t.Result;

                            requestCount--;
                            if (requestCount == 0)
                                wait.Set();
                        });
                }
            }

            //wait for syntax changes on all documents
            wait.WaitOne();
            return documents;
        }

        private bool isUptoDate(Microsoft.CodeAnalysis.Document file)
        {
            return false; //td:
        }

        private Dictionary<string, CompilerFunc> loadExtensions()
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
                        throw new NotImplementedException();
                        //if (compilationFunc != null)
                        //{
                        //    if (_compilation == null)
                        //        _compilation = new CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>();

                        //    compilationFunc(_compilation);
                        //}

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
            throw new NotImplementedException();
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            throw new NotImplementedException();
        }
    }
}
