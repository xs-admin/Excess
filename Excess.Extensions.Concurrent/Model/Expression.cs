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

    internal class Expression
    {
        Class _class;
        public Expression(Class @class)
        {
            _class = @class;
        }

        public SyntaxNode Parse(BinaryExpressionSyntax expression)
        {
            var exprClassName = uniqueId("__expr");

            Debug.Assert(_operators == null);
            _operators = new List<Operator>();

            var startStatements = new List<StatementSyntax>();
            var root = build(expression, null, false, startStatements);

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
                    members.Add(Templates
                        .OperatorStartField
                        .Get<MemberDeclarationSyntax>(op.StartName, exprClassName));
                }
            }

            var exprClass = Templates
                .ExpressionClass
                .Get<ClassDeclarationSyntax>(exprClassName)
                .WithMembers(CSharp.List(
                    members));

            _class.AddType(exprClass);

            var startFunc = StartFunction(startStatements, exprClassName);

            var result = Templates
                .ExpressionInstantiation
                .Get<StatementSyntax>(exprClassName, startFunc);

            return result.ReplaceNodes(result.DescendantNodes()
                .OfType<InitializerExpressionSyntax>(),
                (on, nn) => nn
                    .AddExpressions(_operators
                        .Where(op => !string.IsNullOrEmpty(op.StartName))
                        .Select(op => (ExpressionSyntax)CSharp.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            CSharp.IdentifierName(op.StartName),
                            StartFunction(op.Start, exprClassName)))
                        .ToArray()));
        }

        private ParenthesizedLambdaExpressionSyntax StartFunction(IEnumerable<StatementSyntax> statements, string exprClassName)
        {
            var startFunc = Templates
                .StartCallbackLambda
                .Get<ParenthesizedLambdaExpressionSyntax>(exprClassName);

            var funcLambda = startFunc
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Single()
                    .ArgumentList
                    .Arguments[0]
                    .Expression as ParenthesizedLambdaExpressionSyntax;

            Debug.Assert(funcLambda != null);
            return startFunc
                .ReplaceNode(funcLambda, funcLambda
                    .WithBody((funcLambda.Body as BlockSyntax)
                        .AddStatements(
                            statements
                            .ToArray())));
        }

        class Operator
        {
            public Operator Parent { get; set; }
            public List<StatementSyntax> Start { get; set; }
            public string StartName { get; set; }
            public MethodDeclarationSyntax Eval { get; set; }
            public FieldDeclarationSyntax LeftValue { get; set; }
            public FieldDeclarationSyntax RightValue { get; set; }

            public SyntaxToken Callback
            {
                get
                {
                    Debug.Assert(Eval != null);
                    return Eval.Identifier;
                }
            }
        }

        List<Operator> _operators;
        private Operator build(BinaryExpressionSyntax expr, Operator parent, bool leftOfParent, List<StatementSyntax> start)
        {
            var exprOperator = new Operator();
            exprOperator.Parent = parent;

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
            ExpressionSyntax success = parent == null
                ? Templates
                    .ExpressionCompleteCall
                    .Get<ExpressionSyntax>(Roslyn.@true, Roslyn.@null)
                : CreateCallback(true, leftOfParent, parent.Callback);

            ExpressionSyntax failure = parent == null
                ? Templates
                    .ExpressionCompleteCall
                    .Get<ExpressionSyntax>(Roslyn.@false, Templates.FailureParameter)
                : CreateCallback(false, leftOfParent, parent.Callback);

            var continuationStart = null as List<StatementSyntax>;
            switch (expr.OperatorToken.Text)
            {
                case "&":
                case "&&":
                    exprOperator.Eval = Templates
                        .AndOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, success, failure);
                    break;

                case "|":
                case "||":
                    exprOperator.Eval = Templates
                        .OrOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, success, failure);
                    break;

                case ">>":
                    exprOperator.StartName = uniqueId("__start");
                    exprOperator.Start = new List<StatementSyntax>();
                    continuationStart = exprOperator.Start;
                    exprOperator.Eval = Templates
                        .ContinuationOperatorEval
                        .Get<MethodDeclarationSyntax>(evalName, leftId, rightId, success, failure, exprOperator.StartName);
                    break;

                default:
                    throw new NotImplementedException(); //td: error
            }

            //register
            _operators.Add(exprOperator);

            //recurse
            BinaryExpressionSyntax binaryExpr;
            if (isBinaryExpressionSyntax(expr.Left, out binaryExpr))
                build(binaryExpr, exprOperator, true, start);
            else
                addStart(start, expr.Left, true, evalName);

            if (isBinaryExpressionSyntax(expr.Right, out binaryExpr))
                build(binaryExpr, exprOperator, false, continuationStart ?? start);
            else
                addStart(continuationStart ?? start, expr.Right, false, evalName);

            return exprOperator;
        }

        private ExpressionSyntax CreateCallback(bool success, bool leftOfParent, SyntaxToken token)
        {
            var arg1 = leftOfParent
                ? success ? Roslyn.@true : Roslyn.@false
                : Roslyn.@null;

            var arg2 = leftOfParent
                ? Roslyn.@null
                : success ? Roslyn.@true : Roslyn.@false;

            var ex = success
                ? Roslyn.@null
                : Templates.FailureParameter;

            return CSharp
                .InvocationExpression(CSharp.IdentifierName(token))
                .WithArgumentList(CSharp.ArgumentList(CSharp.SeparatedList( new [] {
                    CSharp.Argument(arg1),
                    CSharp.Argument(arg2),
                    CSharp.Argument(ex)})));
        }

        private bool isBinaryExpressionSyntax(ExpressionSyntax expr, out BinaryExpressionSyntax result)
        {
            result = expr as BinaryExpressionSyntax;
            if (result != null)
                return true;

            if (expr is ParenthesizedExpressionSyntax)
                return isBinaryExpressionSyntax((expr as ParenthesizedExpressionSyntax).Expression, out result);

            return false;
        }

        private void addStart(List<StatementSyntax> start, ExpressionSyntax expr, bool leftOperand, string callbackName)
        {
            var success = Templates
                .StartCallback
                .Get<ExpressionSyntax>(
                    _class.Name,
                    callbackName,
                    leftOperand? Roslyn.@true : Roslyn.@null,
                    leftOperand? Roslyn.@null : Roslyn.@true,
                    Roslyn.@null);

            var failure = Templates
                .StartCallback
                .Get<ExpressionSyntax>(
                    _class.Name,
                    callbackName,
                    leftOperand ? Roslyn.@false : Roslyn.@null,
                    leftOperand ? Roslyn.@null : Roslyn.@false,
                    Templates.FailureParameter);

            start
                .Add(CSharp.ExpressionStatement(Templates
                    .StartExpression
                    .Get<ExpressionSyntax>(expr, success, failure)));
        }

        static Dictionary<string, int> _prefixes = new Dictionary<string, int>();

        public static string uniqueId(string prefix)
        {
            lock(_prefixes)
            {
                int value;
                if (!_prefixes.TryGetValue(prefix, out value))
                    value = 1;

                _prefixes[prefix] = value + 1;
                return prefix + value;
            }
        }

        private string operatorName()
        {
            return uniqueId("__op");
        }
    }
}
