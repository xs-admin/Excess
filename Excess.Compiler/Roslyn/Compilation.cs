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

namespace Excess.Compiler.Roslyn
{
    public class Compilation
    {

        public string OutputFile { get; set; }

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
                .Select(doc => doc.Id == id)
                .Any())
                throw new InvalidOperationException();

            var newDoc = new CompilationDocument
            {
                Id = id,
                Stage = CompilerStage.Started,
                Document = new RoslynDocument(_scope, contents),
                Compiler = new RoslynCompiler()
            };

            injector.apply(newDoc.Compiler);
            _documents.Add(newDoc);
        }

        public void updateDocument(string id, string contents)
        {
            var doc = _documents
                .Where(document => document.Id == id)
                .FirstOrDefault();

            if (doc == null)
                throw new InvalidOperationException();

            doc.Document = new RoslynDocument(_scope, contents);
            doc.Stage = CompilerStage.Started;
        }

        public bool compile()
        {
            return doCompile(null);
        }

        Dictionary<string, SyntaxTree> _trees = new Dictionary<string, SyntaxTree>();
        private bool doCompile(Dictionary<SyntaxTree, SyntaxTree> changes)
        {
            foreach (var doc in _documents)
            {
                var document = doc.Document;
                if (document.Stage <= CompilerStage.Syntactical)
                {
                    var oldRoot = document.SyntaxRoot;
                    document.applyChanges(CompilerStage.Syntactical);
                    if (document.SyntaxRoot != oldRoot && changes != null)
                        changes[oldRoot.SyntaxTree] = document.SyntaxRoot.SyntaxTree;
                }

                if (document.hasErrors())
                    return false;
            }

            return true;
        }

        CSharpCompilation _compilation;

        public Assembly build()
        {
            Dictionary<SyntaxTree, SyntaxTree> changes = new Dictionary<SyntaxTree, SyntaxTree>();
            if (!doCompile(changes))
                return null;

            if (_compilation == null)
                _compilation = createCompilation();

            bool hasChanges = false;
            foreach (var change in changes)
            {
                _compilation = _compilation.ReplaceSyntaxTree(change.Key, change.Value);
                hasChanges = true;
            }

            while (hasChanges)
            {
                hasChanges = false;
                foreach (var doc in _documents)
                {
                    var document = doc.Document;
                    hasChanges |= !document.applyChanges(CompilerStage.Semantical);

                    if (document.hasErrors())
                        return null;
                }
            }

            using (var stream = new MemoryStream())
            {
                var result = _compilation.Emit(stream);
                if (!result.Success)
                    return null;

                return Assembly.Load(stream.GetBuffer()); 
            }
        }

        private CSharpCompilation createCompilation()
        {
            var assemblyName = OutputFile;
            if (assemblyName == null)
                assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            var references = defaultDependencies();
            foreach (var doc in _documents)
            {
                var compiler = doc.Compiler;
                foreach (Type dependency in compiler.Dependencies)
                {
                    if (!references.Contains(dependency))
                        references.Add(dependency);
                }
            }

            return CSharpCompilation.Create(assemblyName,
                syntaxTrees: _trees.Values,
                references: references.Select(reference => MetadataReference.CreateFromAssembly(reference.Assembly)),
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        }

        private List<Type> defaultDependencies()
        {
            return new List<Type>(new []
            {
                typeof(object),
                typeof(Enumerable)
            });
        }

        public IEnumerable<Diagnostic> errors()
        {
            var diagnostics = _compilation.GetDiagnostics();
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

                List<Diagnostic> fileDiagnostics;
                if (byFile.TryGetValue(doc.Id, out fileDiagnostics))
                {
                    foreach (var error in fileDiagnostics)
                    {
                        var docError = document.Error(error);
                        if (docError != null)
                            yield return docError;
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
