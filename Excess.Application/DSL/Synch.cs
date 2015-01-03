using Excess.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Excess.DSL
{
    public class SynchDSL : IDSLHandler
    {
        public SyntaxNode compile(ExcessContext ctx, DSLContext dctx)
        {
            Debug.Assert(dctx.Surroundings == DSLSurroundings.Code);
            return null;
        }

        public SyntaxNode link(ExcessContext ctx, SyntaxNode node, SemanticModel model)
        {
            return node;
        }

        public SyntaxNode setCode(ExcessContext ctx, DSLContext dctx, BlockSyntax code)
        {
            Debug.Assert(dctx.Surroundings == DSLSurroundings.Code);

            SyntaxNode result = ctx.Compile(code);
            SyntaxNode node = dctx.MainNode;

            return Template.ReplaceNode(Template.DescendantNodes().OfType<BlockSyntax>().First(), code);
        }

        static private StatementSyntax Template = SyntaxFactory.ParseStatement(@"
            __ASynchCtx.Post(() => 
            { 
            });");
    }
}