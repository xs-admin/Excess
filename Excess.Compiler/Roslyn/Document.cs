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
            _scope.InitDocumentScope();
        }

        string _documentID;
        public RoslynDocument(Scope scope, string text, string id = null) : this(scope)
        {
            Text = text;
            _documentID = id;
        }

        public RoslynDocument(Scope scope, SyntaxNode root, string id = null) : this(scope)
        {
            setRoot(root);
            _documentID = id;
        }

        public string LexicalText { get; internal set; }

        public FileLinePositionSpan OriginalPosition(Location location)
        {
            var tree = location.SourceTree;
            if (tree == null)
                return default(FileLinePositionSpan);

            var errorNode = tree.GetRoot().FindNode(location.SourceSpan);
            if (errorNode == null)
                return default(FileLinePositionSpan);

            string nodeID = RoslynCompiler.NodeMark(errorNode);
            if (nodeID == null)
                return default(FileLinePositionSpan);

            var originalNode = RoslynCompiler.FindNode(_original, nodeID);
            if (originalNode == null)
                return default(FileLinePositionSpan);

            location = originalNode.SyntaxTree.GetLocation(originalNode.Span);
            return location.GetMappedLineSpan();
        }

        List<Diagnostic> _errors = new List<Diagnostic>();
        public void AddError(string id, string message, SyntaxNode node)
        {
            var location = Location.Create(_root.SyntaxTree, node.Span); 
            var descriptor = new DiagnosticDescriptor(id, message, message, "Excess", DiagnosticSeverity.Error, true);

            var error = Diagnostic.Create(descriptor, location);
            _errors.Add(error);
        }

        public void AddError(string id, string message, int offset, int length)
        {
            var location = Location.Create(_root.SyntaxTree, new TextSpan(offset, length));
            var descriptor = new DiagnosticDescriptor(id, message, message, "Excess", DiagnosticSeverity.Error, true);

            var error = Diagnostic.Create(descriptor, location);
            _errors.Add(error);
        }

        public IEnumerable<Diagnostic> GetErrors()
        {
            return _errors;
        }

        protected override void notifyOriginal(string newText)
        {
            LexicalText = newText;
        }

        protected override SyntaxNode updateRoot(SyntaxNode root)
        {
            var newTree = root.SyntaxTree.WithFilePath(_documentID);
            return newTree.GetRoot();
        }

        public override bool hasErrors()
        {
            if (_errors.Any())
                return true;

            return _root != null 
                && _root
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
                    if (Mapper != null)
                        oldNode = Mapper.SemanticalMap(oldNode);

                    return handler(oldNode, newNode, Model, _scope);
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

        protected override void setRoot(SyntaxNode node)
        {
            _root = node;
        }


        protected override void applySyntactical()
        {
            base.applySyntactical();

            var additionalTypes = _scope.GetAdditionalTypes();
            if (additionalTypes != null && additionalTypes.Any())
            {
                var namespaces = _root
                    .DescendantNodes()
                    .OfType<NamespaceDeclarationSyntax>();

                if (!namespaces.Any())
                {
                    _root = (_root as CompilationUnitSyntax)
                        .AddMembers(additionalTypes.ToArray());
                }
                else
                {
                    _root = _root
                        .ReplaceNodes(namespaces,
                        (on, nn) => nn.AddMembers(additionalTypes.ToArray()));
                }   
            }

            //remove any xs usings
            _root = _root.RemoveNodes(_root.
                DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .Where(@using => @using.Name.ToString().StartsWith("xs.")),
                SyntaxRemoveOptions.KeepEndOfLine);
        }
    }
}
