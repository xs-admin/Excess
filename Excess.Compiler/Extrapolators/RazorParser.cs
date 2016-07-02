using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Extrapolators
{
    public class RazorParser
    {
        public static Func<string, Scope, T> Create<T>(string pattern, Scope scope) where T : new()
        {
            var fragments = ParsePattern(pattern, scope);
            return (text, _scope) =>
            {
                throw new NotImplementedException();
            };
        }

        private static IEnumerable<Fragment> ParsePattern(string pattern, Scope scope)
        {
            var index = 0;
            var lastIndex = 0;
            var startBrace = -1;
            var braceCount = -1;
            var buffer = new StringBuilder();
            for (;;)
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
                                yield return new Literal(pattern.Substring(lastIndex, index));

                            startBrace = index;
                            lastIndex = index;
                            braceCount = 1;
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
                                index++;
                            else
                            {
                                braceCount--;
                                if (braceCount == 0)
                                {
                                    add = false;

                                    var nextBrace = pattern.IndexOf('{', index);
                                    var next = pattern.Substring(index, nextBrace);
                                    yield return new Dynamic(next, createParser(pattern.Substring(lastIndex, index), scope));
                                }
                            }
                        }
                        break;
                }

                if (add)
                    buffer.Append(ch);
            }
        }

        private static Func<string, Scope, bool> createParser(string text, Scope scope)
        {
            throw new NotImplementedException();
        }

        abstract class Fragment
        {
            public abstract bool consume(string stream, int position, out int newPosition, Scope scope);
        }

        class Literal : Fragment
        {
            string _value;
            public Literal(string value)
            {
                _value = value;
            }

            public override bool consume(string stream, int position, out int newPosition, Scope scope)
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
            Func<string, Scope, bool> _parser;
            public Dynamic(string next, Func<string, Scope, bool> parser)
            {
                _next = next;
                _parser = parser;
            }

            public override bool consume(string stream, int position, out int newPosition, Scope scope)
            {
                newPosition = -1;

                int nextIdx = stream.IndexOf(_next, position);
                if (nextIdx < 0)
                    return false;

                if (!_parser(stream.Substring(position, nextIdx), scope))
                    return false;

                newPosition = nextIdx + _next.Length;
                return true;
            }
        }
    }
}
