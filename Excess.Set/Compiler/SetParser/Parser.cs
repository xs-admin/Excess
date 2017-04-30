using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler.SetParser
{
    using CSharp = SyntaxFactory;

    public class Parser
    {
        public static SetSyntax Parse(string text)
        {
            throw new NotImplementedException();
        }

        public static SetSyntax Parse(IEnumerable<SyntaxToken> tokens)
        {
            var tokenArray = tokens.ToArray();
            var index = 0;

            var variables = new List<VariableSyntax>();
            var consumed = 0;
            if (!parseVariables(tokenArray, index, out consumed, variables))
                return null; //td: errors

            index += consumed;

            var constructor = null as ConstructorSyntax;
            var options = new SetOptions();
            if (!parseConstructor(tokenArray, index, out consumed, out constructor, variables, options))
                return null; //td: errors

            return new SetSyntax(variables.ToArray(), constructor);
        }

        private static bool parseVariables(SyntaxToken[] tokens, int index, out int consumed, List<VariableSyntax> result)
        {
            consumed = 0;
            var startIndex = index;
            for (;;)
            {
                if (index >= tokens.Length)
                    return false; //td: error, unexpected en of file

                var iconsumed = 0;
                var token = tokens[index];
                var finished = false;
                if (!parseVariableSeparator(token, out iconsumed, out finished))
                    return false;

                index += iconsumed;
                if (finished)
                    break;

                var vs = null as VariableSyntax;
                if (!parseVariable(tokens, index, out iconsumed, out vs))
                    return false;

                index += iconsumed;
                result.Add(vs);
            }

            consumed = index - startIndex;
            return true;
        }

        private static bool parseVariableSeparator(SyntaxToken token, out int consumed, out bool finished)
        {
            consumed = 0;
            finished = false;
            switch ((SyntaxKind)token.RawKind)
            {
                case SyntaxKind.BarToken:
                    consumed = 1;
                    finished = true;
                    break;
                case SyntaxKind.CommaToken:
                    consumed = 1;
                    break;
                case SyntaxKind.IdentifierToken:
                    break;
                default:
                    return false; //td: error
            }

            return true;
        }

        private static bool parseVariable(SyntaxToken[] tokens, int index, out int consumed, out VariableSyntax variable)
        {
            consumed = 0;
            variable = null;
            var token = tokens[index];
            if (token.RawKind != (int)SyntaxKind.IdentifierToken)
                return false; //td: error, variable name expected

            //variable: name x, or xi for indexing, x as name for alias 
            string varName;
            bool isIndexed;
            string indexName;
            if (!parseVariableName(token, out varName, out isIndexed, out indexName))
                return false;

            consumed++;

            int iconsumed;
            string alias;
            SyntaxToken aliasToken;
            if (!parseAlias(tokens, index + consumed, out alias, out aliasToken, out iconsumed))
                return false;

            consumed += iconsumed;

            //type
            SyntaxToken typeToken;
            string typeName;
            if (!parseType(tokens, index + consumed, out typeToken, out typeName, out iconsumed))
                return false;

            consumed += iconsumed;

            //we golden
            variable = new VariableSyntax(varName, token, typeName, typeToken, alias, aliasToken, isIndexed, indexName, default(SyntaxToken));
            return true;
        }

        private static bool parseVariableName(SyntaxToken token, out string varName, out bool isIndexed, out string indexName)
        {
            varName = null;
            isIndexed = false;
            indexName = null;
            varName = token.ToString();
            if (varName.Length > 2)
                return false; //td: error

            if (!varName.All(c => char.IsLower(c)))
                return false; //td: error

            isIndexed = varName.Length == 2; //td: !!! multiple indices
            indexName = isIndexed? varName[1].ToString() : string.Empty;

            return true;
        }

        //false means "there was an error parsing" where as true can be "there is no alias"
        //with consumed == 0
        private static bool parseAlias(SyntaxToken[] tokens, int index, out string alias, out SyntaxToken token, out int consumed)
        {
            consumed = 0;
            alias = string.Empty;
            token = default(SyntaxToken);
            if (!tokens[index].IsKind(SyntaxKind.AsKeyword))
                return true;

            if (index + 1 >= tokens.Length)
                return false; //td: error, eof

            token = tokens[index + 1];
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return false; //td: error, identifier expected

            consumed = 2;
            alias = token.ToString();
            return true;
        }

        private static bool parseType(SyntaxToken[] tokens, int index, out SyntaxToken token, out string name, out int consumed)
        {
            consumed = 0;
            name = string.Empty;
            token = tokens[index];
            if (!token.IsKind(SyntaxKind.AsKeyword) && !token.IsKind(SyntaxKind.ColonToken))
                return true;

            if (index + 1 >= tokens.Length)
                return false; //td: error, eof

            token = tokens[index + 1];
            if (!token.IsKind(SyntaxKind.IdentifierToken))
                return false; //td: error, identifier expected

            consumed = 2;
            name = token.ToString();
            return true;
        }

        private static bool parseConstructor(SyntaxToken[] tokens, int index, out int consumed, out ConstructorSyntax constructor, List<VariableSyntax> variables, SetOptions options)
        {
            consumed = 0;
            constructor = null;

            var toCompile = new StringBuilder();
            var parenthesis = 0;
            var expressions = new List<ExpressionSyntax>();
            for (var i = index; i < tokens.Length; i++)
            {
                int iconsumed;
                if (toCompile.Length == 0)
                {
                    //at the beggining of a constructor item we give the parser
                    //chance to match custom syntax. Because custom === good.
                    ExpressionSyntax cexpr;
                    if (parseCustomConstructor(tokens, index = consumed, out iconsumed, out cexpr))
                    {
                        Debug.Assert(cexpr != null);

                        consumed += iconsumed;
                        i += iconsumed;
                        expressions.Add(cexpr);
                        continue;
                    }
                }

                consumed++;

                var token = tokens[i];
                var addToCompile = true;
                switch ((SyntaxKind)token.RawKind)
                {
                    case SyntaxKind.OpenParenToken:
                        parenthesis++;
                        break;
                    case SyntaxKind.CloseParenToken:
                        parenthesis--; //td: verify 
                        break;
                    case SyntaxKind.CommaToken:
                        if (parenthesis == 0)
                        {
                            addToCompile = false;
                            var expr = parseConstructorExpression(toCompile);
                            if (expr == null)
                                return false;

                            expressions.Add(expr);
                        }
                        break;
                    //for simplcity, we'll just switch the type of the type token  
                    //so it compiles as a expression. Note the operator we choose is illegal otherwise.
                    case SyntaxKind.ColonToken:
                    case SyntaxKind.InKeyword:
                        token = CSharp.Token(SyntaxKind.GreaterThanGreaterThanToken).WithTriviaFrom(token);
                        break;
                }

                if (addToCompile)
                    toCompile.Append(token.ToFullString());
            }

            if (toCompile.Length > 0)
            {
                var expr = parseConstructorExpression(toCompile);
                if (expr == null)
                    return false;

                expressions.Add(expr);
            }

            return buildConstructor(variables, expressions, options, out constructor);
        }

        private static ExpressionSyntax parseConstructorExpression(StringBuilder text)
        {
            var result = CSharp.ParseExpression(text.ToString());
            text.Clear();
            return result;
        }

        static readonly ExpressionSyntax _ellipsis = CSharp.ParseName("ellipsis");
        private static bool parseCustomConstructor(SyntaxToken[] tokens, int index, out int consumed, out ExpressionSyntax expr)
        {
            consumed = 0;
            expr = null;

            var token = tokens[index];
            switch (token.Kind())
            {
                case SyntaxKind.IdentifierToken:
                    switch (token.ToString())
                    {
                        case "when":
                            if (!parseWhenClause(tokens, index + 1, out consumed, out expr))
                                return false;
                            break;
                        case "otherwise":
                        case "else":
                            if (!parseWhenElseClause(tokens, index + 1, out consumed, out expr))
                                return false;
                            break;
                    }
                    break;

                //check ellipsis
                case SyntaxKind.DotToken:
                    if (index + 4 < tokens.Length
                        && tokens[index + 1].Kind() == SyntaxKind.DotToken
                        && tokens[index + 2].Kind() == SyntaxKind.DotToken)
                    {
                        if (tokens[index + 3].Kind() != SyntaxKind.CommaToken)
                            return false; //td: error, bad ellipsis

                        consumed = 4;
                        expr = _ellipsis;
                    }
                    break;
            }

            return true;
        }

        private static bool parseWhenClause(SyntaxToken[] tokens, int index, out int consumed, out ExpressionSyntax expr)
        {
            consumed = 0;
            expr = null;

            var cond = new StringBuilder();
            var value = null as StringBuilder;
            var parenthesis = 0;
            for (var i = index; i < tokens.Length; i++)
            {
                var token = tokens[i];
                var add = true;
                switch (token.Kind())
                {
                    case SyntaxKind.OpenParenToken:
                        parenthesis++;
                        break;
                    case SyntaxKind.CloseParenToken:
                        parenthesis--; //td: verify 
                        break;
                    case SyntaxKind.CommaToken:
                        if (parenthesis == 0)
                        {
                            if (value != null)
                            {
                                expr = CSharp.ParseExpression($"when({cond}, {value})");
                                return true;
                            }
                            else
                                return false; //td: error, expecting =>
                        }
                        break;
                    case SyntaxKind.EqualsGreaterThanToken:
                        if (value == null)
                        {
                            value = new StringBuilder();
                            add = false;
                        }
                        else
                            return false; //td: error, only one =>
                        break;
                }

                consumed++;
                if (add)
                {
                    if (value != null)
                        value.Append(token.ToFullString());
                    else 
                        cond.Append(token.ToFullString());
                }
            }

            return false; //td: error, unexpected end
        }

        private static bool parseWhenElseClause(SyntaxToken[] tokens, int index, out int consumed, out ExpressionSyntax expr)
        {
            consumed = 0;
            expr = null;

            var parenthesis = 0;
            var value = new StringBuilder();
            for (var i = index; i < tokens.Length; i++)
            {
                var token = tokens[i];
                switch (token.Kind())
                {
                    case SyntaxKind.OpenParenToken:
                        parenthesis++;
                        break;
                    case SyntaxKind.CloseParenToken:
                        parenthesis--; //td: verify 
                        break;
                    case SyntaxKind.CommaToken:
                        if (parenthesis == 0)
                        {
                            if (value != null)
                            {
                                expr = CSharp.ParseExpression($"otherwise({value})");
                                return true;
                            }
                            else
                                return false; //td: error, expecting =>
                        }
                        break;
                }
            }

            return false; //td: error, unexpected end
        }

        private static bool buildConstructor(List<VariableSyntax> variables, List<ExpressionSyntax> expressions, SetOptions options, out ConstructorSyntax constructor)
        {
            constructor = null;
            foreach (var expression in expressions)
            {
                if (applyToOptions(expression, options))
                    continue;

                if (applyToVariable(expression, variables))
                    continue;

                if (applyToMatch(expression, variables, constructor, out constructor))
                    continue;

                if (applyToNumericalInduction(expression, variables, constructor, out constructor))
                    continue;

                if (applyToGeneralInduction(expression, variables, constructor, out constructor))
                    continue;

                if (applyToPredicate(expression, variables, constructor, out constructor))
                    continue;

                return false;
            }

            return true;
        }

        private static bool applyToOptions(ExpressionSyntax expression, SetOptions options)
        {
            var identifier = expression as IdentifierNameSyntax;
            if (identifier != null)
            {
                switch (identifier.ToString())
                {
                    case "notempty":
                        options.NotEmpty = true;
                        break;
                }
            }

            return false;
        }

        private static bool applyToVariable(ExpressionSyntax expression, List<VariableSyntax> variables)
        {
            var binaryExpression = expression as BinaryExpressionSyntax;
            if (binaryExpression != null 
                && binaryExpression.OperatorToken.Kind() == SyntaxKind.GreaterThanGreaterThanEqualsToken)
            {
                var nameNode = binaryExpression.Left as IdentifierNameSyntax;
                var typeNode = binaryExpression.Right as IdentifierNameSyntax;

                if (nameNode == null || typeNode == null)
                    return false; //td: error?

                var name = nameNode.ToString();
                var type = typeNode.ToString();

                for (int i = 0; i < variables.Count; i++)
                {
                    var variable = variables[i];
                    if (variable.Name == name)
                    {
                        variables[i] = variable.WithType(nameNode.Identifier);
                        return true;
                    }

                    if (variable.IndexName == name)
                    {
                        variables[i] = variable.WithIndexType(nameNode.Identifier);
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool applyToMatch(ExpressionSyntax expression, List<VariableSyntax> variables, ConstructorSyntax current, out ConstructorSyntax constructor)
        {
            constructor = current;
            if (expression is InvocationExpressionSyntax)
            {
                var invocation = expression as InvocationExpressionSyntax;
                var isWhen = false;
                var isElse = false;

                switch (invocation.Expression.ToString())
                {
                    case "when":
                        isWhen = true;
                        break;
                    case "otherwise":
                        isElse = true;
                        break;
                }

                var matchConstructor = constructor as MatchConstructorSyntax;
                if (isWhen || isElse)
                {
                    if (constructor != null && matchConstructor == null)
                        return false; //td: error, another type of constructor in progress

                    if (matchConstructor == null)
                        matchConstructor = new MatchConstructorSyntax();
                }
                else
                    return false;

                if (isWhen)
                {
                    Debug.Assert(invocation.ArgumentList.Arguments.Count == 2);
                    var cond = invocation.ArgumentList.Arguments.First().Expression;
                    var value = invocation.ArgumentList.Arguments.Skip(1).First().Expression;
                    matchConstructor.CondValue.Add(new Tuple<ExpressionSyntax, ExpressionSyntax>(cond, value));
                }
                else
                {
                    Debug.Assert(invocation.ArgumentList.Arguments.Count == 1);
                    var value = invocation.ArgumentList.Arguments.First().Expression;
                    matchConstructor.Otherwise = value;
                }

                constructor = matchConstructor;
                return true;
            }

            return false;
        }

        private static bool applyToPredicate(ExpressionSyntax expression, List<VariableSyntax> variables, ConstructorSyntax current, out ConstructorSyntax constructor)
        {
            constructor = current;

            var binaryExpression = expression as BinaryExpressionSyntax;
            if (binaryExpression != null && isBooleanOperator(binaryExpression.OperatorToken))
            {
                var predicateConstructor = constructor as PredicateConstructorSyntax;
                if (constructor != null && predicateConstructor == null)
                    return false; //td: error, another type of constructor in progress

                if (predicateConstructor == null)
                    predicateConstructor = new PredicateConstructorSyntax();

                predicateConstructor.Expressions.Add(expression);

                constructor = predicateConstructor;
                return true;
            }

            return false;
        }

        private static bool isBooleanOperator(SyntaxToken token)
        {
            var kind = token.Kind();
            return kind == SyntaxKind.GreaterThanToken
                || kind == SyntaxKind.GreaterThanEqualsToken
                || kind == SyntaxKind.LessThanToken
                || kind == SyntaxKind.LessThanEqualsToken
                || kind == SyntaxKind.AmpersandToken
                || kind == SyntaxKind.AmpersandAmpersandToken
                || kind == SyntaxKind.BarToken
                || kind == SyntaxKind.BarBarToken
                || kind == SyntaxKind.EqualsToken
                || kind == SyntaxKind.EqualsEqualsToken
                ;
        }

        private static bool applyToGeneralInduction(ExpressionSyntax expression, List<VariableSyntax> variables, ConstructorSyntax current, out ConstructorSyntax constructor)
        {
            constructor = current;

            var binaryExpression = expression as BinaryExpressionSyntax;
            if (binaryExpression != null 
                && binaryExpression.OperatorToken.Kind() == SyntaxKind.SlashToken)
            {
                var inductionConstructor = constructor as InductionConstructorSyntax;
                if (constructor != null && inductionConstructor == null)
                    return false; //td: error, another type of constructor in progress

                if (inductionConstructor == null)
                    inductionConstructor = new InductionConstructorSyntax();

                inductionConstructor.Rules.Add(expression);

                constructor = inductionConstructor;
                return true;
            }

            return false;
        }

        private static bool applyToNumericalInduction(ExpressionSyntax expression, List<VariableSyntax> variables, ConstructorSyntax current, out ConstructorSyntax constructor)
        {
            constructor = current;

            var assignmentExpression = expression as AssignmentExpressionSyntax;
            if (assignmentExpression != null)
            {
                throw new NotImplementedException();
                //td: verify left is x[0], etc...
                //if (assignmentExpression.Left is Inde)
                //{
                //}

                //var inductionConstructor = constructor as NumericalInductionConstructorSyntax;
                //if (constructor != null && inductionConstructor == null)
                //    return false; //td: error, another type of constructor in progress

                //if (inductionConstructor == null)
                //    inductionConstructor = new NumericalInductionConstructorSyntax();

                //inductionConstructor.Values.Add(expression);

                //constructor = inductionConstructor;
                //return true;
            }

            return false;
        }
    }
}
