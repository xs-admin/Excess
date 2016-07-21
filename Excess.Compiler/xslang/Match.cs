using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;

namespace xslang
{
    using System;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using ExcessCompiler = ICompiler<SyntaxToken, SyntaxNode, SemanticModel>;

    public class Match
    {
        public static void Apply(ExcessCompiler compiler)
        {
            var lexical = compiler.Lexical();

            lexical.match()
                .token("match", named: "keyword")
                .enclosed('(', ')')
                .token('{')
                .then(lexical.transform()
                    .replace("keyword", "switch")
                    .then(ProcessMatch, referenceToken: "keyword"));
        }

        private static SyntaxNode ProcessMatch(SyntaxNode node, Scope scope)
        {
            var switchExpr = node as SwitchStatementSyntax;
            if (switchExpr == null)
            {
                scope.AddError("match01", "malformed match", node);
                return node;
            }

            //store items to simplify
            var cases = new List<ExpressionSyntax>();
            var statements = new List<StatementSyntax>();
            var defaultStatement = null as StatementSyntax;

            foreach (var section in switchExpr.Sections)
            {
                bool isDefault;
                var expr = caseExpression(section.Labels, switchExpr.Expression, out isDefault);

                StatementSyntax statement = caseStatement(section.Statements);
                if (isDefault && section.Labels.Count == 1)
                {
                    defaultStatement = statement;
                }
                else
                {
                    cases.Add(expr);
                    statements.Add(statement);

                    if (isDefault)
                        defaultStatement = statement;
                }
            }

            if (cases.Count != statements.Count)
            {
                scope.AddError("match01", "malformed match", node);
                return node;
            }

            //convert cases to ifs
            var last = cases.Count - 1;
            if (last >= 0)
            {
                IfStatementSyntax result = CSharp.IfStatement(cases[last], statements[last]);

                if (defaultStatement != null)
                    result = result.WithElse(CSharp.ElseClause(defaultStatement));


                for (int i = last - 1; i >= 0; i--)
                {
                    result = CSharp.IfStatement(cases[i], statements[i])
                        .WithElse(CSharp.ElseClause(result));
                }

                return result;
            }
            else
                return switchExpr;
        }

        private static StatementSyntax caseStatement(SyntaxList<StatementSyntax> statements)
        {
            return CSharp.Block(
                validCaseStatements(statements));
        }

        private static IEnumerable<StatementSyntax> validCaseStatements(SyntaxList<StatementSyntax> statements)
        {
            foreach (var statement in statements)
            {
                if (statement is BreakStatementSyntax)
                    continue;

                yield return statement;
            }
        }

        private static ExpressionSyntax caseExpression(SyntaxList<SwitchLabelSyntax> labels, ExpressionSyntax control, out bool isDefault)
        {
            isDefault = false;
            List<ExpressionSyntax> cases = new List<ExpressionSyntax>();
            foreach (var label in labels)
            {
                bool thisDefault;
                var expr = caseExpression(label, control, out thisDefault);
                isDefault |= thisDefault;

                if (!thisDefault)
                    cases.Add(expr);
            }

            switch (cases.Count)
            {
                case 0: return null;
                case 1: return cases[0];
                default: return CreateOrExpression(cases);
            }
        }

        private static ExpressionSyntax CreateOrExpression(List<ExpressionSyntax> cases)
        {
            Debug.Assert(cases.Count >= 2);
            var result = CSharp.BinaryExpression(SyntaxKind.LogicalOrExpression, cases[0], cases[1]);
            for(int i = 2; i < cases.Count; i++)
                result = CSharp.BinaryExpression(SyntaxKind.LogicalOrExpression, result, cases[i]);

            return result;
        }

        private static ExpressionSyntax caseExpression(SwitchLabelSyntax label, ExpressionSyntax control, out bool isDefault)
        {
            isDefault = label.Keyword.Kind() == SyntaxKind.DefaultKeyword;

            var caseLabel = label as CaseSwitchLabelSyntax;
            if (caseLabel != null)
            {
                var result = caseLabel.Value;
                if (result is BinaryExpressionSyntax)
                {
                    var expr = result as BinaryExpressionSyntax;
                    if (expr.Left.IsMissing)
                        return CSharp.BinaryExpression(expr.Kind(), control, expr.Right);

                    return expr;
                }

                return CSharp.BinaryExpression(SyntaxKind.EqualsExpression, control, result);
            }

            Debug.Assert(isDefault);
            return null;
        }
    }
}
