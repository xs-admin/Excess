using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseLexicalPass<TToken> : BasePass
    {
        string _text;
        public BaseLexicalPass(string text)
        {
            _text = text;
        }

        public override ICompilerPass Compile(IEventBus events, Scope scope)
        {
            var myEvents = events.poll(passId());
            var matchEvents = myEvents.OfType<LexicalMatchEvent<TToken>>();

            var tokens   = parseTokens(_text).ToArray();
            var matchers = GetMatchers(matchEvents);
            IEnumerable<TToken> result = transformTokens(tokens, 0, tokens.Length, matchers);

            //calculate new text
            //td: !! mapping info
            string newText = tokensToString(result);
            return continuation(newText);
        }

        protected abstract ICompilerPass continuation(string transformed);

        private IEnumerable<ILexicalMatch<TToken>> GetMatchers(IEnumerable<LexicalMatchEvent<TToken>> events)
        {
            foreach (var ev in events)
            {
                foreach (var matcher in ev.Matchers)
                    yield return matcher;
            }
        }

        protected abstract IEnumerable<TToken> parseTokens(string text);
        protected abstract string tokensToString(IEnumerable<TToken> tokens);

        private static IEnumerable<TToken> Range(TToken[] tokens, int begin, int end)
        {
            for (int i = begin; i < end; i++)
                yield return tokens[i];
        }

        private IEnumerable<TToken> transformTokens(TToken[] tokens, int begin, int end, IEnumerable<ILexicalMatch<TToken>> matchers)
        {
            for (int token = 0; token < end; token++)
            {
                IEnumerable<TToken> transformed = null;
                int consumed = 0;
                foreach (var matcher in matchers)
                {
                    transformed = matcher.transform(Range(tokens, token, end), out consumed);
                    if (transformed != null)
                        break;
                }

                if (transformed == null)
                    yield return tokens[token];
                else
                {
                    foreach (var tt in transformed)
                        yield return tt;

                    token += consumed - 1;
                }
            }
        }
    }
}
