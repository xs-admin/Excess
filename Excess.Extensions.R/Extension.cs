using Antlr4.Runtime;
using Excess.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Extensions.R
{
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public static class RScope
    {
        public static void InitR(this Scope scope)
        {
            scope.set("rPreStatements", new List<StatementSyntax>());
            scope.set("rPostStatements", new List<StatementSyntax>());
        }

        public static List<StatementSyntax> PreStatements(this Scope scope)
        {
            var result = scope.find<List<StatementSyntax>>("rPreStatements");
            Debug.Assert(result != null);
            return result;
        }

        public static List<StatementSyntax> PostStatements(this Scope scope)
        {
            var result = scope.find<List<StatementSyntax>>("rPostStatements");
            Debug.Assert(result != null);
            return result;
        }
    }

    public class Extension
    {
        public void Apply(ICompiler<SyntaxToken, SyntaxNode, SemanticModel> compiler)
        {
            compiler.Lexical()
                .grammar<RGrammar, ParserRuleContext>("R", ExtensionKind.Code)
                    .transform<RParser.ProgContext>(Program)
                    .transform<RParser.AssignmentContext>(Assignment)
                    .transform<RParser.RightAssignmentContext>(RightAssignment)
                    .transform<RParser.SignContext>(Sign)
                    .transform<RParser.MultiplicationContext>(Multiplication)
                    .transform<RParser.AdditionContext>(Addition)
                    .transform<RParser.ComparisonContext>(Comparison)
                    .transform<RParser.NegationContext>(Negation)
                    .transform<RParser.LogicalAndContext>(LogicalAnd)
                    .transform<RParser.LogicalOrContext>(LogicalOr)
                    .transform<RParser.FunctionContext>(Function)
                    .transform<RParser.FunctionCallContext>(FunctionCall)
                    .transform<RParser.CompoundContext>(Compound)
                    .transform<RParser.IfStatementContext>(IfStatement)
                    .transform<RParser.IfElseStatementContext>(IfElseStatement)
                    .transform<RParser.ForElseStatementContext>(ForElseStatement)
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
                    .transform<RParser.FalseLiteralContext>(FalseLiteral);
        }

        private SyntaxNode FalseLiteral(RParser.FalseLiteralContext @false, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("false");
        }

        private SyntaxNode TrueLiteral(RParser.TrueLiteralContext @true, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("true");
        }

        private SyntaxNode NanLiteral(RParser.NanLiteralContext nan, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode InfLiteral(RParser.InfLiteralContext inf, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode NA(RParser.NAContext na, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("null"); //td: use cases
        }

        private SyntaxNode NullLiteral(RParser.NullLiteralContext @null, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression("null");
        }

        private SyntaxNode ComplexLiteral(RParser.ComplexLiteralContext complex, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode FloatLiteral(RParser.FloatLiteralContext @float, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(@float.FLOAT().ToString());
        }

        private SyntaxNode IntLiteral(RParser.IntLiteralContext @int, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(@int.INT().ToString());
        }

        private SyntaxNode HexLiteral(RParser.HexLiteralContext hex, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(hex.HEX().ToString());
        }

        private SyntaxNode StringLiteral(RParser.StringLiteralContext str, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(str.STRING().ToString());
        }

        private SyntaxNode Identifier(RParser.IdentifierContext identifier, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.ParseExpression(identifier.ID().ToString());
        }

        private SyntaxNode Parenthesized(RParser.ParenthesizedContext parenth, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode BreakStatement(RParser.BreakStatementContext breakStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return CSharp.BreakStatement();
        }

        private SyntaxNode RepeatStatement(RParser.RepeatStatementContext repeatStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode WhileStatement(RParser.WhileStatementContext whileStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode ForElseStatement(RParser.ForElseStatementContext forStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode IfElseStatement(RParser.IfElseStatementContext ifStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode IfStatement(RParser.IfStatementContext ifStatement, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode Compound(RParser.CompoundContext compound, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode FunctionCall(RParser.FunctionCallContext call, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            var exprNode = transform(call.expr(), parent, scope) as ExpressionSyntax;
            var args = transform(call.sublist(), parent, scope) as ArgumentListSyntax;

            if (exprNode is IdentifierNameSyntax)
                return createInvocation(exprNode.ToString(), args);

            return CSharp.InvocationExpression(exprNode, args);
        }

        private SyntaxNode createInvocation(string call, ArgumentListSyntax args)
        {
            switch (call)
            {
                case "c": return Concatenation(args);

            }
            throw new NotImplementedException();
        }

        private SyntaxNode Concatenation(ArgumentListSyntax args)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode Function(RParser.FunctionContext func, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode Sign(RParser.SignContext sign, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return unaryOperator(tokenKind(sign.GetToken(0, 0).ToString()), sign.expr(), transform, scope);
        }

        private SyntaxNode Negation(RParser.NegationContext neg, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return unaryOperator(SyntaxKind.ExclamationToken, neg.expr(), transform, scope);
        }

        private SyntaxNode Multiplication(RParser.MultiplicationContext mult, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return binaryOperator(tokenKind(mult.GetToken(0, 0).ToString()), mult.expr(0), mult.expr(1), transform, scope);
        }

        private SyntaxNode Addition(RParser.AdditionContext add, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return binaryOperator(tokenKind(add.GetToken(0, 0).ToString()), add.expr(0), add.expr(1), transform, scope);
        }

        private SyntaxNode Comparison(RParser.ComparisonContext comp, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return binaryOperator(tokenKind(comp.GetToken(0, 0).ToString()), comp.expr(0), comp.expr(1), transform, scope);
        }

        private SyntaxNode LogicalAnd(RParser.LogicalAndContext and, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return binaryOperator(tokenKind(and.GetToken(0, 0).ToString()), and.expr(0), and.expr(1), transform, scope);
        }

        private SyntaxNode LogicalOr(RParser.LogicalOrContext or, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            return binaryOperator(tokenKind(or.GetToken(0, 0).ToString()), or.expr(0), or.expr(1), transform, scope);
        }
        
        private SyntaxKind tokenKind(string v)
        {
            throw new NotImplementedException();
        }

        static ExpressionSyntax placeholder = CSharp.ParseExpression("_");
        private SyntaxNode binaryOperator(SyntaxKind kind, RParser.ExprContext left, RParser.ExprContext right, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            var parent = CSharp.BinaryExpression(kind, placeholder, placeholder);
            var leftNode = transform(left, parent, scope) as ExpressionSyntax;
            var rightNode = transform(right, parent, scope) as ExpressionSyntax;

            return parent
                .WithLeft(leftNode)
                .WithRight(rightNode);
        }

        private SyntaxNode unaryOperator(SyntaxKind kind, RParser.ExprContext value, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            var parent = CSharp.PrefixUnaryExpression(kind, placeholder);
            var valueNode = transform(value, parent, scope) as ExpressionSyntax;

            return parent
                .WithOperand(valueNode);
        }

        private SyntaxNode Assignment(RParser.AssignmentContext assignment, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            var left = transform(assignment.expr(), null, scope) as ExpressionSyntax;
            Debug.Assert(left != null);
            var rightNode = null as SyntaxNode;
            var result = null as SyntaxNode;

            if (parent == null)
            {
                result = createAssigment(left, out rightNode);
                parent = result;
            }
            else
            {
                var pre = scope.PreStatements();
                var post = scope.PostStatements();
                throw new NotImplementedException();
            }

            var right = transform(assignment.expr_or_assign(), parent, scope) as StatementSyntax;
            return result;
        }

        private SyntaxNode RightAssignment(RParser.RightAssignmentContext assignment, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode createAssigment(ExpressionSyntax left, out SyntaxNode rightNode)
        {
            throw new NotImplementedException();
        }

        private SyntaxNode Program(RParser.ProgContext prog, SyntaxNode parent, Func<ParserRuleContext, SyntaxNode, Scope, SyntaxNode> transform, Scope scope)
        {
            var statements = new List<StatementSyntax>();
            foreach (var expr in prog.expr_or_assign())
            {
                var inner = new Scope(scope);
                inner.InitR();

                var statement = transform(expr, null, inner) as StatementSyntax;
                Debug.Assert(statement != null);

                var pre  = inner.PreStatements();
                var post = inner.PostStatements();

                statements.AddRange(pre);
                statements.Add(statement);
                statements.AddRange(post);
            }

            return CSharp.Block(statements);
        }
    }
}
