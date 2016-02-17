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
        Class _class;
        public BlockRewriter(Class @class)
        {
            _class = @class;
        }

        public bool HasConcurrent { get; internal set; }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax statement)
        {
            var expr = statement.Expression as BinaryExpressionSyntax;

            if (expr != null)
            {
                var model = new Expression(_class);
                var result = model.Parse(expr);

                HasConcurrent = HasConcurrent || result != expr;
                return result;
            }

            return base.VisitExpressionStatement(statement);
        }
    }
}
