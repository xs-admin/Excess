using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler.Roslyn;
using Excess.Extensions.Concurrent.Model;

namespace Excess.Extensions.Concurrent
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    internal class ExpressionLinker : CSharpSyntaxRewriter
    {
        Class _class;
        SemanticModel _model;
        public ExpressionLinker(Class @class, SemanticModel model)
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
            return node.WithStatements(CSharp.List(
                node
                .Statements
                .SelectMany(st => LinkExpressionStatement(st))
                .ToArray()));
        }

        //the issue at hand, we cannot have "yield return"s inside a try catch (thanks, Obama)
        //So we will split the try catch(es) around the requisition of the (proverbial) cheese.
        int _trysInStack = 0;
        bool _tryConcurrent = false;
        List<string> _tryVariables = new List<string>();
        public override SyntaxNode VisitTryStatement(TryStatementSyntax node)
        {
            _trysInStack++;
            try
            {
                var newNode = (TryStatementSyntax)base.VisitTryStatement(node);

                if (_tryConcurrent)
                {
                    var variableName = "__try" + _trysInStack;
                    _tryVariables.Add(variableName);

                    //we can only split on expressions directly on the try level
                    //i.e. no try { if () { EXPRESSION }}
                    if (_trysInStack > 1 && !(newNode.Parent.Parent is TryStatementSyntax))
                    {
                        Debug.Assert(false); //td: error
                        return newNode;
                    }

                    var statements = new List<StatementSyntax>(newNode.Block.Statements);
                    var newStatements = new List<StatementSyntax>();
                    var currentIndex = 0;

                    while (currentIndex < statements.Count)
                    {
                        var oldIndex = currentIndex;
                        for (int i = oldIndex; i < statements.Count; i++, currentIndex++)
                        {
                            var statement = statements[i];

                            if (statement is YieldStatementSyntax)
                            {
                                newStatements.Add(newNode
                                    .WithBlock(CSharp.Block(
                                        statements
                                        .Skip(oldIndex)
                                        .Take(currentIndex - oldIndex - 1))));

                                //variable and return yield
                                //td: assert
                                newStatements.Add(statements[currentIndex - 1]);
                                newStatements.Add(statements[currentIndex++]);
                                break;
                            }

                            //must make variables available to later code, unless it precedes a yield
                            var yieldNext = statements.Count > i + 1 && statements[i + 1] is YieldStatementSyntax;
                            if (statement is LocalDeclarationStatementSyntax && !yieldNext)
                            {
                                var decl = statement as LocalDeclarationStatementSyntax;

                                var varType = decl.Declaration.Type;
                                if (varType == null || varType.Kind() == SyntaxKind.TypeVarKeyword)
                                    varType = Roslyn.SymbolTypeSyntax(_model, decl
                                        .Declaration
                                        .Variables[0]
                                        .Initializer
                                        .Value);

                                Debug.Assert(varType != null, "Untyped local variable on try fix");

                                var assignmentStatements = new List<StatementSyntax>();
                                newStatements.Add(decl
                                    .WithDeclaration(decl.Declaration
                                    .WithType(varType)
                                    .WithVariables(CSharp.SeparatedList(
                                        decl
                                        .Declaration
                                        .Variables
                                        .Select(v =>
                                        {
                                            assignmentStatements.Add(CSharp.ExpressionStatement(
                                                CSharp.AssignmentExpression(
                                                    SyntaxKind.SimpleAssignmentExpression,
                                                    CSharp.IdentifierName(v.Identifier),
                                                    v.Initializer.Value)));

                                            return v.WithInitializer(v
                                                .Initializer
                                                .WithValue(Templates
                                                    .DefaultValue
                                                    .Get<ExpressionSyntax>(varType)));
                                        })))));

                                //once moved the variables "up" scope
                                //we must keep the assignments
                                Debug.Assert(assignmentStatements.Any());
                                statements.RemoveAt(i);
                                statements.InsertRange(i, assignmentStatements);
                            }
                        }
                    }

                    Debug.Assert(newStatements.Any());
                    return LinkTryStatements(newStatements, variableName);
                }

                return newNode;
            }
            finally
            {
                _trysInStack--;
            }
        }

        private SyntaxNode LinkTryStatements(List<StatementSyntax> statements, string variable)
        {
            var newStatements = new List<StatementSyntax>();
            var firstTry = _trysInStack == 1;
            if (firstTry)
            {
                foreach (var v in _tryVariables)
                    newStatements.Add(Templates
                        .TryVariable
                        .Get<StatementSyntax>(v));

                _tryConcurrent = false;
                _tryVariables.Clear();
            }

            return CSharp.Block(
                newStatements.Union(
                CreateTrySplit(statements, 0, variable)));
        }

        private IEnumerable<StatementSyntax> CreateTrySplit(List<StatementSyntax> statements, int index, string variable)
        {
            bool foundTry = false;
            for (int i = index; i < statements.Count; i++, index++)
            {
                if (foundTry)
                {
                    yield return CSharp.IfStatement(
                        Templates.Negation.Get<ExpressionSyntax>(CSharp.IdentifierName(variable)),
                        CSharp.Block(CreateTrySplit(statements, index, variable)));

                    yield break;
                }

                var statement = statements[i];
                if (statement is TryStatementSyntax)
                {
                    foundTry = true;
                    var @try = statement as TryStatementSyntax;
                    yield return @try
                        .WithCatches(CSharp.List(
                            @try
                            .Catches
                            .Select(c => c.WithBlock(CSharp.Block(new [] {
                                Templates
                                .SetTryVariable
                                .Get<StatementSyntax>(variable)}.Union(
                                c.Block.Statements))))));
                }
                else
                    yield return statement;
            }
        }

        //concurrent expressions, must remember all assignents
        Dictionary<string, Dictionary<string, TypeSyntax>> _typeAssignments = new Dictionary<string, Dictionary<string, TypeSyntax>>();
        private IEnumerable<StatementSyntax> LinkExpressionStatement(StatementSyntax statement)
        {
            var yieldStatement = statement as YieldStatementSyntax;
            if (yieldStatement == null)
                yield return (StatementSyntax)Visit(statement);
            else
            {
                var expression = yieldStatement
                    .Expression
                    as ObjectCreationExpressionSyntax;

                if (expression == null)
                    yield return (StatementSyntax)Visit(statement);
                else
                {
                    //check for try/catch statements 
                    if (_trysInStack > 0)
                    {
                        _tryConcurrent = true;
                        Debug.Assert(statement.Parent.Parent is TryStatementSyntax); //td: error
                    }

                    var assignments = new Dictionary<string, TypeSyntax>();
                    var typeName = expression.Type.ToString();

                    expression = expression
                        .ReplaceNodes(expression
                            .DescendantNodes()
                            .OfType<ParenthesizedLambdaExpressionSyntax>(),
                            (on, nn) => LinkExpresion(nn, assignments));

                    if (assignments.Any())
                        _typeAssignments[typeName] = assignments;

                    //create a variable to hold the expression, then yield-return it 
                    //after completion, the results of the expression will be extracted from 
                    //this variable
                    var varName = typeName + "_var";
                    var exprVariable = Templates
                        .ExpressionVariable
                        .Get<StatementSyntax>(varName, expression);

                    yield return exprVariable;

                    yield return yieldStatement
                        .WithExpression(CSharp.IdentifierName(varName));

                    //we must check whether the expression failed
                    yield return Templates
                        .ExpressionFailedCheck
                        .Get<StatementSyntax>(varName);

                    //and finally, we must generate expression assignments 
                    //this way, any operand result will stay in the expression
                    //and only be used "after" the expression is complete.
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

            if (operand is LiteralExpressionSyntax)
            {
                var literal = operand as LiteralExpressionSyntax;
                switch (literal.Kind())
                {
                    case SyntaxKind.TrueLiteralExpression:
                    case SyntaxKind.FalseLiteralExpression:
                        return CSharp.ExpressionStatement(success
                            .ReplaceNodes(success
                            .DescendantNodes()
                            .OfType<LiteralExpressionSyntax>()
                            .Where(l => l.Kind() == SyntaxKind.TrueLiteralExpression
                                     || l.Kind() == SyntaxKind.FalseLiteralExpression),
                            (on, nn) => literal));
                }
            }

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
            {
                var bodyStatement = (successFunc.Body as StatementSyntax)
                    ?? CSharp.ExpressionStatement((ExpressionSyntax)successFunc.Body);

                return right.ReplaceNode(successFunc, successFunc
                    .WithBody(CSharp.Block(new[] {
                        Templates
                            .ExpressionAssigment
                            .Get<StatementSyntax>(assignment.Left, leftType) }
                        .Union(successFunc.Body is BlockSyntax
                            ? (successFunc.Body as BlockSyntax)
                                .Statements
                                .AsEnumerable()
                            : new[] { bodyStatement })
                        .ToArray())));
            }

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
            if (syntaxOperation(invocation, success, failure, out result))
                return result;
            else
            {
                var identifier = (invocation.Expression as IdentifierNameSyntax)
                    .ToString();
                Signal signal = _class.GetSignal(identifier);
                if (signal != null)
                {
                    var expr = invocation
                        .WithExpression(CSharp.IdentifierName("__concurrent" + identifier))
                        .WithArgumentList(invocation
                            .ArgumentList
                            .AddArguments(
                                CSharp.Argument(
                                    Templates.CancelationArgument),
                                CSharp.Argument(WrapInLambda(success)
                                    .AddParameterListParameters(CSharp.Parameter(CSharp.ParseToken(
                                        "__res")))),
                                CSharp.Argument(WrapInLambda(failure)
                                    .AddParameterListParameters(CSharp.Parameter(CSharp.ParseToken(
                                        "__ex"))))));

                    return CSharp.ExpressionStatement(Templates
                        .Advance
                        .Get<ExpressionSyntax>(expr));
                }

                return CSharp.ExpressionStatement(invocation);
            }
        }

        private StatementSyntax LinkExternalInvocation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax success, InvocationExpressionSyntax failure)
        {
            var queueStatement = null as StatementSyntax;
            if (_class.isQueueInvocation(invocation, true, success, out queueStatement))
                return queueStatement;

            Debug.Assert(invocation.Expression is MemberAccessExpressionSyntax);
            Debug.Assert(!invocation.Expression
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any()); //td: errors

            var symbol = _model.GetSymbolInfo(invocation.Expression).Symbol;
            if (symbol == null)
                return CSharp.ExpressionStatement(invocation);

            if (isConcurrent(symbol))
                return CSharp.ExpressionStatement(
                    invocation
                    .WithArgumentList(invocation
                        .ArgumentList
                        .AddArguments(
                            CSharp.Argument(Templates.CancelationArgument),
                            CSharp.Argument(Templates
                                .SuccessFunction
                                .Get<ExpressionSyntax>(success)),
                            CSharp.Argument(Templates
                                .FailureFunction
                                .Get<ExpressionSyntax>(failure)))));

            return CSharp.ExpressionStatement(invocation);
        }

        private bool isConcurrent(ISymbol symbol)
        {
            var type = symbol.ContainingType;
            return type != null && type.MemberNames.Contains("__concurrent" + symbol.Name);

        }

        private bool syntaxOperation(InvocationExpressionSyntax invocation, InvocationExpressionSyntax success, InvocationExpressionSyntax failure, out StatementSyntax result)
        {
            result = null;
            switch (invocation.Expression.ToString())
            {
                case "seconds":
                    result = Templates
                        .Seconds
                        .Get<StatementSyntax>(invocation.ArgumentList
                            .Arguments[0] //td: validate
                            .Expression,
                            success,
                            failure);
                    break;
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
