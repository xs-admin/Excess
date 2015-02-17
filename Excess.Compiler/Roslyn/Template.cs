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
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
    using Roslyn = RoslynCompiler;

    public class Template
    {
        public static Template ParseExpression(string text)
        {
            var expr = CSharp.ParseExpression(text);
            return new Template(expr, GetValues(expr));
        }

        public static Template ParseExpression<T>(string text, Func<T, bool> comparer = null) where T : SyntaxNode
        {
            var expr = CSharp.ParseExpression(text);
            return new Template(expr, expr
                .DescendantNodes()
                .OfType<T>()
                .Where(node => comparer == null ? true : comparer(node)));
        }

        public static Template ParseStatement(string text)
        {
            var statement = CSharp.ParseStatement(text);
            return new Template(statement, GetValues(statement));
        }

        public static Template ParseStatement<T>(string text, Func<T, bool> comparer = null) where T : SyntaxNode
        {
            var statement = CSharp.ParseStatement(text);
            return new Template(statement, statement
                .DescendantNodes()
                .OfType<T>()
                .Where(node => comparer == null ? true : comparer(node)));
        }

        public static Template ParseStatements(string text)
        {
            var statements = CSharp.ParseStatement("{" + text + "}");
            return new Template(statements, GetValues(statements));
        }

        public static Template ParseStatements<T>(string text, Func<T, bool> comparer = null) where T : SyntaxNode
        {
            var statements = CSharp.ParseStatement("{" + text + "}");
            return new Template(statements, statements
                .DescendantNodes()
                .OfType<T>()
                .Where(node => comparer == null ? true : comparer(node)));
        }

        public static Template Parse(string text)
        {
            var parsed = CSharp.ParseCompilationUnit(text);
            return new Template(parsed, GetValues(parsed));
        }

        public static Template Parse<T>(string text, Func<T, bool> comparer = null) where T : SyntaxNode
        {
            var parsed = CSharp.ParseCompilationUnit(text);

            return new Template(parsed, parsed
                .DescendantNodes()
                .OfType<T>()
                .Where(node => comparer == null? true : comparer(node)));
        }

        private static IEnumerable<SyntaxNode> GetValues(SyntaxNode node)
        {
            var named = node
                .DescendantNodesAndSelf()
                .Where(innerNode => matchValue(innerNode));

            var values = new Dictionary<int, SyntaxNode>();
            foreach (var value in named)
            {
                int idx = int.Parse(value.ToString().Substring(2));

                SyntaxNode existing;
                bool add = true;
                if (values.TryGetValue(idx, out existing))
                {
                    add = existing
                        .Ancestors()
                        .Where(ancestor => ancestor == value)
                        .Any();
                }
                
                if (add)
                    values[idx] = value;
            }

            return values
                .OrderBy(value => value.Key)
                .Select(value => value.Value);
        }

        private static bool matchValue(SyntaxNode node)
        {
            var name = node.ToString();
            int idx;
            return name.StartsWith("__") && int.TryParse(name.Substring(2), out idx);
        }

        private Template(SyntaxNode root, IEnumerable<SyntaxNode> values)
        {
            _root = root;
            _values = values;
        }

        public SyntaxNode Get(params SyntaxNode[] values)
        {
            return instantiate(values);
        }

        public SyntaxNode Get(params object[] values)
        {
            return instantiate(parseValues(values));
        }

        private SyntaxNode[] parseValues(object[] values)
        {
            SyntaxNode[] result = new SyntaxNode[values.Length];
            int current = 0;
            foreach (var obj in values)
            {
                SyntaxNode node;
                if (obj is SyntaxNode)
                    node = obj as SyntaxNode;
                else
                    node = Roslyn.Constant(obj);

                result[current++] = node;
            }

            return result;
        }

        public T Get<T>(params SyntaxNode[] values) where T : SyntaxNode
        {
            return (T)instantiate(values);
        }

        public T Get<T>(params object[] values) where T : SyntaxNode
        {
            return (T)instantiate(parseValues(values));
        }

        public T Value<T>() where T : SyntaxNode
        {
            return (T)_values.First();
        }

        public IEnumerable<SyntaxNode> Values()
        {
            return _values;
        }

        SyntaxNode _root;
        IEnumerable<SyntaxNode> _values;
        private SyntaxNode instantiate(params SyntaxNode[] values)
        {
            return _root.ReplaceNodes(_values, (oldNode, newNode) =>
            {
                int idx = -1;
                foreach (var oldValue in _values)
                {
                    idx++;
                    if (oldNode == oldValue)
                        break;
                }

                Debug.Assert(idx >= 0);
                if (idx < values.Length)
                    return values[idx];

                return null; //?
            });
        }
    }
}
