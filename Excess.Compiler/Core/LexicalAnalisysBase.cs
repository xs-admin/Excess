using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class LexicalMatchResult : ILexicalMatchResult
    {
        ExpandoObject _context = new ExpandoObject();

        public dynamic context()
        {
            return _context;
        }

        public void context_set(string name, dynamic value)
        {
            IDictionary<string, object> values = _context;
            values[name] = value;
        }
    }

    public class LexicalMatch<TToken> : ILexicalMatch<TToken>
    {
        private ILexicalAnalysis<TToken>                            _lexical;
        private ILexicalTransform<TToken>                           _transform;
        private List<Func<TToken, ILexicalMatchResult, TokenMatch>> _matchers = new List<Func<TToken, ILexicalMatchResult, TokenMatch>>();

        public LexicalMatch(ILexicalAnalysis<TToken> lexical)
        {
            _lexical = lexical;
        }

        protected class EnclosedState
        {
            public EnclosedState()
            {
                DepthCount = 1;
            }

            public int DepthCount        { get; set; }
            public List<TToken> Contents { get; set; }
        }

        private static Func<TToken, ILexicalMatchResult, TokenMatch> MatchEnclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start, string end, string contents)
        {
            return (token, result) =>
            {
                dynamic context = result.context();
                if (context._state == null)
                {
                    if (!open(token))
                        return TokenMatch.UnMatch;

                    var initialState = new EnclosedState();
                    if (start != null)
                        result.context_set(start, token);

                    if (contents != null)
                    {
                        initialState.Contents = new List<TToken>();
                        result.context_set(contents, initialState.Contents);
                    }

                    context._state = initialState;
                    return TokenMatch.MatchAndContinue;
                }

                EnclosedState state = context._state;
                if (open(token))
                    state.DepthCount++;
                else if (close(token))
                {
                    state.DepthCount--;
                    if (state.DepthCount == 0)
                    {
                        if (end != null)
                            result.context_set(end, token);

                        return TokenMatch.Match;
                    }
                }

                return TokenMatch.UnMatch;
            };
        }

        private static Func<TToken, bool> MatchString(string value)
        {
            return token => token.ToString() == value;
        }

        public ILexicalMatch<TToken> enclosed(string open, string close, string start, string end, string contents)
        {
            return enclosed(MatchString(open), MatchString(close), start, end, contents);
        }

        public ILexicalMatch<TToken> enclosed(char open, char close, string start = null, string end = null, string contents = null)
        {
            return enclosed(MatchString(open.ToString()), MatchString(close.ToString()), start, end, contents);
        }

        public ILexicalMatch<TToken> enclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start = null, string end = null, string contents = null)
        {
            _matchers.Add(MatchEnclosed(open, close, start, end, contents));
            return this;
        }

        private static Func<TToken, ILexicalMatchResult, TokenMatch> MatchMany(Func<TToken, bool> match, string named, bool matchNone = false)
        {
            return (token, result) =>
            {
                var matches = match(token);

                var context = result.context();
                if (context._state == null)
                {
                    if (!matches)
                        return matchNone ? TokenMatch.Match : TokenMatch.UnMatch;

                    var state = new List<TToken>();
                    state.Add(token);

                    if (named != null)
                        result.context_set(named, state);

                    context._state = state;
                }

                if (matches)
                {
                    if (named != null)
                    {
                        List<TToken> namedResult = context._state;
                        namedResult.Add(token);
                    }

                    return TokenMatch.MatchAndContinue;
                }

                return TokenMatch.UnMatch;
            };
        }

        private static Func<TToken, bool> MatchStringArray(IEnumerable<string> values)
        {
            return token =>
            {
                var tokenValue = token.ToString();
                foreach (var value in values)
                {
                    if (tokenValue == value)
                        return true;
                }

                return false;
            };
        }

        public ILexicalMatch<TToken> many(params string[] anyOf)
        {
            return many(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken> many(params char[] anyOf)
        {
            return many(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken> many(string[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken> many(char[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf.Select<char, string>( ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken> many(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named));
            return this;
        }

        public ILexicalMatch<TToken> manyOrNone(params string[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken> manyOrNone(params char[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken> manyOrNone(string[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken> manyOrNone(char[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken> manyOrNone(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named, true));
            return this;
        }

        public ILexicalMatch<TToken> tokens(params string[] anyOf)
        {
            return tokens(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken> tokens(params char[] anyOf)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken> tokens(string[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken> tokens(char[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        private static Func<TToken, ILexicalMatchResult, TokenMatch> MatchOne(Func<TToken, bool> match, string named, bool matchNone = false)
        {
            return (token, result) =>
            {
                var matches = match(token);
                if (!matches)
                    return matchNone ? TokenMatch.Match : TokenMatch.UnMatch;

                if (named != null)
                    result.context_set(named, token);

                return TokenMatch.Match;
            };
        }

        public ILexicalMatch<TToken> tokens(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named));
            return this;
        }

        public ILexicalMatch<TToken> optional(params string[] anyOf)
        {
            return tokens(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken> optional(params char[] anyOf)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken> optional(string[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken> optional(char[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken> optional(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named, true));
            return this;
        }

        public ILexicalAnalysis<TToken> then(Func<IEnumerable<TToken>, ILexicalMatchResult, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken>(handler);
            return _lexical;
        }

        public ILexicalAnalysis<TToken> then(ILexicalTransform<TToken> transform)
        {
            _transform = transform;
            return _lexical;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, out int consumed)
        {
                consumed    = 0;
            int currMatcher = 0;
            var result      = new LexicalMatchResult();
            foreach (var token in tokens)
            {
                if (currMatcher >= _matchers.Count)
                {
                    if (consumed > 0)
                        break;

                    return null;
                }

                consumed++;

                var matcher     = _matchers[currMatcher];
                var matchResult = matcher(token, result);
                switch (matchResult)
                {
                    case TokenMatch.Match:
                    case TokenMatch.MatchAndContinue:
                    {
                        if (matchResult != TokenMatch.MatchAndContinue)
                            currMatcher++;
                        break;
                    }
                    case TokenMatch.UnMatch:
                        return null;
                }
            }

            return _transform.transform(tokens.Take(consumed), result);
        }

    }

    public abstract class LexicalAnalysis<TToken> :  ILexicalAnalysis<TToken>
    {
        private List<ILexicalMatch<TToken>> _matchers = new List<ILexicalMatch<TToken>>();

        public ILexicalMatch<TToken> match()
        {
            var result = new LexicalMatch<TToken>(this);
            _matchers.Add(result);
            return result;
        }

        public abstract ILexicalTransform<TToken> transform();

        public IEnumerable<CompilerEvent> produce()
        {
            return new[] { new LexicalMatchEvent<TToken>(_matchers) };
        }
    }

}
