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
    class RoslynDocument : BaseDocument<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynDocument(Scope scope) : base(scope)
        {
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(this);
        }

        public RoslynDocument(Scope scope, string text) : base(scope)
        {
            Text = text;
            _scope.set<IDocument<SyntaxToken, SyntaxNode, SemanticModel>>(this);
        }

        public string LexicalText { get; internal set; }
        public SyntaxNode SyntaxRoot { get { return _root; } }

        public CompilerStage Stage { get; internal set; }

        public Diagnostic Error(Diagnostic error)
        {
            return error;
        }

        protected override void notifyResultText(string resultText)
        {
            LexicalText = resultText;
        }

        public override bool hasErrors()
        {
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
    }
}
