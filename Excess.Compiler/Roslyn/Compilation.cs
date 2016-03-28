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
using System.Linq.Expressions;

namespace Excess.Compiler.Roslyn
{
    public interface ICompilationTool //td: get rid of this
    {
        string displayName { get; }
        bool doNotCache { get; }
        bool compile(string file, string contents, Scope scope, Dictionary<string, string> result);
    }

    public class ToolEventArgs
    {
        public ICompilationTool Tool { get; set; }
        public string Document { get; set; }
        public Dictionary<string, string> Result { get; set; }
    }

    public delegate void ToolEventHandler(object sender, ToolEventArgs e);

    public class Compilation
    {
        CompilationAnalysis _analysis;
        IDictionary<string, Action<RoslynCompiler>> _extensions;
        bool _executable;
        public Compilation(
            IPersistentStorage storage = null, 
            CompilationAnalysis analysis = null,
            IDictionary<string, Action<RoslynCompiler>> extensions = null,
            bool executable = false)
        {
            _environment = createEnvironment(storage);
            _analysis = analysis;
            _extensions = extensions;
            _executable = executable;

            //setup
            _scope.set<ICompilerEnvironment>(_environment);
            _scope.set<ICompilerService<SyntaxToken, SyntaxNode, SemanticModel>>(new CompilerService());
        }

        public string OutputFile { get; set; }
        public CompilationAnalysis Analysis
        {
            get
            {
                return _analysis;
            }
        }

        public void setPath(dynamic path)
        {
            _environment.setPath(path);
        }

        public event ToolEventHandler ToolStarted;
        public event ToolEventHandler ToolFinished;

        public ICompilerEnvironment Environment { get { return _environment; } }

        Dictionary<string, ICompilationTool> _tools = new Dictionary<string, ICompilationTool>();
        public void registerTool(string ext, ICompilationTool tool)
        {
            if (_tools.ContainsKey(ext))
                throw new InvalidOperationException("Duplicate tool");

            _tools[ext] = tool;
        }

        private class CompilationDocument
        {
            public string Id { get; set; }
            public CompilerStage Stage { get; set; }
            public IDocument<SyntaxToken, SyntaxNode, SemanticModel> Document { get; set; }
            public RoslynCompiler Compiler { get; set; }
            public ICompilationTool Tool { get; set; }
            public int Hash { get; set; }
            public string Contents { get; set; }
        }

        List<CompilationDocument> _documents = new List<CompilationDocument>();

        Scope _scope = new Scope(null);
        public Scope Scope { get { return _scope; } }

        public bool hasDocument(string id)
        {
            return _documents
                .Where(doc => doc.Id == id)
                .Any();
        }

        public void addDocument(string fileName)
        {
            if (_documents
                .Where(doc => doc.Id == fileName)
                .Any())
                throw new InvalidOperationException($"duplicate file: {fileName}");

            var compiler = null as RoslynCompiler;
            var document = loadDocument(fileName, out compiler);

            Debug.Assert(compiler != null);
            addDocument(fileName, document, compiler);
        }

        private IDocument<SyntaxToken, SyntaxNode, SemanticModel> loadDocument(string fileName, out RoslynCompiler compiler)
        {
            if (_extensions == null || !_extensions.Any())
                throw new InvalidOperationException("no extensions registered, plain c#?");

            var source = File.ReadAllText(fileName);
            var document = new RoslynDocument(new Scope(_scope), source, fileName);

            var compilerResult = new RoslynCompiler(_scope);
            var tree = CSharpSyntaxTree.ParseText(source);
            var usings = (tree.GetRoot() as CompilationUnitSyntax)
                ?.Usings
                .Where(@using =>
                {
                    var usingId = @using.Name.ToString();
                    if (!usingId.StartsWith("xs."))
                        return false;

                    usingId = usingId.Substring("xs.".Length);

                    var action = null as Action<RoslynCompiler>;
                    if (_extensions.TryGetValue(usingId, out action))
                    {
                        action(compilerResult);
                        return true;
                    }

                    return false;
                })
                .ToArray();

            compiler = compilerResult;
            return document;
        }

        public void addDocument(string id, IDocument<SyntaxToken, SyntaxNode, SemanticModel> document)
        {
            if (_documents
                .Where(doc => doc.Id == id)
                .Any())
                throw new InvalidOperationException($"duplicate file: {id}");

            var newDoc = new CompilationDocument
            {
                Id = id,
                Stage = CompilerStage.Started,
                Document = document,
            };

            _documents.Add(newDoc);
        }

        public void addDocument(string id, IDocument<SyntaxToken, SyntaxNode, SemanticModel> document, RoslynCompiler compiler)
        {
            if (_documents
                .Where(doc => doc.Id == id)
                .Any())
                throw new InvalidOperationException($"duplicate file: {id}");

            var newDoc = new CompilationDocument
            {
                Id = id,
                Stage = CompilerStage.Started,
                Document = document,
                Compiler = compiler,
            };

            //build the document
            var documentInjector = newDoc.Compiler as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            documentInjector.apply(newDoc.Document);

            _documents.Add(newDoc);
        }

        public void addDocument(string id, string contents, ICompilerInjector<SyntaxToken, SyntaxNode, SemanticModel> injector)
        {
            if (_documents
                .Where(doc => doc.Id == id)
                .Any())
                throw new InvalidOperationException();

            var ext = Path.GetExtension(id);
            var compiler = null as RoslynCompiler;
            var tool = null as ICompilationTool;
            var hash = 0;
            if (string.IsNullOrEmpty(ext))
            {
                compiler = new RoslynCompiler(_environment, 
                    scope : _scope,
                    compilation: _analysis);

                injector.apply(compiler);
            }
            else if (ext == ".cs")
            {
                addCSharpFile(id, contents);
            }
            else
            {
                if (_tools.TryGetValue(ext, out tool))
                {
                    var storage = _environment.storage();
                    hash = storage == null ? hash : storage.cachedId(id);
                }
            }

            var newDoc = new CompilationDocument
            {
                Id = id,
                Stage = CompilerStage.Started,
                Compiler = compiler,
                Tool = tool,
                Hash = hash
            };

            if (compiler != null)
            {
                newDoc.Stage = CompilerStage.Started;
                newDoc.Document = new RoslynDocument(compiler.Scope, contents, id);

                var documentInjector = newDoc.Compiler as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
                documentInjector.apply(newDoc.Document);
            }
            else
                newDoc.Contents = contents;

            _documents.Add(newDoc);
        }

        public void updateDocument(string id, string contents)
        {
            var doc = _documents
                .Where(document => document.Id == id)
                .FirstOrDefault();

            if (doc == null)
                throw new InvalidOperationException();

            if (doc.Document != null)
            {
                if (_compilation != null && doc.Document.SyntaxRoot != null)
                    _compilation = _compilation.RemoveSyntaxTrees(doc.Document.SyntaxRoot.SyntaxTree);

                doc.Document = new RoslynDocument(doc.Compiler.Scope, contents, id);

                var documentInjector = doc.Compiler as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
                documentInjector.apply(doc.Document);

                doc.Stage = CompilerStage.Started;
            }
            else
                doc.Contents = contents;
        }

        Dictionary<string, SyntaxTree> _csharpFiles = new Dictionary<string, SyntaxTree>();

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
            return doc != null? doc.Document as RoslynDocument : null;
        }

        public string getFileByExtension(string ext)
        {
            var file = _documents
                .Where(document => Path.GetExtension(document.Id) == ext)
                .FirstOrDefault();

            if (file == null)
                return null;

            return file.Id;
        }

        public SyntaxTree getCSharpFile(string fileName)
        {
            SyntaxTree result;
            if (_csharpFiles.TryGetValue(fileName, out result))
                return result;

            return null;
        }

        public void addCSharpFile(string file, SyntaxTree tree = null)
        {
            if (tree == null)
                tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file));

            bool existing = _csharpFiles.ContainsKey(file);
            if (_compilation != null)
            {
                if (existing)
                    _compilation = _compilation.ReplaceSyntaxTree(_csharpFiles[file], tree);
                else
                    _compilation = _compilation.AddSyntaxTrees(tree);
            }

            _csharpFiles[file] = tree;
        }

        public void addCSharpFile(string file, string contents)
        {
            var tree = CSharp.ParseSyntaxTree(contents);
            addCSharpFile(file, tree);
        }

        public string documentText(string id)
        {
            var file = _documents
                .Where(document => document.Id == id)
                .FirstOrDefault();

            if (file != null)
            {
                if (file.Document != null)
                    return file.Document.Text;

                return file.Contents;
            }

            return null;
        }

        public bool compile()
        {
            bool changed;
            IEnumerable<Diagnostic> errors;
            return doCompile(out changed, out errors);
        }

        Dictionary<string, SyntaxTree> _trees = new Dictionary<string, SyntaxTree>();
        private bool doCompile(out bool changed, out IEnumerable<Diagnostic> errors)
        {
            changed = false;
            foreach (var doc in _documents)
            {
                var tool = doc.Tool;
                if (tool == null)
                    continue;

                int hash = 0;
                if (!tool.doNotCache)
                    hash = doc.Contents.GetHashCode();

                if (hash == 0 || hash != doc.Hash)
                {
                    var result = new Dictionary<string, string>();
                    if (ToolStarted != null)
                        ToolStarted(this, new ToolEventArgs { Tool = tool, Document = doc.Id });

                    bool failed = false;
                    try
                    {
                        tool.compile(doc.Id, doc.Contents, _scope, result);
                    }
                    catch (Exception e)
                    {
                        failed = true;
                        result["error"] = e.Message;
                    }

                    if (ToolFinished != null)
                        ToolFinished(this, new ToolEventArgs { Tool = tool, Document = doc.Id, Result = result });

                    if (!failed)
                    {
                        doc.Hash = hash;
                        toolResults(result, doc.Id, hash);
                    }
                }
            }

            foreach (var doc in _documents)
            {
                if (doc.Compiler == null)
                    continue;

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
                {
                    errors = document
                        .SyntaxRoot
                        .GetDiagnostics()
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

                    return false;
                }
            }

            errors = null;
            return true;
        }

        private void toolResults(Dictionary<string, string> result, string fileName, int hash) 
        {
            var storage = _environment.storage();
            if (storage != null)
                storage.cachedId(fileName, hash);

            foreach (var file in result)
            {
                if (hasDocument(file.Key))
                    updateDocument(file.Key, file.Value);
                else if (_csharpFiles.ContainsKey(file.Key))
                    addCSharpFile(file.Key, file.Value);
                else
                {
                    if (storage != null)
                        storage.addFile(file.Key, file.Value, true);

                    addCSharpFile(file.Key, file.Value);
                }
            }
        }

        CSharpCompilation _compilation;

        public MemoryStream build(out IEnumerable<Diagnostic> errors)
        {
            errors = null;

            bool needsProcessing;
            if (!doCompile(out needsProcessing, out errors))
                return null;

            if (_compilation == null)
                _compilation = createCompilation();

            while (needsProcessing)
            {
                needsProcessing = false;
                foreach (var doc in _documents)
                {
                    var document = doc.Document;
                    if (document == null)
                        continue;

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
                        _compilation = _compilation.ReplaceSyntaxTree(oldRoot.SyntaxTree, newTree);
                        _trees[doc.Id] = newTree;
                    }
                }
            }

            errors = _compilation
                .GetDiagnostics()
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

            if (errors.Any())
                return null;

            if (_analysis != null)
                performAnalysis(_analysis);

            var stream = new MemoryStream();
            var result = _compilation.Emit(stream);
            if (!result.Success)
                return null;

            return stream;
        }

        public Assembly build()
        {
            IEnumerable<Diagnostic> errors;
            using (var stream = build(out errors))
            {
                if (stream != null)
                    return Assembly.Load(stream.GetBuffer()); 
            }

            return null;
        }

        RoslynEnvironment _environment = null;
        private CSharpCompilation createCompilation()
        {
            var assemblyName = OutputFile;
            if (assemblyName == null)
                assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            return CSharpCompilation.Create(assemblyName,
                syntaxTrees: _trees.Values
                    .Union(_csharpFiles.Values),
                references: _environment.GetReferences(),
                options: new CSharpCompilationOptions(_executable
                    ? OutputKind.ConsoleApplication
                    : OutputKind.DynamicallyLinkedLibrary));
        }

        protected virtual RoslynEnvironment createEnvironment(IPersistentStorage storage)
        {
            var result = new RoslynEnvironment(_scope, storage);
            result.dependency<object>(new[] { "System", "System.Collections" });
            result.dependency<Queue<object>>(new[] { "System.Collections.Generic" });
            result.dependency<Expression>(new[] { "System.Linq" });

            return result;
        }

        private void performAnalysis(CompilationAnalysis analysis)
        {
            foreach (var document in _documents)
            {
                var root = document.Document.SyntaxRoot;
                if (root != null)
                {
                    var model = _compilation.GetSemanticModel(root.SyntaxTree); //td: really neccesary?
                    var visitor = new CompilationAnalysisVisitor(analysis, model, _scope);
                    visitor.Visit(root);
                }
            }
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
                var document = doc.Document as RoslynDocument;
                if (document == null)
                    continue;

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
