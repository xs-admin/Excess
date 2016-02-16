using Excess.Compiler.Roslyn;
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
    using Microsoft.CodeAnalysis.CSharp;
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class ClassLinker : CSharpSyntaxRewriter
    {
        ClassModel _class;
        SemanticModel _model;
        public ClassLinker(ClassModel @class, SemanticModel model)
        {
            _class = @class;
            _model = model;
        }

        bool _first = true;
        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (_first)
            {
                //the one we are linking
                _first = false;
                return base.VisitClassDeclaration(node);
            }

            //custom types, may be the generated expression types
            var fields = null as Dictionary<string, TypeSyntax>;
            if (_typeAssignments.TryGetValue(node.Identifier.ToString(), out fields))
                return node.AddMembers(fields
                    .Select(f => Templates
                    .AssignmentField
                    .Get<MemberDeclarationSyntax>(f.Key, f.Value))
                    .ToArray());

            return node; //let it go unmodified
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (_class.IsSignal(node))
                return base.VisitMethodDeclaration(node);

            return node;
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            return node
                .WithStatements(CSharp.List(node.Statements
                    .SelectMany(st => LinkExpressionStatement(st))));
        }

        Dictionary<string, Dictionary<string, TypeSyntax>> _typeAssignments = new Dictionary<string, Dictionary<string, TypeSyntax>>();
        private IEnumerable<StatementSyntax> LinkExpressionStatement(StatementSyntax statement)
        {
            var yieldStatement = statement as YieldStatementSyntax;
            if (yieldStatement == null)
                yield return statement;
            else
            {
                var expression = yieldStatement
                    .Expression
                    as ObjectCreationExpressionSyntax;

                if (expression == null)
                    yield return statement;
                else
                {
                    var assignments = new Dictionary<string, TypeSyntax>();
                    var typeName = expression.Type.ToString();

                    expression = expression
                        .ReplaceNodes(expression
                            .DescendantNodes()
                            .OfType<ParenthesizedLambdaExpressionSyntax>(),
                            (on, nn) => LinkExpresion(nn, assignments));

                    //keep the assignments as to add fields to the expression classes
                    if (assignments.Any())
                        _typeAssignments[typeName] = assignments;

                    //create a variable to hold the expressions, then return it 
                    //after completion the results of the expression will be extracted from it
                    var varName = typeName + "_var";
                    yield return Templates
                        .ExpressionVariable
                        .Get<StatementSyntax>(varName, expression);

                    yield return yieldStatement
                        .WithExpression(CSharp.IdentifierName(varName));

                    //we must check whether the expression failed
                    yield return Templates
                        .ExpressionFailedCheck
                        .Get<StatementSyntax>(varName);

                    //and finally, we must generate expression assignments for 
                    //all the assigments inside the concurrent expression
                    foreach (var assigment in assignments)
                    {
                        yield return Templates
                            .AssigmentAfterExpression
                            .Get<StatementSyntax>(
                                assigment.Key,
                                varName);
                    }
                }
            }
        }

        private ParenthesizedLambdaExpressionSyntax LinkExpresion(ParenthesizedLambdaExpressionSyntax expression, Dictionary<string, TypeSyntax> assignments)
        {
            var body = expression.Body as BlockSyntax;
            if (body == null)
                return expression; //td: error checking

            return expression
                .WithBody(body
                    .WithStatements(CSharp.List(body
                        .Statements
                        .Select(st => LinkConcurrentStatement(st, assignments)))));
        }

        private StatementSyntax LinkConcurrentStatement(StatementSyntax statement, Dictionary<string, TypeSyntax> assignments)
        {
            //we encode the process (in whichever form) as call in the form
            //__marker__(process, success, failure);
            //why? why not?
            var exprStatement = statement as ExpressionStatementSyntax;
            if (exprStatement == null)
                return statement; //td: error checking

            var invocation = exprStatement.Expression as InvocationExpressionSyntax;
            if (    invocation == null
                ||  invocation.Expression.ToString() != "__marker__"
                ||  invocation.ArgumentList.Arguments.Count != 3)
                return statement; //td: error checking


            var operand = invocation.ArgumentList.Arguments[0].Expression;
            var success = invocation.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax;
            var failure = invocation.ArgumentList.Arguments[2].Expression as InvocationExpressionSyntax;

            return LinkOperand(operand, success, failure, assignments);
        }

        private StatementSyntax LinkOperand(ExpressionSyntax operand, InvocationExpressionSyntax success, InvocationExpressionSyntax failure, Dictionary<string, TypeSyntax> assignments)
        {
            if (operand is InvocationExpressionSyntax)
                return LinkProcessInvocation(operand as InvocationExpressionSyntax, success, failure);

            if (operand is IdentifierNameSyntax)
                return LinkSignal(operand as IdentifierNameSyntax, success, failure);

            if (operand is AssignmentExpressionSyntax)
                return LinkAssignment(operand as AssignmentExpressionSyntax, success, failure, assignments);

            if (operand is ParenthesizedExpressionSyntax)
                return LinkOperand((operand as ParenthesizedExpressionSyntax).Expression, success, failure, assignments);

            throw new NotImplementedException(); //td:
        }

        private StatementSyntax LinkAssignment(AssignmentExpressionSyntax assignment, InvocationExpressionSyntax success, InvocationExpressionSyntax failure, Dictionary<string, TypeSyntax> assignments)
        {
            var leftString = assignment.Left.ToString();
            var leftType = Roslyn.SymbolTypeSyntax(_model, assignment.Left);

            Debug.Assert(assignment.Left is IdentifierNameSyntax);
            Debug.Assert(!assignments.ContainsKey(leftString));
            Debug.Assert(leftType != null); //td: error

            assignments[leftString] = leftType;

            var emptyAssignments = new Dictionary<string, TypeSyntax>();
            var right = LinkOperand(assignment.Right, success, failure, emptyAssignments);

            Debug.Assert(right != null);
            Debug.Assert(!emptyAssignments.Any());

            //there are 2 scenarios, first, the right operand was a concurrent expression
            //in which case it would have a success function
            var successFunc = right
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Where(fn => fn
                    .ParameterList
                    .Parameters
                    .Any(p => p.Identifier.ToString() == "__res"))
                .SingleOrDefault();

            if (successFunc != null)
                return right.ReplaceNode(successFunc, successFunc
                    .WithBody(CSharp.Block( new[] {
                        Templates
                            .ExpressionAssigment
                            .Get<StatementSyntax>(assignment.Left, leftType) }
                        .Union(successFunc.Body is BlockSyntax
                            ? (successFunc.Body as BlockSyntax)
                                .Statements
                                .AsEnumerable()
                            : new[] { successFunc.Body as StatementSyntax })
                        .ToArray())));

            //else, we need to substitute the actual Right expr by 
            //an assignment.
            var rightString = assignment.Right.ToString();
            return right.ReplaceNodes(right.
                DescendantNodes()
                .OfType<ExpressionSyntax>()
                .Where(node => node.ToString().Equals(rightString)),
                (on, nn) => CSharp.AssignmentExpression(
                    assignment.Kind(), 
                    Templates
                        .ExpressionProperty
                        .Get<ExpressionSyntax>(assignment.Left),
                    nn));
        }

        private ParenthesizedLambdaExpressionSyntax WrapInLambda(params ExpressionSyntax[] expressions)
        {
            return WrapInLambda(expressions
                .AsEnumerable());
        }

        private ParenthesizedLambdaExpressionSyntax WrapInLambda(IEnumerable<ExpressionSyntax> expressions)
        {
            return CSharp.ParenthesizedLambdaExpression(
                CSharp.Block(expressions
                    .Select(e => CSharp.ExpressionStatement(e))));
        }

        private StatementSyntax LinkSignal(IdentifierNameSyntax name, InvocationExpressionSyntax success, InvocationExpressionSyntax failure)
        {
            var signalName = name.ToString();
            var signal = _class.GetSignal(signalName);
            Debug.Assert(signal != null);

            return Templates
                .SignalListener
                .Get<StatementSyntax>(
                    Roslyn.Quoted(signalName),
                    WrapInLambda(success));
        }

        private StatementSyntax LinkProcessInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax success, InvocationExpressionSyntax failure)
        {
            var call = invocation.Expression;
            if (call is IdentifierNameSyntax)
                return LinkThisInvocation(invocation, success, failure);

            return LinkExternalInvocation(invocation, success, failure);
        }

        private StatementSyntax LinkThisInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax success, InvocationExpressionSyntax failure)
        {
            Debug.Assert(invocation.Expression is IdentifierNameSyntax);

            //internal calls
            StatementSyntax result;
            if (syntaxOperation(invocation, out result))
                return result;
            else
            {
                var identifier = (invocation.Expression as IdentifierNameSyntax)
                    .ToString();
                SignalModel signal = _class.GetSignal(identifier);
                if (signal != null)
                    return CSharp.ExpressionStatement(
                        invocation
                        .WithExpression(CSharp.IdentifierName("__concurrent" + identifier))
                        .WithArgumentList(invocation
                            .ArgumentList
                            .AddArguments(
                                CSharp.Argument(WrapInLambda(success)
                                    .AddParameterListParameters(CSharp.Parameter(CSharp.ParseToken(
                                        "__res")))),
                                CSharp.Argument(WrapInLambda(failure)
                                    .AddParameterListParameters(CSharp.Parameter(CSharp.ParseToken(
                                        "__ex")))))));

                return Templates
                    .MethodInvocation
                    .Get<StatementSyntax>(invocation, success, failure);
            }
        }

        private StatementSyntax LinkExternalInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax success, InvocationExpressionSyntax failure)
        {
            Debug.Assert(invocation.Expression is MemberAccessExpressionSyntax);
            Debug.Assert(!invocation.Expression
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any()); //td: errors

            var symbol = _model.GetSymbolInfo(invocation.Expression).Symbol;
            if (symbol == null)
                return CSharp.ExpressionStatement(invocation);

            if (isConcurrent(symbol.ContainingType))
                return CSharp.ExpressionStatement(
                    invocation
                    .WithArgumentList(invocation
                        .ArgumentList
                        .AddArguments(
                            CSharp.Argument(success),
                            CSharp.Argument(failure))));

            return Templates
                .MethodInvocation
                .Get<StatementSyntax>(invocation, success, failure);
        }

        private bool isConcurrent(INamedTypeSymbol type)
        {
            return type != null && type.MemberNames.Contains("__concurrent__");

        }

        private bool syntaxOperation(InvocationExpressionSyntax invocation, out StatementSyntax result)
        {
            result = null;
            switch (invocation.Expression.ToString())
            {
                case "wait":
                case "timeout":
                    throw new NotImplementedException();
                case "parallel":
                    throw new NotImplementedException();
                default:
                    return false;            
            }

            return true;
        }
    }
}
