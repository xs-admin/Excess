using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Core
{
    public class Linker
    {
        public Linker(ExcessContext ctx, Compilation compilation)
        {
            ctx_         = ctx;
            compilation_ = compilation;
        }

        public SyntaxNode link(SyntaxNode node, out Compilation result)
        {
            var           tree  = node.SyntaxTree;
            SemanticModel model = compilation_.GetSemanticModel(node.SyntaxTree);
            node = node.ReplaceNodes(node.GetAnnotatedNodes(Compiler.LinkerAnnotationId), (oldNode, newNode) =>
            {
                var annotation = oldNode.GetAnnotations(Compiler.LinkerAnnotationId).First();
                if (oldNode.SyntaxTree != model.SyntaxTree)
                {
                    oldNode = model.SyntaxTree.GetRoot().GetAnnotatedNodes(Compiler.LinkerAnnotationId).
                                Where(i => i.GetAnnotations(Compiler.LinkerAnnotationId).First().Data == annotation.Data).First();
                }

                return ctx_.Link(oldNode, newNode, annotation.Data, model); 
            });

            result = compilation_.ReplaceSyntaxTree(tree, node.SyntaxTree);
            model  = result.GetSemanticModel(node.SyntaxTree);
            return ctx_.ApplyLinkerInfo(node, model, result, out result);
        }

        private ExcessContext ctx_;
        private Compilation   compilation_;
    }
}
