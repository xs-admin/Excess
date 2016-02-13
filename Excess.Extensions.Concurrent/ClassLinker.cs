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
                _first = false;
                return base.VisitClassDeclaration(node);
            }

            return node; //ignore all subsequent types 
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
                    expression = expression
                        .ReplaceNodes(expression
                            .DescendantNodes()
                            .OfType<ParenthesizedLambdaExpressionSyntax>(),
                            (on, nn) => LinkExpresion(nn));

                    //create a variable to hold the expressions, then return it 
                    //after completion the results of the expression will be extracted from it 
                    var varName = expression.Type.ToString() + "_i";
                    yield return Templates
                        .ExpressionVariable
                        .Get<StatementSyntax>(varName, expression);

                    yield return yieldStatement
                        .WithExpression(CSharp.IdentifierName(varName));

                    //td: !!! assigments
                }
            }
        }

        private ParenthesizedLambdaExpressionSyntax LinkExpresion(ParenthesizedLambdaExpressionSyntax expression)
        {
            var body = expression.Body as BlockSyntax;
            if (body == null)
                return expression; //td: error checking

            return expression
                .WithBody(body
                    .WithStatements(CSharp.List(body
                        .Statements
                        .Select(st => LinkConcurrentStatement(st)))));
        }

        private StatementSyntax LinkConcurrentStatement(StatementSyntax statement)
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
            var success = invocation.ArgumentList.Arguments[1].Expression as ParenthesizedLambdaExpressionSyntax;
            var failure = invocation.ArgumentList.Arguments[2].Expression as ParenthesizedLambdaExpressionSyntax;

            if (operand is InvocationExpressionSyntax)
                return LinkProcessInvocation(operand as InvocationExpressionSyntax, success, failure);

            if (operand is IdentifierNameSyntax)
                return LinkSignal(operand as IdentifierNameSyntax, success, failure);

            throw new NotImplementedException(); //td:
        }

        private StatementSyntax LinkSignal(IdentifierNameSyntax identifierNameSyntax, ParenthesizedLambdaExpressionSyntax success, ParenthesizedLambdaExpressionSyntax failure)
        {
            throw new NotImplementedException();
        }

        private StatementSyntax LinkProcessInvocation(InvocationExpressionSyntax invocation, ParenthesizedLambdaExpressionSyntax success, ParenthesizedLambdaExpressionSyntax failure)
        {
            var call = invocation.Expression;
            if (call is IdentifierNameSyntax)
                return LinkThisInvocation(invocation, success, failure);

            return LinkExternalInvocation(invocation, success, failure);
        }

        private StatementSyntax LinkThisInvocation(InvocationExpressionSyntax invocation, ParenthesizedLambdaExpressionSyntax success, ParenthesizedLambdaExpressionSyntax failure)
        {
            Debug.Assert(invocation.Expression is IdentifierNameSyntax);

            //internal calls
            StatementSyntax result;
            if (syntaxOperation(invocation, out result))
                return result;
            else
            {
                var identifier = invocation.Expression as IdentifierNameSyntax;
                SignalModel signal = _class.GetSignal(identifier.ToString());
                if (signal != null)
                    return CSharp.ExpressionStatement(
                        invocation
                        .WithArgumentList(invocation
                            .ArgumentList
                            .AddArguments(
                                CSharp.Argument(success),
                                CSharp.Argument(failure))));
                else
                    return Templates
                        .MethodInvocation
                        .Get<StatementSyntax>(invocation, success, failure);
            }
        }

        private StatementSyntax LinkExternalInvocation(InvocationExpressionSyntax invocation, ParenthesizedLambdaExpressionSyntax success, ParenthesizedLambdaExpressionSyntax failure)
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
