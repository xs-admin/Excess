using Excess.Extensions.Concurrent.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.Concurrent
{
    internal class BlockRewriter : CSharpSyntaxRewriter
    {
        ClassModel _class;
        public BlockRewriter(ClassModel @class)
        {
            _class = @class;
        }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax statement)
        {
            var expr = statement.Expression as BinaryExpressionSyntax;
            if (expr != null)
            {
                var model = new ExpressionModel(_class);
                return model.Parse(expr);
            }

            return base.VisitExpressionStatement(statement);
        }
    }
}
