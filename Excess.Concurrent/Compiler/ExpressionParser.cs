using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;
using Excess.Concurrent.Compiler.Model;
using Microsoft.CodeAnalysis.CSharp;

namespace Excess.Concurrent.Compiler
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;
    using Scope = Excess.Compiler.Scope;

    internal class ExpressionParser : CSharpSyntaxRewriter
    {
        Class _class;
        Scope _scope;
        bool _static;
        public ExpressionParser(Class @class, Scope scope, bool @static)
        {
            _class = @class;
            _scope = scope;
            _static = @static;
        }

        public bool HasConcurrent { get; internal set; }

        public override SyntaxNode VisitExpressionStatement(ExpressionStatementSyntax statement)
        {
            var result = null as SyntaxNode;
            if (statement.Expression is BinaryExpressionSyntax)
            {
                result = Parse(statement.Expression as BinaryExpressionSyntax, Templates.Cheese(_static));
                HasConcurrent = HasConcurrent || result != statement.Expression;
            }
            else if (statement.Expression is AwaitExpressionSyntax)
            {
                HasConcurrent = true;
                var expr = (statement.Expression as AwaitExpressionSyntax)
                    .Expression;

                Debug.Assert(expr != null); //td: error
                result = VisitExpressionStatement(Templates
                    .AwaitExpr
                    .Get<ExpressionStatementSyntax>(expr));
            }
            else if (statement.Expression is InvocationExpressionSyntax)
            {
                var queueStatement = null as StatementSyntax;
                if (_class.isQueueInvocation(
                    statement.Expression as InvocationExpressionSyntax,
                    false, //synch call
                    null,
                    out queueStatement))
                    return queueStatement;
            }

            if (result == null)
                result = base.VisitExpressionStatement(statement);

            if (statement.Parent is BlockSyntax)
                return result;

            //in cases such as if (x) y; 
            //we standarize the blocks, if (x) {y;}
            return CSharp.Block((StatementSyntax)result); 
        }

        public override SyntaxNode VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            var awaitExpr = null as ExpressionSyntax;
            var extraStatement = null as StatementSyntax;

            if (node.Declaration.Variables.Count == 1)
            {
                var variable = node
                    .Declaration
                    .Variables[0];

                var value = variable
                    ?.Initializer
                    ?.Value;

                if (node.Declaration.Type.ToString() == "await")
                {
                    //case: await signal;
                    awaitExpr = CSharp.IdentifierName(variable.Identifier);
                }
                else if (value != null && value is AwaitExpressionSyntax)
                {
                    //case: var a = await b();
                    extraStatement = CSharp
                        .LocalDeclarationStatement(CSharp.VariableDeclaration(
                            node.Declaration.Type, CSharp.SeparatedList(new[] {
                            CSharp.VariableDeclarator(variable.Identifier)})));

                    awaitExpr = CSharp.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        CSharp.IdentifierName(variable.Identifier),
                        (value as AwaitExpressionSyntax)
                            .Expression);
                }

            }

            if (awaitExpr != null)
            {
                var result = VisitExpressionStatement(Templates
                    .AwaitExpr
                    .Get<ExpressionStatementSyntax>(awaitExpr))
                    as StatementSyntax;

                if (extraStatement != null)
                {
                    var block = Roslyn.TrackNode(CSharp.Block(
                        extraStatement,
                        result));

                    var document = _scope.GetDocument();
                    document.change(node.Parent, Roslyn.ExplodeBlock(block));
                    return block;
                }

                return result;
            }

            return base.VisitLocalDeclarationStatement(node);
        }

        public override SyntaxNode VisitAwaitExpression(AwaitExpressionSyntax node)
        {
            Debug.Assert(false); //td: error
            return node;
        }

        private SyntaxNode Parse(BinaryExpressionSyntax expression, SyntaxToken actor)
        {
            var exprClassName = uniqueId("__expr");

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

            var startFunc = StartFunction(startStatements, exprClassName, false);

            var result = Templates
                .ExpressionInstantiation
                .Get<StatementSyntax>(exprClassName, startFunc, actor);

            return result.ReplaceNodes(result.DescendantNodes()
                .OfType<InitializerExpressionSyntax>(),
                (on, nn) => nn
                    .AddExpressions(_operators
                        .Where(op => !string.IsNullOrEmpty(op.StartName))
                        .Select(op => (ExpressionSyntax)CSharp.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            CSharp.IdentifierName(op.StartName),
                            StartFunction(op.Start, exprClassName, true)))
                        .ToArray()));
        }

        private ParenthesizedLambdaExpressionSyntax StartFunction(IEnumerable<StatementSyntax> statements, string exprClassName, bool mustEnter)
        {
            var startFunc = Templates
                .StartCallbackLambda
                .Get<ParenthesizedLambdaExpressionSyntax>(exprClassName);

            if (mustEnter)
            {
                var enterStatement = Templates
                    .StartCallbackEnter
                        .Get<StatementSyntax>(Templates.Cheese(_static));

                statements = new[]
                {
                    enterStatement
                        .ReplaceNodes(enterStatement
                            .DescendantNodes()
                            .OfType<BlockSyntax>(),
                            (on, nn) => nn.WithStatements(CSharp.List(
                                statements)))
                };
            }

            
            return startFunc
                .WithBody((startFunc.Body as BlockSyntax)
                    .AddStatements(
                        statements.ToArray()));

            //var funcLambda = startFunc
            //    .DescendantNodes()
            //    .OfType<InvocationExpressionSyntax>()
            //    .Single()
            //        .ArgumentList
            //        .Arguments[0]
            //        .Expression as ParenthesizedLambdaExpressionSyntax;

            //Debug.Assert(funcLambda != null);
            //return startFunc
            //    .ReplaceNode(funcLambda, funcLambda
            //        .WithBody((funcLambda.Body as BlockSyntax)
            //            .AddStatements(
            //                statements
            //                .ToArray())));
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
                .WithArgumentList(CSharp.ArgumentList(CSharp.SeparatedList(new[] {
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
                    leftOperand ? Roslyn.@true : Roslyn.@null,
                    leftOperand ? Roslyn.@null : Roslyn.@true,
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
            lock (_prefixes)
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
    