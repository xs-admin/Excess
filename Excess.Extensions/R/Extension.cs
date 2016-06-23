using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Antlr4.Runtime;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Antlr4;

using R;
using R.Grammar;

namespace Excess.Extensions.R
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RExtension
    {
        class RGrammar : AntlrGrammar
        {
            protected override ITokenSource GetLexer(AntlrInputStream stream) => new RLexer(stream);
            protected override Parser GetParser(ITokenStream tokenStream) => new RParser(tokenStream);
            protected override ParserRuleContext GetRoot(Parser parser) => ((RParser)parser).prog();
        }

        public static void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
        {
            compiler.Environment()
                .dependency<IVector>("Excess.Extensions.R");

            compiler.Lexical()
                .grammar<RGrammar, ParserRuleContext>("R", ExtensionKind.Code)
                    .transform<RParser.ProgContext>(Program)
                    .transform<RParser.Expr_or_assignContext>(EitherOr)
                    .transform<RParser.ExpressionStatementContext>(ExpressionStatement)
                    .transform<RParser.AssignmentContext>(AssignmentStatement)
                    .transform<RParser.RightAssignmentContext>(RightAssignment)
                    .transform<RParser.SignContext>(UnaryOperator)
                    .transform<RParser.NegationContext>(UnaryOperator)
                    .transform<RParser.MultiplicationContext>(BinaryOperator)
                    .transform<RParser.AdditionContext>(BinaryOperator)
                    .transform<RParser.ComparisonContext>(BinaryOperator)
                    .transform<RParser.LogicalAndContext>(BinaryOperator)
                    .transform<RParser.LogicalOrContext>(BinaryOperator)
                    .transform<RParser.FunctionContext>(Function)
                    .transform<RParser.FunctionCallContext>(FunctionCall)
                    .transform<RParser.CompoundContext>(Compound)
                    .transform<RParser.IfStatementContext>(IfStatement)
                    .transform<RParser.IfElseStatementContext>(IfElseStatement)
                    .transform<RParser.ForEachStatementContext>(ForEachStatement)
                    .transform<RParser.WhileStatementContext>(WhileStatement)
                    .transform<RParser.RepeatStatementContext>(RepeatStatement)
                    .transform<RParser.BreakStatementContext>(BreakStatement)
                    .transform<RParser.ParenthesizedContext>(Parenthesized)
                    .transform<RParser.IdentifierContext>(Identifier)
                    .transform<RParser.StringLiteralContext>(StringLiteral)
                    .transform<RParser.HexLiteralContext>(HexLiteral)
                    .transform<RParser.IntLiteralContext>(IntLiteral)
                    .transform<RParser.FloatLiteralContext>(FloatLiteral)
                    .transform<RParser.ComplexLiteralContext>(ComplexLiteral)
                    .transform<RParser.NullLiteralContext>(NullLiteral)
                    .transform<RParser.NAContext>(NA)
                    .transform<RParser.InfLiteralContext>(InfLiteral)
                    .transform<RParser.NanLiteralContext>(NanLiteral)
                    .transform<RParser.TrueLiteralContext>(TrueLiteral)
                    .transform<RParser.FalseLiteralContext>(FalseLiteral)
                    .transform<RParser.SublistContext>(ArgumentList)
                    .transform<RParser.IndexContext>(Index)
                    .transform<RParser.SequenceContext>(LinearSequence)
                    
                    .then(Transform)
            ;
        }

        private static SyntaxNode Transform(SyntaxNode oldNode, SyntaxNode newNode, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            Debug.Assert(newNode is BlockSyntax);
            var isAssignment = oldNode is LocalDeclarationStatementSyntax;
            if (!isAssignment && oldNode is BinaryExpressionSyntax)
            {
                var expr = oldNode as BinaryExpressionSyntax;
                isAssignment = expr.Kind() == SyntaxKind.SimpleAssignmentExpression;
            }

            if (isAssignment)
            {
                scope.AddError("r01", "R does not return", oldNode);
                return newNode;
            }

            var document = scope.GetDocument<SyntaxToken, SyntaxNode, SemanticModel>();
            document.change(oldNode.Parent, RoslynCompiler.ExplodeBlock(newNode));

            return newNode;
        }

        static Template binaryOperatorCall = Template.ParseExpression("__0(__1, __2)");
        private static SyntaxNode BinaryOperator(RParser.ExprContext expr, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var left = transform(expr.GetRuleContext<RParser.ExprContext>(0), scope) as ExpressionSyntax;
            var right = transform(expr.GetRuleContext<RParser.ExprContext>(1), scope) as ExpressionSyntax;
            Debug.Assert(left != null && right != null);

            Debug.Assert(expr.children.Count == 3);
            var op = expr.children[1].GetText();

            return binaryOperatorCall.Get(_binaryOperators[op], left, right);
        }

        static Template unaryOperatorCall = Template.ParseExpression("__0(__1)");
        private static SyntaxNode UnaryOperator(RParser.ExprContext expr, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            Debug.Assert(expr.children.Count == 2);
            var op = expr.children[0].GetText();

            var value = transform(expr.GetRuleContext<RParser.ExprContext>(0), scope) as ExpressionSyntax;
            if (isConstant(value))
                return CSharp.ParseExpression(op + value.ToString());

            Debug.Assert(value != null);

            return unaryOperatorCall.Get(_unaryOperators[op], value);
        }

        private static bool isConstant(ExpressionSyntax value)
        {
            switch ((SyntaxKind)value.RawKind)
            {
                case SyntaxKind.NumericLiteralExpression:
                case SyntaxKind.TrueLiteralExpression:
                case SyntaxKind.FalseLiteralExpression:
                    return true;
            }

            return false;
        }

        static Dictionary<string, ExpressionSyntax> _binaryOperators = new Dictionary<string, ExpressionSyntax>();
        static Dictionary<string, ExpressionSyntax> _unaryOperators = new Dictionary<string, ExpressionSyntax>();
        
        static RExtension()
        {
            _binaryOperators["+"] = CSharp.ParseExpression("RR.add");
            _binaryOperators["-"] = CSharp.ParseExpression("RR.sub");
            _binaryOperators["*"] = CSharp.ParseExpression("RR.mul");
            _binaryOperators["/"] = CSharp.ParseExpression("RR.div");
            _binaryOperators[">"] = CSharp.ParseExpression("RR.gt");
            _binaryOperators[">="] = CSharp.ParseExpression("RR.ge");
            _binaryOperators["<"] = CSharp.ParseExpression("RR.lt");
            _binaryOperators["<="] = CSharp.ParseExpression("RR.le");
            _binaryOperators["=="] = CSharp.ParseExpression("RR.eq");
            _binaryOperators["!="] = CSharp.ParseExpression("RR.neq");
            _binaryOperators["&&"] = CSharp.ParseExpression("RR.and");
            _binaryOperators["&"] = CSharp.ParseExpression("RR.bnd");
            _binaryOperators["||"] = CSharp.ParseExpression("RR.or");
            _binaryOperators["|"] = CSharp.ParseExpression("RR.bor");

            _unaryOperators["+"] = CSharp.ParseExpression("RR.ps");
            _unaryOperators["-"] = CSharp.ParseExpression("RR.ns");
            _unaryOperators["!"] = CSharp.ParseExpression("RR.neg");
        }

        static Template linearSequenceCall = Template.ParseExpression("RR.lseq(__0, __1)");
        private static SyntaxNode LinearSequence(RParser.SequenceContext sequence, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var exprs = sequence.expr();
            Debug.Assert(exprs.Length == 2);


            var left = transform(exprs[0], scope) as ExpressionSyntax;
            var right = transform(exprs[1], scope) as ExpressionSyntax;

            return linearSequenceCall.Get(left, right);
        }

        static Template indexCall = Template.ParseExpression("RR.index(__0, __1)");
        private static SyntaxNode Index(RParser.IndexContext index, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var indexExprs = index.sublist().sub();
            if (indexExprs.Length != 1)
            {
                //td: error
                return null;
            }

            var expr = transform(index.expr(), scope) as ExpressionSyntax;
            var args = transform(index.sublist(), scope) as ArgumentListSyntax;
            Debug.Assert(expr != null && args != null);

            var indexExpr = args.Arguments[0].Expression;
            Debug.Assert(indexExpr != null);

            return indexCall.Get(expr, indexExpr);
        }

        private static SyntaxNode ArgumentList(RParser.SublistContext args, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var nodes = new List<ArgumentSyntax>();
            foreach (var arg in args.sub())
            {
                var argName = null as string;
                var value = null as ExpressionSyntax;
                if (arg is RParser.SubExpressionContext)
                    value = (ExpressionSyntax)transform((arg as RParser.SubExpressionContext).expr(), scope);
                else if (arg is RParser.SubAssignmentContext)
                {
                    var paramName = (arg as RParser.SubAssignmentContext);
                    argName = paramName.ID().ToString();
                    value = (ExpressionSyntax)transform(paramName.expr(), scope);
                }
                else if (arg is RParser.SubStringAssignmentContext)
                {
                    var paramName = (arg as RParser.SubStringAssignmentContext);
                    argName = paramName.STRING().ToString();
                    value = (ExpressionSyntax)transform(paramName.expr(), scope);
                }
                else if (arg is RParser.SubIncompleteNullContext || arg is RParser.SubNullAssignmentContext)
                {
                    throw new NotImplementedException();
                }
                else if (arg is RParser.SubEllipsisContext)
                {
                    throw new NotImplementedException();
                }
                else if (arg is RParser.SubIncompleteAssignmentContext || arg is RParser.SubIncompleteStringContext || arg is RParser.SubIncompleteStringContext)
                {
                    throw new NotImplementedException();
                }
                else if (arg is RParser.SubEmptyContext)
                {
                    continue;
                }
                else
                    Debug.Assert(false);

                Debug.Assert(value != null);
                var result = CSharp.Argument(value);
                if (argName != null)
                    result = result
                        .WithNameColon(CSharp
                            .NameColon(argName));

                nodes.Add(result);
            }

            return CSharp
                .ArgumentList()
                .WithArguments(CSharp
                    .SeparatedList(nodes));
        }

        private static SyntaxNode FalseLiteral(RParser.FalseLiteralContext @false, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("false");
        }

        private static SyntaxNode TrueLiteral(RParser.TrueLiteralContext @true, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("true");
        }

        static ExpressionSyntax Nan = CSharp.ParseExpression("Double.NaN");
        private static SyntaxNode NanLiteral(RParser.NanLiteralContext ctx, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return Nan;
        }

        static ExpressionSyntax Inf = CSharp.ParseExpression("Double.PositiveInfinity");
        private static SyntaxNode InfLiteral(RParser.InfLiteralContext inf, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return Inf;
        }

        private static SyntaxNode NA(RParser.NAContext na, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("null"); //td: use cases
        }

        private static SyntaxNode NullLiteral(RParser.NullLiteralContext @null, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("null");
        }

        private static SyntaxNode ComplexLiteral(RParser.ComplexLiteralContext complex, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException(); //td: find a complex library
        }

        private static SyntaxNode FloatLiteral(RParser.FloatLiteralContext @float, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(@float.FLOAT().ToString());
        }

        private static SyntaxNode IntLiteral(RParser.IntLiteralContext @int, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(@int.INT().ToString());
        }

        private static SyntaxNode HexLiteral(RParser.HexLiteralContext hex, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(hex.HEX().ToString());
        }

        private static SyntaxNode StringLiteral(RParser.StringLiteralContext str, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(str.STRING().ToString());
        }

        private static SyntaxNode Identifier(RParser.IdentifierContext identifier, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var str = identifier
                .ID()
                .ToString()
                .Replace('.', '_');

            return CSharp.ParseExpression(str);
        }

        private static SyntaxNode Parenthesized(RParser.ParenthesizedContext parenth, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var expr = transform(parenth.expr(), scope) as ExpressionSyntax;
            Debug.Assert(expr != null);

            return CSharp.ParenthesizedExpression(expr);
        }

        private static SyntaxNode BreakStatement(RParser.BreakStatementContext breakStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.BreakStatement();
        }

        static WhileStatementSyntax repeat = (WhileStatementSyntax)CSharp.ParseStatement("while(true) {}");
        private static SyntaxNode RepeatStatement(RParser.RepeatStatementContext repeatStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (!topLevel(repeatStatement))
                throw new NotImplementedException();

            BlockSyntax body = parseBlock(repeatStatement.expr(), transform, scope);
            return repeat
                .WithStatement(body);
        }

        private static BlockSyntax parseBlock(RParser.ExprContext expr, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var node = transform(expr, scope);
            if (expr is RParser.CompoundContext)
                return (BlockSyntax)node;

            if (node is ExpressionSyntax)
                node = CSharp.ExpressionStatement(node as ExpressionSyntax);

            return CSharp
                .Block()
                .WithStatements(CSharp.List(new StatementSyntax[] {
                    (StatementSyntax)node}));
        }

        static Template @while = Template.ParseStatement("while(__0) {}");
        private static SyntaxNode WhileStatement(RParser.WhileStatementContext whileStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (!topLevel(whileStatement))
                throw new NotImplementedException();

            var exprs = whileStatement.expr();
            var cond = (ExpressionSyntax)transform(exprs[0], scope);
            var body = parseBlock(exprs[1], transform, scope);

            return @while.Get<WhileStatementSyntax>(cond)
                .WithStatement(body);
        }

        private static bool topLevel(RuleContext ctx)
        {
            return ctx.Parent is RParser.ProgContext 
                || ctx.Parent is RParser.CompoundContext
                || ctx.Parent is RParser.ExpressionStatementContext
                ;
        }

        static Template @foreach = Template.ParseStatement("foreach(var _0 in __1) {}");
        private static SyntaxNode ForEachStatement(RParser.ForEachStatementContext forStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (!topLevel(forStatement))
                throw new NotImplementedException();

            var exprs = forStatement.expr();
            var array = (ExpressionSyntax)transform(exprs[0], scope);
            var body = parseBlock(exprs[1], transform, scope);

            return @foreach.Get<ForEachStatementSyntax>(forStatement.ID().ToString(), array)
                .WithStatement(body);
        }

        static Template @ifelse = Template.ParseStatement("if(__0) {} else {}");
        private static SyntaxNode IfElseStatement(RParser.IfElseStatementContext ifStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (!topLevel(ifStatement))
                throw new NotImplementedException();

            var exprs = ifStatement.expr();
            var cond = (ExpressionSyntax)transform(exprs[0], scope);
            var @if = parseBlock(exprs[1], transform, scope);
            var @else = parseBlock(exprs[2], transform, scope);

            return @ifelse.Get<IfStatementSyntax>(cond)
                .WithStatement(@if)
                .WithElse(CSharp.ElseClause(
                    @else));
        }

        static Template @if = Template.ParseStatement("if(__0) {}");
        private static SyntaxNode IfStatement(RParser.IfStatementContext ifStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (!topLevel(ifStatement))
                throw new NotImplementedException();

            var exprs = ifStatement.expr();
            var cond = (ExpressionSyntax)transform(exprs[0], scope);
            var code = parseBlock(exprs[1], transform, scope);

            return @if.Get<IfStatementSyntax>(cond)
                .WithStatement(code);
        }

        private static SyntaxNode Compound(RParser.CompoundContext compound, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            //create a new context for the block
            var inner = new Scope(scope);
            inner.InitR();

            var pre = inner.PreStatements();
            var statements = new List<StatementSyntax>();
            var exprlist = compound.exprlist();
            if (exprlist is RParser.ExpressionListContext)
            {
                var list = exprlist as RParser.ExpressionListContext;
                foreach (var expr in list.expr_or_assign())
                {
                    pre.Clear();
                    var statement = transform(expr, inner) as StatementSyntax;
                    Debug.Assert(statement != null);


                    statements.AddRange(pre);
                    statements.Add(statement);
                }
            }

            return CSharp.Block(statements);
        }

        private static SyntaxNode FunctionCall(RParser.FunctionCallContext call, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var expr = transform(call.expr(), scope) as ExpressionSyntax;
            var args = transform(call.sublist(), scope) as ArgumentListSyntax;

            Debug.Assert(expr != null && args != null);
            if (expr is IdentifierNameSyntax)
                return createInvocation(expr.ToString(), args);

            throw new NotImplementedException();
        }

        private static SyntaxNode createInvocation(string call, ArgumentListSyntax args)
        {
            //td: use cases
            //switch (call)
            //{
            //    case "c":
            //        call = "RR.c";
            //        break;
            //}

            return CSharp.InvocationExpression(CSharp.ParseExpression("RR." + call), args);
        }

        private static SyntaxNode Function(RParser.FunctionContext func, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private static SyntaxNode ExpressionStatement(RParser.ExpressionStatementContext exprStatement, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var expr = transform(exprStatement.expr(), scope);
            Debug.Assert(expr != null);

            if (expr is ExpressionSyntax)
                return CSharp.ExpressionStatement(expr as ExpressionSyntax);

            return (StatementSyntax)expr;
        }

        private static SyntaxNode AssignmentStatement(RParser.AssignmentContext assignment, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var left = transform(assignment.expr(), scope) as ExpressionSyntax;
            var right = transform(assignment.expr_or_assign(), scope) as ExpressionStatementSyntax;

            Debug.Assert(left != null && right != null);
            return assigmentStatement(left, right.Expression, scope);
        }

        static Template preVariable = Template.ParseStatement("object _0 = null;");
        static Template preAssignment = Template.ParseExpression("(_0 = __1)");
        private static SyntaxNode RightAssignment(RParser.RightAssignmentContext assignment, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var left = transform(assignment.expr()[1], scope) as ExpressionSyntax;
            var right = transform(assignment.expr()[0], scope) as ExpressionSyntax;
            Debug.Assert(left != null && right != null);

            if (topLevel(assignment))
                return assigmentStatement(left, right, scope);

            Debug.Assert(left is IdentifierNameSyntax);
            var leftValue = left.ToString();
            if (!scope.hasVariable(leftValue))
            {
                scope.PreStatements().Add(preVariable.Get<StatementSyntax>(left));
                scope.addVariable(leftValue);
            }

            return preAssignment.Get(left, right);
        }

        static Template assignment = Template.ParseStatement("__0 = __1;");
        static Template declaration = Template.ParseStatement("var _0 = __1;");
        private static StatementSyntax assigmentStatement(ExpressionSyntax left, ExpressionSyntax right, Scope scope)
        {
            if (left is IdentifierNameSyntax)
            {
                var varName = left.ToString();
                if (scope.hasVariable(varName))
                    return assignment.Get<StatementSyntax>(left, right);
                else
                {
                    scope.addVariable(varName);
                    return declaration.Get<StatementSyntax>(varName, right);
                }
            }

            throw new NotImplementedException();
        }

        private static SyntaxNode EitherOr(RParser.Expr_or_assignContext either, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            if (either is RParser.AssignmentContext)
                return AssignmentStatement(either as RParser.AssignmentContext, transform, scope);

            Debug.Assert(either is RParser.ExpressionStatementContext);
            return ExpressionStatement(either as RParser.ExpressionStatementContext, transform, scope);
        }

        private static SyntaxNode Program(RParser.ProgContext prog, Func<ParserRuleContext, Scope, SyntaxNode> transform, Scope scope)
        {
            var statements = new List<StatementSyntax>();
            var inner = new Scope(scope);
            inner.InitR();
            var pre = inner.PreStatements();

            foreach (var expr in prog.expr_or_assign())
            {
                pre.Clear();

                var statement = transform(expr, inner) as StatementSyntax;
                Debug.Assert(statement != null);

                statements.AddRange(pre);
                statements.Add(statement);
            }

            return CSharp.Block(statements);
        }
    }
}
