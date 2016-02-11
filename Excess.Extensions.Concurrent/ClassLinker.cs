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
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class ClassLinker : CSharpSyntaxRewriter
    {
        ClassModel _class;
        public ClassLinker(ClassModel @class)
        {
            _class = @class;
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
                        .Select(st => LinkStatement(st)))));
        }

        private StatementSyntax LinkStatement(StatementSyntax statement)
        {
            //we encode the process (in whichever form) as call in the form
            //__marker__(process, success);
            //why? why not?
            var exprStatement = statement as ExpressionStatementSyntax;
            if (exprStatement == null)
                return statement; //td: error checking

            var invocation = exprStatement.Expression as InvocationExpressionSyntax;
            if (    invocation == null
                ||  invocation.Expression.ToString() != "__marker__"
                ||  invocation.ArgumentList.Arguments.Count != 2)
                return statement; //td: error checking


            var operand = invocation.ArgumentList.Arguments[0].Expression;
            var continuation = invocation.ArgumentList.Arguments[1].Expression as InvocationExpressionSyntax;

            if (operand is InvocationExpressionSyntax)
                return LinkProcessInvocation(operand as InvocationExpressionSyntax, continuation);

            if (operand is IdentifierNameSyntax)
                return LinkSignal(operand as IdentifierNameSyntax, continuation);

            throw new NotImplementedException(); //td:
        }

        private StatementSyntax LinkSignal(IdentifierNameSyntax identifierNameSyntax, InvocationExpressionSyntax continuation)
        {
            throw new NotImplementedException();
        }

        private StatementSyntax LinkProcessInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax continuation)
        {
            var call = invocation.Expression;
            if (call is IdentifierNameSyntax)
                return LinkThisInvocation(invocation, continuation);

            return LinkExternalInvocation(invocation, continuation);
        }

        private StatementSyntax LinkThisInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax continuation)
        {
            throw new NotImplementedException();
        }

        private StatementSyntax LinkExternalInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax continuation)
        {
            throw new NotImplementedException();
        }
    }
}
