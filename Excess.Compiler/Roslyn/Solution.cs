using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using ExtensionFunction = Action<RoslynCompiler, Scope>;

    public class ExcessDocument
    {
        public DocumentId Id { get; set; }
        public string Name { get; set; }
        public DocumentId CSharpDocument { get; set; }
        public RoslynDocument Document { get; set; }
        public RoslynCompiler Compiler { get; set; }
        public Dictionary<int, int> Mapping { get; set; }
    }

    public class ExcessSolution
    {
        ExcessSolution _parent;
        Solution _solution;
        IDictionary<string, ExtensionFunction> _extensions;
        Scope _scope;
        public ExcessSolution(Solution solution, IDictionary<string, ExtensionFunction> extensions)
        {
            _solution = solution;
            _extensions = extensions;

            _scope = new Scope(null); //td: scopes
            _scope.set<ICompilerService<SyntaxToken, SyntaxNode, SemanticModel>>(new CompilerService());
            loadExcessDocuments();
        }

        public Solution Solution { get { return _solution; } }

        Dictionary<DocumentId, ExcessDocument> _documents;
        public ExcessSolution ApplyChanges(DocumentId document)
        {
            throw new NotImplementedException();
        }

        public ExcessDocument GetDocument(string name)
        {
            return _documents.First(d => d.Value.Name == name).Value;
        }

        private void loadExcessDocuments()
        {
            Debug.Assert(_documents == null);
            _documents = new Dictionary<DocumentId, ExcessDocument>();
            foreach (var project in _solution.Projects)
            {
                foreach (var doc in project.AdditionalDocuments)
                {
                    if (Path.GetExtension(doc.Name) == ".xs")
                        _documents[doc.Id] = loadDocument(_solution, doc, out _solution);
                }
            }

            _solution = build(_solution);
        }

        private Solution doCompile(Solution solution, out bool changed)
        {
            changed = false;
            var documents = _documents.Values;
            foreach (var doc in documents)
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
                    solution = solution.WithDocumentSyntaxRoot(doc.CSharpDocument, newRoot);
                }
            }

            return solution;
        }

        public Solution build(Solution from)
        {

            bool needsProcessing;
            var result = doCompile(from, out needsProcessing);
            if (result == null)
                return null;

            while (needsProcessing)
            {
                needsProcessing = false;
                var documents = _documents.Values;
                foreach (var doc in documents)
                {
                    var document = doc.Document;
                    if (document == null)
                        continue;

                    var tree = document.SyntaxRoot.SyntaxTree;

                    var solutionDoc = result.GetDocument(doc.CSharpDocument);
                    var model = solutionDoc.GetSemanticModelAsync().Result;

                    document.Model = model;
                    var oldRoot = document.SyntaxRoot;
                    if (!document.applyChanges(CompilerStage.Semantical))
                        needsProcessing = true;

                    var newRoot = document.SyntaxRoot;


                    Debug.Assert(oldRoot != null && newRoot != null);
                    var newTree = newRoot.SyntaxTree;
                    if (oldRoot != newRoot)
                        result = result.WithDocumentSyntaxRoot(doc.CSharpDocument, newRoot);
                }
            }

            return result;
        }

        private ExtensionFunction getExtension(string name)
        {
            var extension = null as ExtensionFunction;
            _extensions.TryGetValue(name, out extension);
            return extension;
        }

        private ExcessDocument loadDocument(Solution solution, TextDocument doc, out Solution result)
        {
            SourceText text;
            if (!doc.TryGetText(out text))
                throw new InvalidOperationException($"Cannot read from {doc.Name}");

            var contents = text.ToString();
            var document = new RoslynDocument(new Scope(_scope), contents); //port: rid of scope

            var compilerResult = new RoslynCompiler(new Scope(_scope)); //port: rid of scope
            var tree = CSharpSyntaxTree.ParseText(contents);
            var usings = (tree.GetRoot() as CompilationUnitSyntax)
                ?.Usings
                .Where(@using =>
                {
                    var usingId = @using.Name.ToString();
                    if (!usingId.StartsWith("xs."))
                        return false;

                    usingId = usingId.Substring("xs.".Length);

                    var extension = getExtension(usingId);
                    if (extension != null)
                    {
                        extension(compilerResult, null); //td: props?
                        return true;
                    }

                    return false;
                }).ToArray();

            if (_extensions.ContainsKey("xs"))
                _extensions["xs"](compilerResult, _scope);

            result = solution; //td: add needed cs files, etc
            var filename = doc.Name + ".cs";
            var fileid = doc
                .Project
                .Documents
                .FirstOrDefault(d => d.Name == filename)
                ?.Id;

            if (fileid == null)
            {
                var newFile = doc.Project.AddDocument(filename, "");
                result = newFile.Project.Solution;
                fileid = newFile.Id;
            }

            var documentInjector = compilerResult as IDocumentInjector<SyntaxToken, SyntaxNode, SemanticModel>;
            documentInjector.apply(document);

            return new ExcessDocument
            {
                Id = doc.Id,
                Name = doc.Name,
                CSharpDocument = fileid,
                Compiler = compilerResult,
                Document = document,
            };
        }
    }
}
