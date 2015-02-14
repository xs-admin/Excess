using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.Roslyn
{
    public class RoslynDocument : BaseDocument<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynDocument(Scope scope) : base(scope)
        {
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(this);
        }

        string _documentID;
        public RoslynDocument(Scope scope, string text, string id = null) : base(scope)
        {
            Text = text;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(this);

            _documentID = id;
        }

        public string LexicalText { get; internal set; }

        public CompilerStage Stage { get; internal set; }

        public Diagnostic ExcessError(Diagnostic error, string file)
        {
            var location = error.Location;
            Debug.Assert(location != null);

            var tree = location.SourceTree;
            if (tree == null)
                return error;


            var errorNode = tree.GetRoot().FindNode(location.SourceSpan);
            if (errorNode == null)
                return error;

            string nodeID = RoslynCompiler.NodeMark(errorNode);
            if (nodeID == null)
                return error;

            var originalNode = RoslynCompiler.FindNode(_original, nodeID);
            if (originalNode == null)
                return error;

            location    = originalNode.SyntaxTree.GetLocation(originalNode.Span);
            var message = error.GetMessage();
            var descriptor = new DiagnosticDescriptor(error.Id, message, message, error.Category, DiagnosticSeverity.Error, error.IsEnabledByDefault);

            return Diagnostic.Create(descriptor, location);
        }

        List<Diagnostic> _errors = new List<Diagnostic>();
        public void AddError(string id, string message, SyntaxNode node)
        {
            var location = Location.Create(_root.SyntaxTree, node.Span); //td: translate
            var descriptor = new DiagnosticDescriptor(id, message, message, "Excess", DiagnosticSeverity.Error, true);

            var error = Diagnostic.Create(descriptor, location);
            _errors.Add(error);
        }

        public IEnumerable<Diagnostic> GetErrors()
        {
            return _errors;
        }

        protected override SyntaxNode notifyOriginal(SyntaxNode root, string newText)
        {
            LexicalText = newText;

            if (_documentID == null)
                return root;

            var newTree = root.SyntaxTree.WithFilePath(_documentID);
            return newTree.GetRoot();
        }

        public override bool hasErrors()
        {
            if (_errors.Any())
                return true;

            return _root != null && 
                   _root
                        .GetDiagnostics()
                        .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                        .Any();
        }

        protected override SyntaxNode transform(SyntaxNode node, Dictionary<int, Func<SyntaxNode, Scope, SyntaxNode>> transformers)
        {
            var nodes = new Dictionary<SyntaxNode, Func<SyntaxNode, Scope, SyntaxNode>> ();
            foreach (var transformer in transformers)
            {
                SyntaxNode tNode = node
                    .GetAnnotatedNodes(RoslynCompiler.NodeIdAnnotation + transformer.Key.ToString())
                    .First();

                Debug.Assert(tNode != null); //td: cases

                nodes[tNode] = transformer.Value;
            }

            IEnumerable<SyntaxNode> toReplace = nodes.Keys;
            return node.ReplaceNodes(toReplace, (oldNode, newNode) =>
            {
                Func<SyntaxNode, Scope, SyntaxNode> handler;
                if (nodes.TryGetValue(oldNode, out handler))
                {
                    var result = handler(newNode, _scope);
                    return result;
                }

                return newNode;
            });
        }

        protected override SyntaxNode transform(SyntaxNode node, Dictionary<int, Func<SyntaxNode, SyntaxNode, SemanticModel, Scope, SyntaxNode>> transformers)
        {
            Debug.Assert(Model != null);

            var nodes = new Dictionary<SyntaxNode, Func<SyntaxNode, SyntaxNode, SemanticModel, Scope, SyntaxNode>>();
            foreach (var transformer in transformers)
            {
                SyntaxNode tNode = node
                    .GetAnnotatedNodes(RoslynCompiler.NodeIdAnnotation + transformer.Key.ToString())
                    .First();

                Debug.Assert(tNode != null); //td: cases

                nodes[tNode] = transformer.Value;
            }

            IEnumerable<SyntaxNode> toReplace = nodes.Keys;
            return node.ReplaceNodes(toReplace, (oldNode, newNode) =>
            {
                Func<SyntaxNode, SyntaxNode, SemanticModel, Scope, SyntaxNode> handler;
                if (nodes.TryGetValue(oldNode, out handler))
                {
                    var result = handler(oldNode, newNode, Model, _scope);
                    return result;
                }

                return newNode;
            });
        }

        protected override SyntaxNode addModules(SyntaxNode root, IEnumerable<string> modules)
        {
            var compilationUnit = (CompilationUnitSyntax)root;
            return compilationUnit
                .WithUsings(CSharp.List(
                    compilationUnit.Usings.Union(
                    modules
                        .Select(module => CSharp.UsingDirective(
                            CSharp.ParseName(module))))));
        }

        protected override SyntaxNode syntacticalTransform(SyntaxNode node, Scope scope, IEnumerable<Func<SyntaxNode, Scope, SyntaxNode>> transformers)
        {
            SyntaxRewriter rewriter = new SyntaxRewriter(transformers, scope);
            return rewriter.Visit(node);
        }

        protected override SyntaxNode getRoot()
        {
            return _root;
        }
    }
}
