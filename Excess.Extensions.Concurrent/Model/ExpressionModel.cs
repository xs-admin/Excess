using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Excess.Compiler.Roslyn;

namespace Excess.Extensions.Concurrent.Model
{
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class ExpressionModel
    {
        ClassModel _class;
        public ExpressionModel(ClassModel @class)
        {
            _class = @class;
        }

        public SyntaxNode Parse(BinaryExpressionSyntax expression)
        {
            Debug.Assert(_operators == null);
            _operators = new List<Operator>();
            build(expression, null, operatorName());

            if (!_operators.Any())
                return expression;

            var members = new List<MemberDeclarationSyntax>();
            foreach (var op in _operators)
            {
                if (op.Eval != null)
                    members.Add(op.Eval);

                if (op.LeftValue != null)
                    members.Add(op.LeftValue);

                if (op.RightValue != null)
                    members.Add(op.RightValue);

                if (op.StartName != null && op.StartName.Any())
                {
                    members.Add(CSharp.MethodDeclaration(Roslyn.@void, op.StartName)
                        .WithBody(CSharp.Block(
                            op.Start)));
                }
            }

            var exprClassName = expressionName();
            var exprClass = Templates
                .ExpressionClass
                .Get<ClassDeclarationSyntax>(exprClassName)
                .WithMembers(CSharp.List(
                    members));

            _class.AddType(exprClass);
            var result = Templates
                .ExpressionInstantiation
                .Get<StatementSyntax>(exprClassName);

            return result.ReplaceNodes(result.DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>(),
                (on, nn) => nn
                    .WithInitializer(CSharp.InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        CSharp.SeparatedList(_operators
                            .Where(op => !string.IsNullOrEmpty(op.StartName))
                            .Select(op => (ExpressionSyntax)CSharp.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            CSharp.IdentifierName(op.StartName),
                            CSharp.ParenthesizedLambdaExpression(CSharp.Block(
                               op.Start))))))));
        }

        class Operator
        {
            public Operator Parent { get; set; }
            public List<StatementSyntax> Start { get; set; }
            public string StartName { get; set; }
            public MethodDeclarationSyntax Eval { get; set; }
            public FieldDeclarationSyntax LeftValue { get; set; }
            public FieldDeclarationSyntax RightValue { get; set; }

            public ExpressionSyntax Callback
            {
                get
                {
                    Debug.Assert(Eval != null);
                    return CSharp.LiteralExpression(SyntaxKind.IdentifierName, Eval.Identifier);
                }
            }
        }

        List<Operator> _operators;
        private void build(BinaryExpressionSyntax expr, Operator parent, string startFunction)
        {
            var exprOperator = new Operator();
            exprOperator.Parent = parent;
            exprOperator.Start = startFunction != null ? new List<StatementSyntax>() : null;
            exprOperator.StartName = startFunction;

            var evalName = operatorName();
            var leftId = evalName + "_Left";
            var rightId = evalName + "_Right";

            //generate internal fields to hold the current state of an operand
            exprOperator.LeftValue = Templates
                .OperatorState
                .Get<FieldDeclarationSyntax>(leftId);
            exprOperator.RightValue = Templates
                .OperatorState
                .Get<FieldDeclarationSyntax>(rightId);

            //generate method to update the expression
            ExpressionSyntax callback = parent == null
                ? Roslyn.@null
                : parent.Callback;

            var continuationName = null as string;
            switch (expr.OperatorToken.Text)
            {
                case "&":
                case "&&":
                    exprOperator.Eval = Templates
                        .OrOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, callback);
                    break;

                case "|":
                case "||":
                    exprOperator.Eval = Templates
                        .AndOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, callback);
                    break;

                case ">>":
                    continuationName = operatorName();
                    exprOperator.Eval = Templates
                        .ContinuationOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, callback, continuationName);
                    break;

                default:
                    throw new NotImplementedException(); //td: error
            }

            //recurse
            if (expr.Left is BinaryExpressionSyntax)
                build(expr.Left as BinaryExpressionSyntax, exprOperator, null);
            else
                addStart(exprOperator, expr.Left, true, evalName);

            if (expr.Right is BinaryExpressionSyntax)
                build(expr.Right as BinaryExpressionSyntax, exprOperator, continuationName);
            else if (continuationName != null)
            {
                //case: continuation into a simple expression
                var contOperator = new Operator
                {
                    Parent = exprOperator,
                };

                addStart(contOperator, expr.Right, false, evalName);
            }
            else
                addStart(exprOperator, expr.Right, false, evalName);
        }

        private void addStart(Operator exprOperator, ExpressionSyntax expr, bool leftOperand, string callbackName)
        {
            if (exprOperator.StartName == null)
            {
                exprOperator.StartName = operatorName();
                exprOperator.Start = new List<StatementSyntax>();
            }

            var callback = Templates
                .StartCallback
                .Get<ExpressionSyntax>(
                    callbackName,
                    leftOperand? CSharp.IdentifierName("result") : Roslyn.@null,
                    leftOperand? Roslyn.@null : CSharp.IdentifierName("result"));

            exprOperator
                .Start
                .Add(CSharp.ExpressionStatement(Templates
                    .StarExpression
                    .Get<ExpressionSyntax>(expr, callback)));
        }

        static string _operatorPrefix = "__op";
        static int _oidx = 0;
        private string operatorName()
        {
            return _operatorPrefix + _oidx++;
        }

        static string _exprPrefix = "__expr";
        static int _eidx = 0;
        private string expressionName()
        {
            return _exprPrefix + _eidx++;
        }
    }
}
