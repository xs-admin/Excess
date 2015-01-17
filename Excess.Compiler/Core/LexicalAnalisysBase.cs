using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Excess.Compiler.Core
{
    using MatchFunction      = Func<SyntaxToken, ILexicalMatchResult, TokenMatch>;
    using MatchTokenFunction = Func<SyntaxToken, bool>;

    public class BaseLexicalMatch : ILexicalMatch
    {
        private List<MatchFunction> _matchers = new List<MatchFunction>();

        protected class EnclosedState
        {
            public EnclosedState()
            {
                DepthCount = 1;
            }

            public int DepthCount             { get; set; }
            public List<SyntaxToken> Contents { get; set; }
        }

        private static MatchFunction MatchEnclosed(MatchTokenFunction open, MatchTokenFunction close, string start, string end, string contents)
        {
            return (SyntaxToken token, ILexicalMatchResult result) =>
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
                        initialState.Contents = new List<SyntaxToken>();
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

        private static MatchTokenFunction MatchString(string value)
        {
            return token => token.ToString() == value;
        }

        private static MatchTokenFunction MatchSyntaxKind(SyntaxKind value)
        {
            return token => token.CSharpKind() == value;
        }

        public ILexicalMatch enclosed(string open, string close, string start, string end, string contents)
        {
            return enclosed(MatchString(open), MatchString(close), start, end, contents);
        }

        public ILexicalMatch enclosed(SyntaxKind open, SyntaxKind close, string start = null, string end = null, string contents = null)
        {
            return enclosed(MatchSyntaxKind(open), MatchSyntaxKind(close), start, end, contents);
        }

        public ILexicalMatch enclosed(char open, char close, string start = null, string end = null, string contents = null)
        {
            return enclosed(MatchString(open.ToString()), MatchString(close.ToString()), start, end, contents);
        }

        public ILexicalMatch enclosed(Func<SyntaxToken, bool> open, Func<SyntaxToken, bool> close, string start = null, string end = null, string contents = null)
        {
            _matchers.Add(MatchEnclosed(open, close, start, end, contents));
            return this;
        }

        private static MatchFunction MatchMany(MatchTokenFunction match, string named, bool matchNone = false)
        {
            return (SyntaxToken token, ILexicalMatchResult result) =>
            {
                var matches = match(token);

                var context = result.context();
                if (context._state == null)
                {
                    if (!matches)
                        return matchNone ? TokenMatch.Match : TokenMatch.UnMatch;

                    var state = new List<SyntaxToken>();
                    state.Add(token);

                    if (named != null)
                        result.context_set(named, state);

                    context._state = state;
                }

                if (matches)
                {
                    if (named != null)
                    {
                        List<SyntaxToken> namedResult = context._state;
                        namedResult.Add(token);
                    }

                    return TokenMatch.MatchAndContinue;
                }

                return TokenMatch.UnMatch;
            };
        }

        private static MatchTokenFunction MatchStringArray(IEnumerable<string> values)
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

        private static MatchTokenFunction MatchSyntaxKindArray(IEnumerable<SyntaxKind> values)
        {
            return token =>
            {
                var tokenValue = token.CSharpKind();
                foreach (var value in values)
                {
                    if (tokenValue == value)
                        return true;
                }

                return false;
            };
        }

        public ILexicalMatch many(params SyntaxKind[] anyOf)
        {
            return many(MatchSyntaxKindArray(anyOf));
        }

        public ILexicalMatch many(params string[] anyOf)
        {
            return many(MatchStringArray(anyOf));
        }

        public ILexicalMatch many(params char[] anyOf)
        {
            return many(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch many(SyntaxKind[] anyOf, string named = null)
        {
            return many(MatchSyntaxKindArray(anyOf), named);
        }

        public ILexicalMatch many(string[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch many(char[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf.Select<char, string>( ch => ch.ToString())), named);
        }

        public ILexicalMatch many(Func<SyntaxToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named));
            return this;
        }

        public ILexicalMatch manyOrNone(params SyntaxKind[] anyOf)
        {
            return manyOrNone(MatchSyntaxKindArray(anyOf));
        }

        public ILexicalMatch manyOrNone(params string[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf));
        }

        public ILexicalMatch manyOrNone(params char[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch manyOrNone(SyntaxKind[] anyOf, string named = null)
        {
            return manyOrNone(MatchSyntaxKindArray(anyOf), named);
        }

        public ILexicalMatch manyOrNone(string[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch manyOrNone(char[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch manyOrNone(Func<SyntaxToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named, true));
            return this;
        }

        public ILexicalAnalysis then(Func<ILexicalMatchResult, IEnumerable<SyntaxToken>> handler)
        {
            throw new NotImplementedException();
        }

        public ILexicalAnalysis then(ILexicalTransform transform)
        {
            throw new NotImplementedException();
        }

        public ILexicalMatch tokens(params SyntaxKind[] anyOf)
        {
            return tokens(MatchSyntaxKindArray(anyOf));
        }

        public ILexicalMatch tokens(params string[] anyOf)
        {
            return tokens(MatchStringArray(anyOf));
        }

        public ILexicalMatch tokens(params char[] anyOf)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch tokens(SyntaxKind[] anyOf, string named = null)
        {
            return tokens(MatchSyntaxKindArray(anyOf), named);
        }

        public ILexicalMatch tokens(string[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch tokens(char[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        private static MatchFunction MatchOne(MatchTokenFunction match, string named, bool matchNone = false)
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

        public ILexicalMatch tokens(Func<SyntaxToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named));
            return this;
        }

        public ILexicalMatch optional(params SyntaxKind[] anyOf)
        {
            return tokens(MatchSyntaxKindArray(anyOf));
        }

        public ILexicalMatch optional(params string[] anyOf)
        {
            return tokens(MatchStringArray(anyOf));
        }

        public ILexicalMatch optional(params char[] anyOf)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch optional(SyntaxKind[] anyOf, string named = null)
        {
            return tokens(MatchSyntaxKindArray(anyOf), named);
        }

        public ILexicalMatch optional(string[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch optional(char[] anyOf, string named = null)
        {
            return tokens(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch optional(Func<SyntaxToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named, true));
            return this;
        }
    }
}
