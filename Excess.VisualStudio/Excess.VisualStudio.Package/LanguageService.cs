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

        Dictionary<string, CompilerFunc> _extensions;
        private void BeforeBuild(vsBuildScope Scope, vsBuildAction Action)
        {
            _extensions = LoadExtensions();

            var xsFiles = _workspace
                .CurrentSolution
                .Projects
                .SelectMany(project => project
                    .Documents
                    .Where(document => Path.GetExtension(document.FilePath).ToLower() == ".xs"));

            switch (Scope)
            {
                case vsBuildScope.vsBuildScopeSolution:
                case vsBuildScope.vsBuildScopeProject:
                    break;
                default:
                    throw new NotImplementedException(); //td: 
            }

            if (Action == vsBuildAction.vsBuildActionClean)
            {
                throw new NotImplementedException(); //td:
            }

            var requests = new Dictionary<DocumentId, RoslynDocument>();
            var requestCount = 0;
            var wait = new ManualResetEvent(false);
            foreach (var file in xsFiles)
            {
                if (Action != vsBuildAction.vsBuildActionRebuildAll && isUptoDate(file))
                    continue;

                var id = file.Id;
                requests[id] = null;

                Task.Run(() =>
                {
                    try
                    {
                        var xsDoc = parseFile(id, file);
                        lock (requests)
                        {
                            requests[id] = xsDoc;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        requestCount--;
                        if (requestCount == 0)
                            wait.Set();
                    }
                });
            }

            wait.WaitOne();

            //update the .cs files
            foreach (var request in requests)
            {
                if (request.Value == null)
                    continue; //td: what to do with the cs file?

                saveCodeBehind(request.Value, request.Key, false);
            }

            //now we must join all files for semantic stuff
            while (requests.Any())
            {
                var temp = new Dictionary<DocumentId, RoslynDocument>(requests);
                requests.Clear();

                foreach (var request in temp)
                {
                    var xs = request.Value;
                    if (!xs.HasSemanticalChanges())
                        continue;

                    var doc = _workspace.CurrentSolution.GetDocument(request.Key);
                    var semanticRoot = doc.GetSyntaxRootAsync().Result;
                    var model = doc.GetSemanticModelAsync().Result;
                    xs.SyntaxRoot = semanticRoot;
                    xs.Model = model;

                    if (!xs.applyChanges(CompilerStage.Semantical))
                        requests[request.Key] = request.Value;

                    saveCodeBehind(request.Value, request.Key, false);
                }
            }
        }

        private Dictionary<string, CompilerFunc> LoadExtensions()
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

        private RoslynDocument parseFile(DocumentId id, Microsoft.CodeAnalysis.Document file)
        {
            var text = file.GetTextAsync().Result;
            return CreateExcessDocument(text.ToString(), id);
        }

        public RoslynDocument CreateExcessDocument(string text, DocumentId document)
        {
            //td: we need the using list in order to deduct the extensions
            //however, we don't need to parse the whole document.
            //We must optimize this (maybe a custom using parser?)
            var compilationUnit = CSharp.ParseCompilationUnit(text);
            var extensions = new List<UsingDirectiveSyntax>(compilationUnit.Usings);
            var keywords = null as IEnumerable<string>;
            var compiler = GetCompiler(document, extensions, out keywords);

            //build a new document
            var result = new RoslynDocument(compiler.Scope, text);
            result.Mapper = new MappingService();
            compiler.apply(result);
            return result;
        }

        private bool isExtension(UsingDirectiveSyntax @using) => @using.Name.ToString().StartsWith("xs.");

        public RoslynCompiler GetCompiler(DocumentId documentId, ICollection<UsingDirectiveSyntax> extensions, out IEnumerable<string> keywords)
        {
            keywords = null;
            var result = (RoslynCompiler)XSLanguage.CreateCompiler();

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
            return result;
        }

        private bool isUptoDate(Microsoft.CodeAnalysis.Document file)
        {
            return false; //td:
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
