using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    public static class AntlrExpression
    {
        public static SyntaxNode Parse(ParserRuleContext node, Func<ParserRuleContext, Scope, SyntaxNode> continuation, Scope scope)
        {
            return visitNode((ParserRuleContext)node); //td: scope needed?
        }

        static Dictionary<string, Func<ParserRuleContext, ExpressionSyntax>> _handlers = new Dictionary<string, Func<ParserRuleContext, ExpressionSyntax>>();
        static AntlrExpression()
        {
            _handlers["ExpressionContext"] = Expression;
            _handlers["AssignmentExpressionContext"] = Assignment;
            _handlers["UnaryExpressionContext"] = UnaryExpression;
            _handlers["LogicalAndExpressionContext"] = BinaryExpression;
            _handlers["AdditiveExpressionContext"] = BinaryExpression;
            _handlers["ConditionalExpressionContext"] = BinaryExpression;
            _handlers["LogicalOrExpressionContext"] = BinaryExpression;
            _handlers["InclusiveOrExpressionContext"] = BinaryExpression;
            _handlers["ExclusiveOrExpressionContext"] = BinaryExpression;
            _handlers["AndExpressionContext"] = BinaryExpression;
            _handlers["EqualityExpressionContext"] = BinaryExpression;
            _handlers["ConstantContext"] = Constant;
            _handlers["CastExpressionContext"] = Cast;
            _handlers["RelationalExpressionContext"] = BinaryExpression;
            _handlers["ShiftExpressionContext"] = BinaryExpression;
            _handlers["MultiplicativeExpressionContext"] = BinaryExpression;
            _handlers["MemberAccessContext"] = MemberAccess;
            _handlers["ParenthesizedContext"] = Parenthesized;
            _handlers["MemberPointerAccessContext"] = MemberAccess;
            _handlers["SimpleExpressionContext"] = Expression;
            _handlers["IndexContext"] = Index;
            _handlers["CallContext"] = Call;
            _handlers["PostfixIncrementContext"] = PostFix;
            _handlers["PostfixDecrementContext"] = PostFix;
            _handlers["IdentifierContext"] = Identifer;
            _handlers["StringLiteralContext"] = StringLiteral;
            _handlers["ArgumentExpressionListContext"] = Hidden;
            _handlers["UnaryOperatorContext"] = Hidden;
            _handlers["AssignmentOperatorContext"] = Hidden;
            _handlers["ConstantExpressionContext"] = Hidden;
        }

        private static ExpressionSyntax StringLiteral(ParserRuleContext arg)
        {
            return CSharp.ParseExpression(arg.GetText());
        }

        private static ExpressionSyntax Hidden(ParserRuleContext arg)
        {
            throw new InvalidOperationException("This node should not be processed directly");
        }

        private static ExpressionSyntax Identifer(ParserRuleContext arg)
        {
            return CSharp.IdentifierName(arg.GetText());
        }

        private static ExpressionSyntax Expression(ParserRuleContext node)
        {
            Debug.Assert(node.ChildCount == 1);
            return visitNode(node.GetRuleContext<ParserRuleContext>(0));
        }

        private static ExpressionSyntax Assignment(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 3);
            var left = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            var right = visitNode(node.GetRuleContext<ParserRuleContext>(2));

            SyntaxKind kind;
            var op = GetBinaryOperator(node.children[1].GetText(), out kind);

            return CSharp.AssignmentExpression(kind, left, op, right);
        }

        private static SyntaxToken GetBinaryOperator(string value, out SyntaxKind kind)
        {
            var result = CSharp.ParseToken(value);
            kind = result.Kind();
            switch (kind)
            {
                case SyntaxKind.PlusToken:
                    kind = SyntaxKind.AddExpression;
                    break;
                case SyntaxKind.MinusToken:
                    kind = SyntaxKind.SubtractExpression;
                    break;
                case SyntaxKind.AsteriskToken:
                    kind = SyntaxKind.MultiplyExpression;
                    break;
                case SyntaxKind.AsteriskEqualsToken:
                    kind = SyntaxKind.MultiplyAssignmentExpression;
                    break;
                case SyntaxKind.SlashToken:
                    kind = SyntaxKind.DivideExpression;
                    break;
                case SyntaxKind.SlashEqualsToken:
                    kind = SyntaxKind.DivideAssignmentExpression;
                    break;
                case SyntaxKind.GreaterThanToken:
                    kind = SyntaxKind.GreaterThanExpression;
                    break;
                case SyntaxKind.GreaterThanEqualsToken:
                    kind = SyntaxKind.GreaterThanOrEqualExpression;
                    break;
                case SyntaxKind.LessThanToken:
                    kind = SyntaxKind.LessThanExpression;
                    break;
                case SyntaxKind.LessThanEqualsToken:
                    kind = SyntaxKind.LessThanOrEqualExpression;
                    break;
                case SyntaxKind.EqualsEqualsToken:
                    kind = SyntaxKind.EqualsExpression;
                    break;
                case SyntaxKind.ExclamationEqualsToken:
                    kind = SyntaxKind.NotEqualsExpression;
                    break;
                case SyntaxKind.EqualsToken:
                    kind = SyntaxKind.SimpleAssignmentExpression;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        private static SyntaxToken GetUnaryOperator(string value, out SyntaxKind kind)
        {
            var result = CSharp.ParseToken(value);
            kind = result.Kind();
            switch (kind)
            {
                case SyntaxKind.EqualsToken:
                    kind = SyntaxKind.SimpleAssignmentExpression;
                    break;
            }

            return result;
        }

        private static ExpressionSyntax UnaryExpression(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 2);
            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(2));

            SyntaxKind kind;
            var op = GetUnaryOperator(node.children[0].GetText(), out kind);

            return CSharp.PrefixUnaryExpression(kind, op, expr);
        }

        private static ArgumentListSyntax Arguments(ParserRuleContext node)
        {
            var expr = null as ExpressionSyntax;
            var args = null as ArgumentListSyntax;
            if (node.ChildCount == 1)
                expr = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            else
            {
                Debug.Assert(node.ChildCount == 2);
                expr = visitNode(node.GetRuleContext<ParserRuleContext>(1));
                args = Arguments(node.GetRuleContext<ParserRuleContext>(0));
            }

            var arg = CSharp.Argument(expr);
            if (args != null)
                return args.AddArguments(arg);

            return CSharp
                .ArgumentList(CSharp
                .SeparatedList(new[] { arg }));
        }

        private static ExpressionSyntax Constant(ParserRuleContext node)
        {
            return CSharp.ParseExpression(node.GetText());
        }

        private static ExpressionSyntax Parenthesized(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 3);
            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(0));

            return CSharp.ParenthesizedExpression(expr);
        }

        private static ExpressionSyntax Cast(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 2);
            var type = CSharp.ParseTypeName(node.children[0].GetText());
            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(1));

            return CSharp.CastExpression(type, expr);
        }

        private static ExpressionSyntax BinaryExpression(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 3);
            var left = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            var right = visitNode(node.GetRuleContext<ParserRuleContext>(1));

            SyntaxKind kind;
            var op = GetBinaryOperator(node.children[1].GetText(), out kind);

            return CSharp.BinaryExpression(kind, left, op, right);
        }

        private static ExpressionSyntax MemberAccess(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 3);
            var left = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            var right = CSharp.IdentifierName(node.children[2].GetText());

            SyntaxKind kind;
            var op = GetBinaryOperator(node.children[1].GetText(), out kind);
            return CSharp.MemberAccessExpression(kind, left, op, right);
        }

        private static ExpressionSyntax Index(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 4);
            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            var index = (ArgumentListSyntax)Arguments(node.GetRuleContext<ParserRuleContext>(2));

            return CSharp.ElementAccessExpression(
                expr, CSharp
                .BracketedArgumentList(index.Arguments));
        }

        private static ExpressionSyntax Call(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(0));
            var args = null as ArgumentListSyntax;

            if (node.ChildCount == 4)
                args = Arguments(node.GetRuleContext<ParserRuleContext>(2));
            else
                args = CSharp.ArgumentList();

            return CSharp.InvocationExpression(expr, args);
        }

        private static ExpressionSyntax PostFix(ParserRuleContext node)
        {
            if (node.ChildCount == 1)
                return Expression(node.GetRuleContext<ParserRuleContext>(0));

            Debug.Assert(node.ChildCount == 2);
            var expr = visitNode(node.GetRuleContext<ParserRuleContext>(0));

            SyntaxKind kind;
            var op = GetBinaryOperator(node.children[1].GetText(), out kind);
            return CSharp.PostfixUnaryExpression(kind, expr, op);
        }

        private static ExpressionSyntax visitNode(ParserRuleContext node)
        {
            var typename = node.GetType().Name;
            return _handlers[typename](node);
        }
    }
}
