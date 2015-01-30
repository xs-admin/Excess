using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class BaseLexicalMatch<TToken, TNode, TModel> : ILexicalMatch<TToken, TNode, TModel>,
                                                                    IDocumentHandler<TToken, TNode, TModel>
    {
        private ILexicalAnalysis<TToken, TNode, TModel> _lexical;
        private ILexicalTransform<TToken, TNode> _transform;
        private List<Func<TToken, Scope, TokenMatch>> _matchers = new List<Func<TToken, Scope, TokenMatch>>();

        public BaseLexicalMatch(ILexicalAnalysis<TToken, TNode, TModel> lexical)
        {
            _lexical = lexical;
        }

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            document.change(ApplyMatchers);
        }

        private IEnumerable<TToken> ApplyMatchers(IEnumerable<TToken> tokens, Scope scope)
        {
            int consumed = 0;
            var result = transform(tokens, scope, out consumed);
            if (result != null)
                scope.set("_consumed", consumed);

            return result;
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

        private static Func<TToken, Scope, TokenMatch> MatchEnclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start, string end, string contents)
        {
            return (token, scope) =>
            {
                dynamic context = scope;
                if (context._state == null)
                {
                    if (!open(token))
                        return TokenMatch.UnMatch;

                    var initialState = new EnclosedState();
                    if (start != null)
                        scope.set(start, token);

                    if (contents != null)
                    {
                        initialState.Contents = new List<TToken>();
                        scope.set(contents, initialState.Contents);
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
                            scope.set(end, token);

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

        public ILexicalMatch<TToken, TNode, TModel> enclosed(string open, string close, string start, string end, string contents)
        {
            return enclosed(MatchString(open), MatchString(close), start, end, contents);
        }

        public ILexicalMatch<TToken, TNode, TModel> enclosed(char open, char close, string start = null, string end = null, string contents = null)
        {
            return enclosed(MatchString(open.ToString()), MatchString(close.ToString()), start, end, contents);
        }

        public ILexicalMatch<TToken, TNode, TModel> enclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start = null, string end = null, string contents = null)
        {
            _matchers.Add(MatchEnclosed(open, close, start, end, contents));
            return this;
        }

        private static Func<TToken, Scope, TokenMatch> MatchMany(Func<TToken, bool> match, string named, bool matchNone = false)
        {
            return (token, scope) =>
            {
                var matches = match(token);

                dynamic context = scope;
                if (context._state == null)
                {
                    if (!matches)
                        return matchNone ? TokenMatch.Match : TokenMatch.UnMatch;

                    var state = new List<TToken>();
                    state.Add(token);

                    if (named != null)
                        scope.set(named, state);

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

        public ILexicalMatch<TToken, TNode, TModel> many(params string[] anyOf)
        {
            return many(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode, TModel> many(params char[] anyOf)
        {
            return many(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode, TModel> many(string[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> many(char[] anyOf, string named = null)
        {
            return many(MatchStringArray(anyOf.Select<char, string>( ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> many(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode, TModel> manyOrNone(params string[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode, TModel> manyOrNone(params char[] anyOf)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode, TModel> manyOrNone(string[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> manyOrNone(char[] anyOf, string named = null)
        {
            return manyOrNone(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> manyOrNone(Func<TToken, bool> tokens, string named = null)
        {
            _matchers.Add(MatchMany(tokens, named, true));
            return this;
        }

        public ILexicalMatch<TToken, TNode, TModel> token(char value, string named = null)
        {
            return token(MatchString(value.ToString()), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> token(string value, string named = null)
        {
            return token(MatchString(value), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> token(Func<TToken, bool> matcher, string named = null)
        {
            _matchers.Add(MatchOne(matcher, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode, TModel> any(params string[] anyOf)
        {
            return any(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode, TModel> any(params char[] anyOf)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode, TModel> any(string[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> any(char[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        private static Func<TToken, Scope, TokenMatch> MatchOne(Func<TToken, bool> match, string named, bool matchNone = false)
        {
            return (token, result) =>
            {
                var matches = match(token);
                if (!matches)
                    return matchNone ? TokenMatch.MatchAndStay : TokenMatch.UnMatch;

                if (named != null)
                    result.set(named, token);

                return TokenMatch.Match;
            };
        }

        public ILexicalMatch<TToken, TNode, TModel> any(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named));
            return this;
        }

        public ILexicalMatch<TToken, TNode, TModel> optional(params string[] anyOf)
        {
            return any(MatchStringArray(anyOf));
        }

        public ILexicalMatch<TToken, TNode, TModel> optional(params char[] anyOf)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())));
        }

        public ILexicalMatch<TToken, TNode, TModel> optional(string[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> optional(char[] anyOf, string named = null)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> optional(Func<TToken, bool> handler, string named = null)
        {
            _matchers.Add(MatchOne(handler, named, true));
            return this;
        }

        private Func<TToken, bool> MatchIdentifier()
        {
            return token => isIdentifier(token); 
        }

        protected abstract bool isIdentifier(TToken token);

        public ILexicalMatch<TToken, TNode, TModel> identifier(string named = null, bool optional = false)
        {
            _matchers.Add(MatchOne(MatchIdentifier(), named, optional));
            return this;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken, TNode>(handler);
            return _lexical;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(ILexicalTransform<TToken, TNode> transform)
        {
            _transform = transform;
            return _lexical;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, Scope scope, out int consumed)
        {
            consumed = 0;

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
                    var matchResult = matcher(token, scope);
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

            return _transform.transform(tokens.Take(consumed), scope);
        }

    }

    public abstract class BaseLexicalAnalysis<TToken, TNode, TModel> :  ILexicalAnalysis<TToken, TNode, TModel>,
                                                                        IDocumentHandler<TToken, TNode, TModel>
    {
        private List<ILexicalMatch<TToken, TNode, TModel>> _matchers = new List<ILexicalMatch<TToken, TNode, TModel>>();

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            foreach (var matcher in _matchers)
            {
                var handler = matcher as IDocumentHandler<TToken, TNode, TModel>;
                Debug.Assert(handler != null);
                handler.apply(document);
            }
        }

        protected abstract ILexicalMatch<TToken, TNode, TModel> createMatch();

        public ILexicalMatch<TToken, TNode, TModel> match()
        {
            var result = createMatch();
            _matchers.Add(result);
            return result;
        }

        public virtual ILexicalTransform<TToken, TNode> transform()
        {
            return new LexicalTransform<TToken, TNode, TModel>();
        }

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> ReplaceExtension(string keyword, ExtensionKind kind, Func<IEnumerable<TToken>, Scope, LexicalExtension<TToken>, IEnumerable<TToken>> handler)
        {
            return (tokens, scope) =>
            {
                dynamic context = scope; 

                var extension = new LexicalExtension<TToken>
                {
                    Kind = kind,
                    Keyword = context.keyword,
                    Identifier = context.id != null ? context.id : default(TToken),
                    Arguments = context.arguments,
                    Body = context.body,
                };

                return handler(tokens, scope, extension);
            };
        }

        Dictionary<int, Func<Scope, LexicalExtension<TToken>, TNode>> _extensions = new Dictionary<int, Func<Scope, LexicalExtension<TToken>, TNode>>();

        private Func<IEnumerable<TToken>, Scope, LexicalExtension<TToken>, IEnumerable<TToken>> SyntacticalExtension(Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            return (tokens, scope, extension) =>
            {
                var compiler = scope.GetService<TToken, TNode, TModel>();

                //insert some placeholders, depending on the extension kind
                switch (extension.Kind)
                {
                    case ExtensionKind.Code:
                    {
                        tokens = compiler.ParseTokens("__extension();");
                        break;
                    }
                    case ExtensionKind.Member:
                    {
                        tokens = compiler.ParseTokens("void __extension() {}");
                        break;
                    }
                    case ExtensionKind.Type:
                    {
                        tokens = compiler.ParseTokens("class __extension() {}");
                        break;
                    }

                    default: throw new InvalidOperationException();
                }

                //schedule the processing of these extensions for a time we actally have sintaxis
                var document = scope.GetDocument<TToken, TNode, TModel>();
                return document.change(tokens, TransformLexicalExtension(extension, handler), kind: "lexical-extension");
            };
        }

        private Func<TNode, Scope, TNode> TransformLexicalExtension(LexicalExtension<TToken> extension, Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            return (node, scope) => handler(node, scope, extension);
        }

        public ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler)
        {
            return extension(keyword, kind, SyntacticalExtension(handler));
        }

        public ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<IEnumerable<TToken>, Scope, LexicalExtension<TToken>, IEnumerable<TToken>> handler)
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
    }
}
