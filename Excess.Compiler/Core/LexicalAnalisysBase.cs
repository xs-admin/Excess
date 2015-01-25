using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class LexicalMatchResult<TToken> : ILexicalMatchResult<TToken>
    {
        public IEnumerable<TToken> Tokens { get; set; }
        public Scope Scope { get; set; }
        public IEventBus Events { get; set; }

        public LexicalMatchResult(Scope scope, IEventBus events)
        {
            Scope  = scope;
            Events = events;
        }

        public dynamic context()
        {
            return Scope;
        }
    }

    public abstract class BaseLexicalMatch<TToken, TNode> : ILexicalMatch<TToken, TNode>
    {
        private ILexicalAnalysis<TToken, TNode> _lexical;
        private ILexicalTransform<TToken> _transform;
        private List<Func<TToken, ILexicalMatchResult<TToken>, TokenMatch>> _matchers = new List<Func<TToken, ILexicalMatchResult<TToken>, TokenMatch>>();

        public BaseLexicalMatch(ILexicalAnalysis<TToken, TNode> lexical)
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

        private static Func<TToken, ILexicalMatchResult<TToken>, TokenMatch> MatchEnclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start, string end, string contents)
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
                        result.Scope.set(start, token);

                    if (contents != null)
                    {
                        initialState.Contents = new List<TToken>();
                        result.Scope.set(contents, initialState.Contents);
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
                            result.Scope.set(end, token);

                        context._state = null;
                        return TokenMatch.Match;
                    }
                }

                if (state.Contents != null)
                    state.Contents.Add(token);

                return TokenMatch.MatchAndContinue;
            };
        }

        private static Func<TToken, bool> MatchString(string value)
        {
            return token => token.ToString() == value;
        }

        public ILexicalMatch<TToken, TNode> enclosed(string open, string close, string start, string end, string contents)
        {
            return enclosed(MatchString(open), MatchString(close), start, end, contents);
        }

        public ILexicalMatch<TToken, TNode> enclosed(char open, char close, string start = null, string end = null, string contents = null)
        {
            return enclosed(MatchString(open.ToString()), MatchString(close.ToString()), start, end, contents);
        }

        public ILexicalMatch<TToken, TNode> enclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start = null, string end = null, string contents = null)
        {
            _matchers.Add(MatchEnclosed(open, close, start, end, contents));
            return this;
        }

        private static Func<TToken, ILexicalMatchResult<TToken>, TokenMatch> MatchMany(Func<TToken, bool> match, string named, bool matchNone = false)
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
                        result.Scope.set(named, state);

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

        public ILexicalMatch<TToken, TNode> many(params string[] anyOf)
        {
            return many(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode> many(params char[] anyOf)
        {
            return many(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode> many(string[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode> many(char[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf.Select<char, string>( ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode> many(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode> manyOrNone(params string[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode> manyOrNone(params char[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode> manyOrNone(string[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode> manyOrNone(char[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode> manyOrNone(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named, true));
            return this;
        }

        public ILexicalMatch<TToken, TNode> token(char value, string named = null)
        {
            return token(MatchString(value.ToString()), named);
        }

        public ILexicalMatch<TToken, TNode> token(string value, string named = null)
        {
            return token(MatchString(value), named);
        }

        public ILexicalMatch<TToken, TNode> token(Func<TToken, bool> matcher, string named = null)
        {
            _matchers.Add(MatchOne(matcher, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode> any(params string[] anyOf)
        {
            return any(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode> any(params char[] anyOf)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode> any(string[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode> any(char[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        private static Func<TToken, ILexicalMatchResult<TToken>, TokenMatch> MatchOne(Func<TToken, bool> match, string named, bool matchNone = false)
        {
            return (token, result) =>
            {
                var matches = match(token);
                if (!matches)
                    return matchNone ? TokenMatch.MatchAndStay : TokenMatch.UnMatch;

                if (named != null)
                    result.Scope.set(named, token);

                return TokenMatch.Match;
            };
        }

        public ILexicalMatch<TToken, TNode> any(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode> optional(params string[] anyOf)
        {
            return any(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode> optional(params char[] anyOf)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode> optional(string[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode> optional(char[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode> optional(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named, true));
            return this;
        }

        private Func<TToken, bool> MatchIdentifier()
        {
            return token => isIdentifier(token); 
        }

        protected abstract bool isIdentifier(TToken token);

        public ILexicalMatch<TToken, TNode> identifier(string named = null, bool optional = false)
        {
            _matchers.Add(MatchOne(MatchIdentifier(), named, optional));
            return this;
        }

        public ILexicalAnalysis<TToken, TNode> then(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken>(handler);
            return _lexical;
        }

        public ILexicalAnalysis<TToken, TNode> then(ILexicalTransform<TToken> transform)
        {
            _transform = transform;
            return _lexical;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken> result, out int consumed)
        {
            consumed      = 0;
            result.Tokens = tokens;

            int currMatcher = 0;
            foreach (var token in tokens)
            {
                if (currMatcher >= _matchers.Count)
                {
                    if (consumed > 0)
                        break;

                    return null;
                }

                consumed++;

                bool keepMatching = false;
                do
                {
                    var matcher = _matchers[currMatcher];
                    var matchResult = matcher(token, result);
                    switch (matchResult)
                    {
                        case TokenMatch.Match:
                        case TokenMatch.MatchAndContinue:
                        case TokenMatch.MatchAndStay:
                        {
                            if (matchResult != TokenMatch.MatchAndContinue)
                                currMatcher++;

                            keepMatching = matchResult == TokenMatch.MatchAndStay;
                            break;
                        }
                        case TokenMatch.UnMatch:
                            return null;
                    }
                }
                while (keepMatching);
            }

            result.Tokens = tokens.Take(consumed);
            return _transform.transform(result.Tokens, result);
        }

    }

    public abstract class LexicalAnalysis<TToken, TNode> :  ILexicalAnalysis<TToken, TNode>
    {
        private List<ILexicalMatch<TToken, TNode>> _matchers = new List<ILexicalMatch<TToken, TNode>>();
        protected abstract ILexicalMatch<TToken, TNode> createMatch();

        public ILexicalMatch<TToken, TNode> match()
        {
            var result = createMatch();
            _matchers.Add(result);
            return result;
        }

        public abstract ILexicalTransform<TToken> transform();
        public abstract IEnumerable<TToken> parseTokens(string tokens);


        private Func<IEnumerable<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> ReplaceExtension(string keyword, ExtensionKind kind, Func<LexicalExtension<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> handler)
        {
            return (tokens, result) =>
            {
                dynamic context = result.Scope; 

                var extension = new LexicalExtension<TToken>
                {
                    Kind = kind,
                    Keyword = context.keyword,
                    Identifier = context.id != null ? context.id : default(TToken),
                    Arguments = context.arguments,
                    Body = context.body,
                };

                return handler(extension, result);
            };
        }

        protected abstract TToken setLexicalId(TToken token, int value);

        Dictionary<int, Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode>> _extensions = new Dictionary<int, Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode>>();

        private Func<LexicalExtension<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> SyntacticalExtension(Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode> handler)
        {
            return (extension, result) =>
            {
                IEnumerable<TToken> returnValue = null;

                //insert some placeholders, depending on the extension kind
                switch (extension.Kind)
                {
                    case ExtensionKind.Code:
                    {
                        returnValue = parseTokens("__extension();");
                        break;
                    }
                    case ExtensionKind.Member:
                    {
                        returnValue = parseTokens("void __extension() {};");
                        break;
                    }
                    case ExtensionKind.Type:
                    {
                        returnValue = parseTokens("class __extension() {};");
                        break;
                    }

                    default: throw new InvalidOperationException();
                }

                //schedule the processing of these extensions for a time we actally have sintaxis
                int mark = extension.GetHashCode();

                result.Events.schedule(new LexicalExtensionEvent<TToken, TNode>(extension, mark, handler));

                bool firstToken = true;
                return returnValue.Select(token =>
                {
                    if (firstToken)
                    {
                        firstToken = false;
                        return setLexicalId(token, mark);
                    }

                    return token;
                });
            };
        }

        public ILexicalAnalysis<TToken, TNode> extension(string keyword, ExtensionKind kind, Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode> handler)
        {
            return extension(keyword, kind, SyntacticalExtension(handler));
        }

        public ILexicalAnalysis<TToken, TNode> extension(string keyword, ExtensionKind kind, Func<LexicalExtension<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> handler)
        {
            var result = createMatch();

            result
                .token(keyword, named: "keyword")
                .identifier(named: "id", optional: true)
                .enclosed('(', ')', contents: "arguments")
                .enclosed('{', '}', contents: "body")
                .then(ReplaceExtension(keyword, kind, handler));

            _matchers.Add(result);
            return this;
        }

        public IEnumerable<CompilerEvent> produce()
        {
            return new[] { new LexicalMatchEvent<TToken, TNode>(_matchers) };
        }

    }
}
