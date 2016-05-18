using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using ExcessDocument = IDocument<SyntaxToken, SyntaxNode, SemanticModel>;
    using ExtensionFunc = Action<RoslynCompiler, Scope>;
    using CompilationAnalysis = CompilationAnalysisBase<SyntaxToken, SyntaxNode, SemanticModel>;

    public class SolutionStorage : IPersistentStorage
    {
        Project _project;
        public SolutionStorage(Project project)
        {
            _project = project;
        }

        public int addFile(string name, string contents, bool hidden)
        {
            throw new NotImplementedException();
        }

        public int cachedId(string name)
        {
            //td: refactor the whole Compilation
            throw new NotImplementedException();
        }

        public void cachedId(string name, int id)
        {
            throw new NotImplementedException();
        }
    }

    public class SolutionCompilation
    {
        MSBuildWorkspace _workspace;
        string _solution;
        Scope _scope = new Scope(null);
        IDictionary<string, ExtensionFunc> _extensions;
        CompilationAnalysis _analysis;
        public SolutionCompilation(string solution, CompilationAnalysis analysis, IDictionary<string, ExtensionFunc> extensions)
        {
            _extensions = extensions;
            _analysis = analysis;
            _solution = solution;
            _workspace = MSBuildWorkspace.Create();
        }

        private class CompilationDocument
        {
            public ExcessDocument Document { get; set; }
            public RoslynCompiler Compiler { get; set; }
            public DocumentId Id { get; set; }
        }

        public IEnumerable<Diagnostic> Transpile()
        {
            var solution = _workspace.OpenSolutionAsync(_solution).Result;
            var currSolution = solution;
            var errors = new List<Diagnostic>();
            foreach (var project in solution.Projects)
            {
                var currProject = project;
                var documents = new List<CompilationDocument>();
                foreach (var document in project.Documents)
                {
                    if (Path.GetExtension(document.FilePath) == ".xs")
                    {
                        var fileName = document.FilePath + ".cs";
                        var codeBehind = project
                            .Documents
                            .Where(doc => doc.FilePath == fileName)
                            .FirstOrDefault();

                        if (codeBehind != null)
                        {
                            codeBehind = currProject.AddDocument(Path.GetFileName(fileName), string.Empty, filePath: fileName);
                            currProject = codeBehind.Project;
                        }

                        var compiler = null as RoslynCompiler;
                        var eDoc = loadDocument(project, document.FilePath, out compiler);

                        documents.Add(new CompilationDocument
                        {
                            Document = eDoc,
                            Compiler = compiler,
                            Id = codeBehind.Id
                        });
                    }

                    currProject = compileProject(project, documents, errors);
                    currSolution = currProject.Solution;
                }
            }

            return errors;
        }

        private Project compileProject(Project project, List<CompilationDocument> documents, List<Diagnostic> errors)
        {
            var hasErrors = false;
            foreach (var doc in documents)
            {
                if (doc.Compiler == null)
                    continue;

                var document = doc.Document;
                if (document.Stage <= CompilerStage.Syntactical)
                {
                    var oldRoot = document.SyntaxRoot;
                    document.applyChanges(CompilerStage.Syntactical);

                    project = project
                        .GetDocument(doc.Id)
                        .WithSyntaxRoot(document.SyntaxRoot)
                        .Project;
                }

                if (document.hasErrors())
                {
                    hasErrors = true;
                    foreach (var error in document
                        .SyntaxRoot
                        .GetDiagnostics()
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
                        errors.Add(error);
                }
            }

            //apply semantic pass
            if (!hasErrors)
            {
                var compilation = project.GetCompilationAsync().Result;
                var needsProcessing = true;
                while (needsProcessing)
                {
                    needsProcessing = false;
                    foreach (var doc in documents)
                    {
                        var document = doc.Document;
                        if (document == null)
                            continue;

                        var tree = document.SyntaxRoot.SyntaxTree;

                        document.Model = compilation.GetSemanticModel(tree);
                        var oldRoot = document.SyntaxRoot;
                        if (!document.applyChanges(CompilerStage.Semantical))
                            needsProcessing = true;

                        var newRoot = document.SyntaxRoot;
                        var newTree = newRoot.SyntaxTree;
                        if (oldRoot != newRoot)
                            compilation = compilation.ReplaceSyntaxTree(oldRoot.SyntaxTree, newTree);
                    }
                }
            }

            return project;
        }

        private ExcessDocument loadDocument(Project project, string fileName, out RoslynCompiler compiler)
        {
            if (_extensions == null || !_extensions.Any())
                throw new InvalidOperationException("no extensions registered, plain c#?");

            var source = File.ReadAllText(fileName);
            var document = new RoslynDocument(new Scope(_scope), source, fileName);

            var enviroment = new RoslynEnvironment(_scope, new SolutionStorage(project));
            var compilerResult = new RoslynCompiler(enviroment, _scope);
            var tree = CSharpSyntaxTree.ParseText(source);
            var usings = (tree.GetRoot() as CompilationUnitSyntax)
                ?.Usings
                .Where(@using =>
                {
                    var usingId = @using.Name.ToString();
                    if (!usingId.StartsWith("xs."))
                        return false;

                    usingId = usingId.Substring("xs.".Length);

                    var action = null as ExtensionFunc;
                    if (_extensions.TryGetValue(usingId, out action))
                    {
                        action(compilerResult, null); //td: props?
                        return true;
                    }

                    return false;
                })
                .ToArray();

            compiler = compilerResult;
            return document;
        }
    }
}
