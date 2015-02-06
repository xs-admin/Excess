using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.Extensions
{
    public class Asynch
    {
        public static void Apply(RoslynCompiler compiler)
        {
            var sintaxis = compiler.Sintaxis();

            //code extension
            sintaxis
                .extension("asynch", ExtensionKind.Code, ProcessAsynch)
                .extension("synch", ExtensionKind.Code, ProcessSynch);
        }

        private static SyntaxNode ProcessAsynch(SyntaxNode node, Scope scope, SyntacticalExtension<SyntaxNode> extension)
        {
            if (extension.Kind == ExtensionKind.Code)
            {
                var result = AsynchTemplate
                    .ReplaceNodes(AsynchTemplate
                        .DescendantNodes()
                        .OfType<BlockSyntax>(), 
                     (oldNode, newNode) => extension.Body);

                var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
                document.change(node.Parent, RoslynCompiler.AddStatement(ContextVariable, before: node));

                return result;
            }

            //td: error, asynch cannot return a value
            return node;
        }

        private static SyntaxNode ProcessSynch(SyntaxNode node, Scope scope, SyntacticalExtension<SyntaxNode> extension)
        {
            if (extension.Kind == ExtensionKind.Code)
            {
                //td: verify it's inside an asynch
                return SynchTemplate
                    .ReplaceNodes(SynchTemplate
                        .DescendantNodes()
                        .OfType<BlockSyntax>(),
                     (oldNode, newNode) => extension.Body);
            }

            //td: error, synch cannot return a value
            return node;
        }

        static private StatementSyntax ContextVariable = CSharp.ParseStatement(@"
            SynchronizationContext __ASynchCtx = SynchronizationContext.Current; ");

        static private StatementSyntax AsynchTemplate = CSharp.ParseStatement(@"
            Task.Factory.StartNew(() =>
            {
            });");

        static private StatementSyntax SynchTemplate = CSharp.ParseStatement(@"
            __ASynchCtx.Post(() => 
            { 
            });");
    }
}
