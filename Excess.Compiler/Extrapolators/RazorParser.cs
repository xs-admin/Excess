using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Extrapolators
{
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RazorParser
    {
        public static Func<string, Scope, T> Create<T>(string pattern) where T : new()
        {
            var fragments = parsePattern<T>(pattern).ToArray();
            return (text, scope) =>
            {
                var values = new Dictionary<string, string>();
                var index = 0;
                foreach (var fragment in fragments)
                {
                    var newIndex = 0;
                    if (!fragment.parse(text, index, out newIndex, scope, values))
                        return default(T);

                    index = newIndex;
                }

                return Loader.Load<T>(values);
            };
        }

        private static IEnumerable<Fragment> parsePattern<T>(string pattern)
        {
            var index = 0;
            var lastIndex = 0;
            var startBrace = -1;
            var braceCount = -1;
            var buffer = new StringBuilder();
            for (;; index++)
            {
                if (index == pattern.Length)
                    yield break;

                var ch = pattern[index];
                var add = true;
                switch (ch)
                {
                    case '{':
                        if (startBrace < 0)
                        {
                            //found brace
                            if (index > lastIndex)
                                yield return new Literal(buffer.ToString());

                            buffer.Clear();
                            startBrace = index;
                            lastIndex = index;
                            braceCount = 1;

                            add = false;
                        }
                        else if (startBrace == index - 1)
                        {
                            add = false;
                            startBrace = -1; //{{, reset
                        }
                        else
                            braceCount++;
                        break;
                    case '}':
                        if (startBrace > 0)
                        {
                            if (pattern.Length > index + 1 && pattern[index + 1] == '}')
                                index++; //}}
                            else
                            {
                                braceCount--;
                                if (braceCount == 0)
                                {
                                    var nextBrace = pattern.IndexOf('{', index);
                                    var next =  nextBrace > 0
                                        ? pattern.Substring(index + 1, nextBrace - (index + 1))
                                        : null;

                                    yield return new Dynamic(next, createParser<T>(buffer.ToString()));

                                    buffer.Clear();
                                    add = false;
                                    index = nextBrace;
                                }
                            }
                        }
                        break;
                }

                if (add)
                    buffer.Append(ch);
            }
        }

        private static Func<string, IDictionary<string, string>, Scope, bool> createParser<T>(string pattern)
        {
            var expr = CSharp.ParseExpression(pattern);
            if (expr.ContainsDiagnostics)
                throw new InvalidProgramException("bad expression");

            if (expr is IdentifierNameSyntax)
            {
                var indentifier = expr.ToString();
                return (text, values, scope) =>
                {
                    values[indentifier] = text;
                    return true;
                };
            }
            else if (expr is AssignmentExpressionSyntax)
            {
                var assing = expr as AssignmentExpressionSyntax;
                if (assing != null)
                {
                    Debug.Assert(false); //td:
                }
            }

            return null;
        }

        abstract class Fragment
        {
            public abstract bool parse(string stream, int position, out int newPosition, Scope scope, IDictionary<string, string> values);
        }

        class Literal : Fragment
        {
            string _value;
            public Literal(string value)
            {
                _value = value;
            }

            public override bool parse(string stream, int position, out int newPosition, Scope scope, IDictionary<string, string> values)
            {
                newPosition = position;
                foreach (var ch in _value)
                {
                    if (stream[newPosition] != ch)
                        return false;

                    newPosition++;
                }

                return true;
            }
        }

        class Dynamic : Fragment
        {
            string _next;
            Func<string, IDictionary<string, string>, Scope, bool> _parser;
            public Dynamic(string next, Func<string, IDictionary<string, string>, Scope, bool> parser)
            {
                _next = next;
                _parser = parser;
            }

            public override bool parse(string stream, int position, out int newPosition, Scope scope, IDictionary<string, string> values)
            {
                newPosition = -1;

                int nextIdx = _next != null
                    ? stream.IndexOf(_next, position)
                    : stream.Length;

                if (nextIdx < 0)
                    return false;

                if (!_parser(stream.Substring(position, nextIdx - position), values, scope))
                    return false;

                newPosition = nextIdx + _next.Length;
                return true;
            }
        }
    }
}
