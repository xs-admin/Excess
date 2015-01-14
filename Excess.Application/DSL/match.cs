using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using Excess.Core;
using Excess.RuntimeProject;

namespace Excess.DSL
{
    public class MatchFactory
    {
        public static string DSLName = "match";

        public static IDSLFactory Create()
        {
            PureParser.DSLName = DSLName;
            PureLinker.DSLName = DSLName;

            var parser = new MatchParser();
            var linker = new MatchLinker();

            parser.Linker = linker;
            return new ManagedDSLFactory(DSLName, parser, linker);
        }
    }

    public class MatchParser : ManagedParser<MatchLinker>
    {
        public SyntaxNode ParseCode(SyntaxNode node, string id, ParameterListSyntax args, BlockSyntax code, bool expectsResult)
        {
            ExpressionSyntax control = null;
            if (args.Parameters.Count > 0)
                control = SyntaxFactory.IdentifierName(args.Parameters[0].Identifier);

            //all the match element supports are case/default
            if (!startsWithCase(code))
            {
                Error(node, "match requires switch like cases");
                return null;
            }

            List<ExpressionSyntax> cases      = new List<ExpressionSyntax>();
            List<StatementSyntax>  statements = new List<StatementSyntax>();
            StatementSyntax        defaultStatement = null; 

            bool expectingCase          = true;
            bool expectDefaultOrEnd     = false;
            bool expectDefaultStatement = false;
            foreach (var child in node.ChildNodes())
            {
                if (expectDefaultOrEnd)
                {
                    if (child.ToString().Trim() != "default" || child.GetTrailingTrivia().ToString().Trim() != ":")
                    {
                        Error(child, "expecting match default");
                        return null;
                    }

                    expectDefaultStatement = true;
                    expectDefaultOrEnd     = false;
                    expectingCase          = false; 
                }
                else if (expectingCase)
                {
                    expectingCase = false;
                    ExpressionSyntax caseExpression = getCaseExpression(control, child);
                    if (caseExpression == null)
                        return null; //something went wrong

                    cases.Add(caseExpression);
                }
                else
                {
                    expectingCase = true;
                    StatementSyntax statement = getCaseStatement(child, out expectDefaultOrEnd);
                    if (statement == null)
                        return null; //something went wrong

                    if (expectDefaultStatement)
                        defaultStatement = statement;
                    else
                        statements.Add(statement);

                    expectDefaultStatement = false;
                }
            }
            
            return buildIfStatement(node, cases, statements, defaultStatement);
        }

        private bool startsWithCase(BlockSyntax code)
        {
            foreach (var trivia in code.OpenBraceToken.GetAllTrivia())
            {
                if (trivia.IsKind(SyntaxKind.SkippedTokensTrivia) && trivia.ToString() == "case")
                    return true;
            }

            return false;
        }

        private ExpressionSyntax getCaseExpression(ExpressionSyntax control, SyntaxNode child)
        {
            ExpressionStatementSyntax statement = child as ExpressionStatementSyntax;
            ExpressionSyntax          result    = statement != null? statement.Expression : null;
            if (result == null)
            {
                Error(child, "match expected expression, got '" + child.ToString() + "'");
                return null;
            }

            var ttrivia = child.GetTrailingTrivia().ToString().Trim();
            if (ttrivia != ":")
            {
                Error(child, ": expected after a match epression");
                return null;
            }

            if (result is BinaryExpressionSyntax)
            {
                var expr = result as BinaryExpressionSyntax;
                if (expr.Left.IsMissing)
                {
                    if (control == null)
                    {
                        Error(child, "controlless match can only evaluate binary expressions");
                        return null;
                    }

                    return SyntaxFactory.BinaryExpression(expr.CSharpKind(), control, expr.Right);
                }

                return expr;
            }

            if (control == null)
            {
                Error(child, "controlless match can only evaluate binary expressions");
                return null;
            }

            return SyntaxFactory.BinaryExpression(SyntaxKind.EqualsExpression, control, result);
        }

        private StatementSyntax getCaseStatement(SyntaxNode child, out bool expectDefaultOrEnd)
        {
            expectDefaultOrEnd = false;

            StatementSyntax result = child as StatementSyntax;
            if (result == null)
            {
                Error(child, "expecting statement after case");
                return null;
            }

            var trivia = child.GetTrailingTrivia().ToString().Trim();
            expectDefaultOrEnd = trivia != "case";
            return result.WithoutTrailingTrivia();
        }

        private SyntaxNode buildIfStatement(SyntaxNode mainNode, List<ExpressionSyntax> cases, List<StatementSyntax> statements, StatementSyntax defaultStatement)
        {
            if (cases.Count == 0)
            {
                Error(mainNode, "match expects at least one case");
                return null;
            }

            var last = cases.Count - 1;
            IfStatementSyntax result = SyntaxFactory.IfStatement(cases[last], statements[last]);
            if (defaultStatement != null)
                result = result.WithElse(SyntaxFactory.ElseClause(defaultStatement));


            for (int i = last - 1; i >= 0; i--)
            {
                result = SyntaxFactory.IfStatement(cases[i], statements[i])
                    .WithElse(SyntaxFactory.ElseClause(result));
            }

            return result;
        }
    }

    public class MatchLinker : ManagedLinker
    {
        public SyntaxNode Link(SyntaxNode node, SemanticModel model)
        {
            return node;
        }
    }
}