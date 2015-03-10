using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Entensions.XS
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Asynch
    {
        public static void Apply(ExcessCompiler compiler)
        {
            var syntax = compiler.Syntax();

            //code extension
            syntax
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

            scope.AddError("asynch01", "asynch does not return a value", node);
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

            scope.AddError("synch01", "synch does not return a value", node);
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
