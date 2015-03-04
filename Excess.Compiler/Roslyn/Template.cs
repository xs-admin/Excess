using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excess.Compiler.Roslyn;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    public class Template
    {
        public static Template ParseExpression(string text)
        {
            var expr = CSharp.ParseExpression(text);
            return new Template(expr, GetValues(expr));
        }

        public static Template ParseStatement(string text)
        {
            var statement = CSharp.ParseStatement(text);
            return new Template(statement, GetValues(statement));
        }

        public static Template ParseStatements(string text)
        {
            var statements = CSharp.ParseStatement("{" + text + "}");
            return new Template(statements, GetValues(statements));
        }

        public static Template Parse(string text)
        {
            var parsed = CSharp.ParseCompilationUnit(text);
            return new Template(parsed, GetValues(parsed));
        }

        private static IDictionary<object, int> GetValues(SyntaxNode node)
        {
            var result = new Dictionary<object, int>();
            var tokens = node
                .DescendantTokens()
                .Where(token => matchValue(token, result));

            tokens.All(_ => true);
            return result;
        }

        private static bool matchValue(SyntaxToken token, Dictionary<object, int> values)
        {
            var name = token.ToString();

            int idx;
            if (name.StartsWith("__") && int.TryParse(name.Substring(2), out idx))
                values[token.Parent] = idx;
            else if (name.StartsWith("_") && int.TryParse(name.Substring(1), out idx))
                values[token] = idx;
            else
                return false;

            return true;
        }

        SyntaxNode _root;
        IDictionary<object, int> _values;

        private Template(SyntaxNode root, IDictionary<object, int> values)
        {
            _root = root;
            _values = values;
        }

        public SyntaxNode Get(params object[] values)
        {
            return instantiate(values);
        }

        public T Get<T>(params object[] values) where T : SyntaxNode
        {
            var result = instantiate(values);
            return result
                .DescendantNodesAndSelf()
                .OfType<T>()
                .First();
        }

        public T Value<T>() where T : SyntaxNode
        {
            return (T)_values.First().Key;
        }

        public IDictionary<object, int> Values()
        {
            return _values;
        }

        private SyntaxNode instantiate(params object[] values)
        {
            var result = _root;
            var nodes = _values.Keys.OfType<SyntaxNode>();
            var tokens = _values.Keys.OfType<SyntaxToken>();

            var hasNodes = nodes.Any();
            var hasTokens = tokens.Any();

            if (hasNodes && hasTokens)
                result = result.ReplaceNodes(nodes
                    .Union(tokens.Select(token => token.Parent)), (oldNode, newNode) => getMixedNode(oldNode, newNode, values));
            else if (hasNodes)
                result = result.ReplaceNodes(nodes, (oldNode, newNode) => getNode(oldNode, values));
            else if (hasTokens)
                result = result.ReplaceTokens(tokens, (oldToken, newToken) => getToken(oldToken, values));

            return result;
        }

        private SyntaxNode getMixedNode(SyntaxNode oldNode, SyntaxNode newNode, object[] values)
        {
            int idx;
            if (_values.TryGetValue(oldNode, out idx))
                return getNode(oldNode, values);

            var newTokens = newNode.DescendantTokens().GetEnumerator();
            foreach (var token in oldNode.DescendantTokens())
            {
                newTokens.MoveNext();
                if (_values.TryGetValue(token, out idx))
                {
                    var newToken = getToken(token, values);
                    return newNode.ReplaceToken(newTokens.Current, newToken);
                }
                var value = values[idx];
            }

            return newNode;
        }

        private SyntaxNode getNode(SyntaxNode node, object[] values)
        {
            int idx = _values[node];
            var value = values[idx];
            if (value is SyntaxNode)
                return (SyntaxNode)value;

            if (value is SyntaxToken)
                return ((SyntaxToken)value).Parent;

            if (value is bool)
            {
                if ((bool)value)
                    return Roslyn.@true;
                else
                    return Roslyn.@false;
            }

            return CSharp.ParseExpression(value.ToString());
        }

        private SyntaxToken getToken(SyntaxToken token, object[] values)
        {
            int idx = _values[token];
            var value = values[idx];

            if (value == null)
                return token;

            if (value is SyntaxToken)
                return (SyntaxToken)value;

            if (value is SyntaxNode)
                return (value as SyntaxNode).DescendantTokens().Single();

            if (value is bool)
            {
                if ((bool)value)
                    return CSharp.Token(SyntaxKind.TrueKeyword);
                else
                    return CSharp.Token(SyntaxKind.FalseKeyword);
            }

            return CSharp.ParseToken(value.ToString());
        }
    }
}
