using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Core;
using Excess.Compiler.Attributes;
using Excess.Compiler.Roslyn;

namespace Contract
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Linq;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    [Extension("contract")]
    public class ContractExtension
    {
        public static void Apply(ExcessCompiler compiler, Scope scope = null)
        {
            scope?.AddKeywords("contract");

            compiler.extension("contract", ParseContract);
        }

        static private Template ContractCheck = Template.ParseStatement(@"
            if (!(__0)) 
                throw new InvalidOperationException(""Breach of contract!!"");");

        static private Template ContractCheckWithException = Template.ParseStatement(@"
            if (!(__0)) 
                throw new __1();");

        private static SyntaxNode ParseContract(BlockSyntax block, Scope scope)
        {
            List<StatementSyntax> checks = new List<StatementSyntax>();
            foreach (var st in block.Statements)
            {
                var stExpression = st as ExpressionStatementSyntax;
                if (stExpression == null)
                {
                    scope.AddError("contract01", "contracts only support boolean expressions", st);
                    continue;
                }

                var expr = stExpression.Expression as BinaryExpressionSyntax;
                if (expr != null && expr.OperatorToken.IsKind(SyntaxKind.GreaterThanGreaterThanToken))
                {
                    var exprAssign = expr.Right as InvocationExpressionSyntax;
                    if (exprAssign == null)
                    {
                        scope.AddError("contract02", "contracts only invocations on the right side of >>", st);
                        continue;
                    }

                    var throwExpr = ContractCheckWithException
                        .Get<StatementSyntax>(
                            expr.Left,
                            exprAssign.Expression);

                    var newExpr = throwExpr
                        .DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Single();

                    checks.Add(throwExpr.ReplaceNode(newExpr,
                        newExpr.WithArgumentList(exprAssign.ArgumentList)));
                }
                else
                {
                    checks.Add(ContractCheck
                        .Get<StatementSyntax>(stExpression.Expression));
                }
            }

            return CSharp.Block(checks);
        }
    }
}