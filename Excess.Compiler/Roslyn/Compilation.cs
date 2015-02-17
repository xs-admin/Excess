using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Reflection;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System.IO;
using System.Diagnostics;

namespace Excess.Compiler.Roslyn
{
    public class Compilation
    {
        public Compilation()
        {
            _environment = createEnvironment();
        }

        public string OutputFile { get; set; }
        public ICompilerEnvironment Environment { get { return _environment; } }

        private class CompilationDocument
        {
            public string Id { get; set; }
            public CompilerStage Stage { get; set; }
            public RoslynDocument Document { get; set; }
            public RoslynCompiler Compiler { get; set; }
        }

        List<CompilationDocument> _documents = new List<CompilationDocument>();

        Scope _scope = new Scope(null);

        public void addDocument(string id, string contents, ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> injector)
        {
            if (_documents
                .Where(doc => doc.Id == id)
                .Any())
                throw new InvalidOperationException();

            var compiler = new RoslynCompiler(_environment, _scope);
            var newDoc = new CompilationDocument
            {
                Id = id,
                Stage = CompilerStage.Started,
                Document = new RoslynDocument(compiler.Scope, contents, id),
                Compiler = compiler
            };

            injector.apply(newDoc.Compiler);

            var documentInjector = newDoc.Compiler as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            documentInjector.apply(newDoc.Document);

            _documents.Add(newDoc);
        }

        public void updateDocument(string id, string contents)
        {
            var doc = _documents
                .Where(document => document.Id == id)
                .FirstOrDefault();

            if (doc == null)
                throw new InvalidOperationException();

            if (_compilation != null && doc.Document.SyntaxRoot != null)
                _compilation = _compilation.RemoveSyntaxTrees(doc.Document.SyntaxRoot.SyntaxTree);

            doc.Document = new RoslynDocument(doc.Compiler.Scope, contents, id);

            var documentInjector = doc.Compiler as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            documentInjector.apply(doc.Document);

            doc.Stage = CompilerStage.Started;
        }

        List<SyntaxTree> _additionalTrees = new List<SyntaxTree>();

        public FileLinePositionSpan OriginalPosition(Location location)
        {
            var tree = location.SourceTree;
            if (tree == null)
                return default(FileLinePositionSpan);

            var document = documentByTree(tree);
            if (document == null)
                return default(FileLinePositionSpan);

            return document.OriginalPosition(location);
        }

        private RoslynDocument documentByTree(SyntaxTree tree)
        {
            string id = null;
            foreach (var cTree in _trees)
            {
                if (cTree.Value == tree)
                {
                    id = cTree.Key;
                    break;
                }
            }

            if (id == null)
                return null;

            var doc = _documents.Find(document => document.Id == id);
            return doc != null? doc.Document : null;
        }

        public void addSyntaxTree(SyntaxTree tree)
        {
            Debug.Assert(_compilation == null);
            _additionalTrees.Add(tree);
        }

        public string documentText(string id)
        {
            var file = _documents
                .Where(document => document.Id == id)
                .FirstOrDefault();

            if (file != null)
                return file.Document.Text;

            return null;
        }

        public bool compile()
        {
            bool changed;
            return doCompile(out changed);
        }

        Dictionary<string, SyntaxTree> _trees = new Dictionary<string, SyntaxTree>();
        private bool doCompile(out bool changed)
        {
            changed = false;
            foreach (var doc in _documents)
            {
                var document = doc.Document;
                if (document.Stage <= CompilerStage.Syntactical)
                {
                    changed = true;

                    var oldRoot = document.SyntaxRoot;
                    document.applyChanges(CompilerStage.Syntactical);
                    var newRoot = document.SyntaxRoot;

                    Debug.Assert(newRoot != null);
                    var newTree = newRoot.SyntaxTree;

                    if (_compilation != null)
                    {
                        if (oldRoot == null)
                            _compilation = _compilation.AddSyntaxTrees(newTree);
                        else
                            _compilation = _compilation.ReplaceSyntaxTree(oldRoot.SyntaxTree, newTree);
                    }

                    _trees[doc.Id] = newTree;
                }

                if (document.hasErrors())
                    return false;
            }

            return true;
        }

        CSharpCompilation _compilation;

        public Assembly build()
        {
            bool needsProcessing;
            if (!doCompile(out needsProcessing))
                return null;

            if (_compilation == null)
                _compilation = createCompilation();

            while (needsProcessing)
            {
                needsProcessing = false;
                foreach (var doc in _documents)
                {
                    var document = doc.Document;
                    var tree = document.SyntaxRoot.SyntaxTree;

                    document.Model = _compilation.GetSemanticModel(tree);
                    Debug.Assert(document.Model != null);

                    var oldRoot = document.SyntaxRoot;
                    if (!document.applyChanges(CompilerStage.Semantical))
                        needsProcessing = true;

                    var newRoot = document.SyntaxRoot;

                    Debug.Assert(oldRoot != null && newRoot != null);
                    var newTree = newRoot.SyntaxTree;
                    if (oldRoot != newRoot)
                    {
                        _compilation   = _compilation.ReplaceSyntaxTree(oldRoot.SyntaxTree, newTree);
                        _trees[doc.Id] = newTree;
                    }
                }
            }

            if (_compilation
                    .GetDiagnostics()
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Any())
                return null;

            using (var stream = new MemoryStream())
            {
                var result = _compilation.Emit(stream);
                if (!result.Success)
                    return null;

                return Assembly.Load(stream.GetBuffer()); 
            }
        }

        RoslynEnvironment _environment = null;
        private CSharpCompilation createCompilation()
        {
            var assemblyName = OutputFile;
            if (assemblyName == null)
                assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            return CSharpCompilation.Create(assemblyName,
                syntaxTrees: _trees.Values
                    .Union(_additionalTrees),
                references: _environment.GetReferences(),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        protected virtual RoslynEnvironment createEnvironment()
        {
            var result = new RoslynEnvironment();
            result.dependency<object>(new[] { "System", "System.Collections", "System.Collections.Generic" });
            result.dependency<IEnumerable<object>>(new[] { "System.Collections", "System.Collections.Generic" });

            return result;
        }

        public IEnumerable<Diagnostic> errors()
        {
            var diagnostics = null as IEnumerable<Diagnostic>;
            if (_compilation == null)
                _compilation = createCompilation();

            diagnostics = _compilation
                .GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error);

            var byFile = new Dictionary<string, List<Diagnostic>>();
            foreach (var diagnostic in diagnostics)
            {
                var tree = diagnostic.Location.SourceTree;
                var file = treeFile(tree);
                    
                if (file != null)
                {
                    List<Diagnostic> fileDiagnostics;
                    if (!byFile.TryGetValue(file, out fileDiagnostics))
                    {
                        fileDiagnostics  = new List<Diagnostic>();
                        byFile[file] = fileDiagnostics;
                    }

                    fileDiagnostics.Add(diagnostic);
                }
                else
                    yield return diagnostic; //td: maybe ignore?
            }

            foreach (var doc in _documents)
            {
                var document = doc.Document;

                var errors = document.GetErrors();

                //native errors
                List<Diagnostic> fileDiagnostics;
                if (byFile.TryGetValue(doc.Id, out fileDiagnostics))
                {
                    if (errors == null)
                        errors = fileDiagnostics;
                    else
                        errors = errors.Union(fileDiagnostics);
                }

                if (errors != null)
                {
                    foreach (var error in errors)
                    {
                        yield return error;
                    }
                }
            }
        }

        private string treeFile(SyntaxTree tree)
        {
            foreach (var file in _trees)
            {
                if (file.Value == tree)
                    return file.Key;
            }

            return null;
        }
    }
}
