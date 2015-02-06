using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.Extensions
{
    public class Contract
    {
        public static void Apply(RoslynCompiler compiler)
        {
            var sintaxis = compiler.Sintaxis();

            sintaxis
                .extension("contract", ExtensionKind.Code, ProcessContract);
        }

        private static SyntaxNode ProcessContract(SyntaxNode node, Scope scope, SyntacticalExtension<SyntaxNode> extension)
        {
            if (extension.Kind == ExtensionKind.Code)
            {
                var block = extension.Body as BlockSyntax;
                Debug.Assert(block != null);

                List<StatementSyntax> checks = new List<StatementSyntax>();
                foreach (var st in block.Statements)
                {
                    var stExpression = st as ExpressionStatementSyntax;
                    if (stExpression == null)
                    {
                        //td: error, contracts only support boolean expression
                        continue;
                    }

                    var contractCheck = ContractCheck
                        .ReplaceNodes(ContractCheck
                            .DescendantNodes()
                            .OfType<ExpressionSyntax>()
                            .Where(expr => expr.ToString() == "__condition"),

                         (oldNode, newNode) => 
                            stExpression.Expression);

                    checks.Add(contractCheck);
                }

                return CSharp.Block(checks);
            }

            //td: error, contract cannot return a value
            return node;
        }

        static private StatementSyntax ContractCheck = CSharp.ParseStatement(@"
            if (!(__condition)) 
                throw new InvalidOperationException(""Breach of contract!!"");");
    }
}
