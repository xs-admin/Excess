using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseLexicalMatchResult<TToken, TNode> : ILexicalMatchResult<TToken, TNode>
    {
        List<LexicalMatchItem> _items;
        public BaseLexicalMatchResult(List<LexicalMatchItem> items, ILexicalTransform<TToken, TNode> transform)
        {
            Items = items;
            Transform = transform;

            Consumed = 0;
            foreach (var item in items)
                Consumed += item.Span.Length;
        }

        public int Consumed { get; private set; }
        public IEnumerable<LexicalMatchItem> Items { get; private set; }
        public ILexicalTransform<TToken, TNode> Transform { get; set; }

        public IEnumerable<TToken> GetTokens(IEnumerable<TToken> tokens, string identifier)
        {
            Debug.Assert(identifier != null);
            foreach (var item in _items)
            {
                if (item.Identifier == identifier)
                    return tokens
                        .Skip(item.Span.Start)
                        .Take(item.Span.Length);
            }

            return null;
        }
    }

    public class BaseLexicalMatch<TToken, TNode, TModel> : ILexicalMatch<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> _lexical;
        ILexicalTransform<TToken, TNode> _transform;
        List<Func<MatchResultBuilder, Scope, bool>> _matchers = new List<Func<MatchResultBuilder, Scope, bool>>();
        ICompilerService<TToken, TNode, TModel> _compiler;

        public BaseLexicalMatch(ILexicalAnalysis<TToken, TNode, TModel> lexical, ICompilerService<TToken, TNode, TModel> compiler)
        {
            _lexical = lexical;
            _compiler = compiler;
        }

        private class MatchResultBuilder
        {

            IEnumerable<TToken> _tokens;
            List<LexicalMatchItem> _items = new List<LexicalMatchItem>();
            ILexicalTransform<TToken, TNode> _transform;

            public MatchResultBuilder(IEnumerable<TToken> tokens, ILexicalTransform<TToken, TNode> transform)
            {
                _tokens = tokens;
                _transform = transform;
            }

            public TToken Peek()
            {
                return _tokens.FirstOrDefault();
            }

            public IEnumerable<TToken> Remaining()
            {
                return _tokens;
            }

            int _current = 0;
            public TokenSpan Consume(int count)
            {
                var result = new TokenSpan(_current, count);
                _current += count;
                _tokens = _tokens.Take(count);
                return result;
            }

            List<LexicalMatchItem> _results = new List<LexicalMatchItem>();
            public void AddResult(int length, string identifier, bool literal = false)
            {
                _results.Add(new LexicalMatchItem(Consume(length), identifier, literal));
            }

            public bool isDocumentStart()
            {
                return _current == 0;
            }

            public bool Any()
            {
                return _tokens.Any();
            }

            internal TToken Take()
            {
                _current++;
                var result = _tokens.First();
                _tokens = _tokens.Skip(1);
                return result;
            }

            internal ILexicalMatchResult<TToken, TNode> GetResult()
            {
                return new BaseLexicalMatchResult<TToken, TNode>(_items, _transform);
            }
        }

        public ILexicalMatchResult<TToken, TNode> match(IEnumerable<TToken> tokens, Scope scope)
        {
            var builder = new MatchResultBuilder(tokens, _transform);
            var inner = new Scope(scope);
            foreach (var matcher in _matchers)
            {
                if (!matcher(builder, inner))
                    return null;
            }

            return builder.GetResult();
        }


        //matchers
        private static Func<MatchResultBuilder, Scope, bool> MatchEnclosed(Func<TToken, bool> open, Func<TToken, bool> close, string start, string end, string contents)
        {
            return (match, scope) =>
            {
                if (!open(match.Peek()))
                    return false;

                if (start != null)
                    match.AddResult(1, start);

                int contentLength = 0;
                int depthCount    = 1;
                foreach (var token in match.Remaining())
                {
                    if (open(token))
                        depthCount++;
                    else if (close(token))
                    {
                        depthCount--;
                        if (depthCount == 0)
                        {
                            if (end != null)
                            {
                                match.AddResult(contentLength - 1, contents);
                                match.AddResult(1, end);
                            }
                            else
                                match.AddResult(contentLength, contents);

                            return true;
                        }

                        contentLength++;
                    }
                }

                return false;
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

        private static Func<MatchResultBuilder, Scope, bool> MatchMany(Func<TToken, bool> selector, string named, bool matchNone = false, bool literal = false)
        {
            return (match, scope) =>
            {
                int matched = 0;
                while (match.Any())
                {
                    if (!selector(match.Take()))
                        break;

                    matched++;
                }

                if (matched == 0 && !matchNone)
                    return false;

                match.AddResult(matched, named, literal);
                return true;
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

        public ILexicalMatch<TToken, TNode, TModel> any(string[] anyOf, string named = null, bool matchDocumentStart = false)
        {
            return any(MatchStringArray(anyOf), named, matchDocumentStart);
        }

        public ILexicalMatch<TToken, TNode, TModel> any(char[] anyOf, string named = null, bool matchDocumentStart = false)
        {
            return any(MatchStringArray(anyOf.Select<char, string>(ch => ch.ToString())), named, matchDocumentStart);
        }

        private static Func<MatchResultBuilder, Scope, bool> MatchOne(Func<TToken, bool> selector, string named = null, bool matchNone = false, bool matchDocumentStart = false)
        {
            return (match, scope) =>
            {
                if (selector(match.Peek()))
                {
                    match.AddResult(1, named);
                    return true;
                }

                if (matchNone || (matchDocumentStart && match.isDocumentStart()))
                {
                    match.AddResult(0, named);
                    return true;
                }

                return false;
            };
        }

        private static Func<MatchResultBuilder, Scope, bool> MatchUntil(Func<TToken, bool> selector, string named = null, string matchNamed = null)
        {
            return (match, scope) =>
            {
                int contentLength = 0;
                foreach (var token in match.Remaining())
                {
                    if (selector(token))
                    {
                        if (matchNamed != null)
                        {
                            match.AddResult(contentLength - 1, named);
                            match.AddResult(1, matchNamed);
                        }
                        else
                            match.AddResult(contentLength, named);

                        return true;
                    }
                }

                return false;
            };
        }

        public ILexicalMatch<TToken, TNode, TModel> any(Func<TToken, bool> handler, string named = null, bool matchDocumentStart = false)
        {
            _matchers.Add(MatchOne(handler, named, matchDocumentStart));
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
            return token => _compiler.isIdentifier(token); 
        }

        public ILexicalMatch<TToken, TNode, TModel> identifier(string named = null, bool optional = false)
        {
            _matchers.Add(MatchOne(MatchIdentifier(), named, optional));
            return this;
        }

        public ILexicalMatch<TToken, TNode, TModel> until(char token, string named = null)
        {
            return until(MatchString(token.ToString()), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> until(string token, string named = null)
        {
            return until(MatchString(token.ToString()), named);
        }

        public ILexicalMatch<TToken, TNode, TModel> until(Func<TToken, bool> matcher, string named = null)
        {
            _matchers.Add(MatchUntil(matcher, named));
            return this;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler)
        {
            return then(new LexicalFunctorTransform<TToken, TNode>(ScheduleThen(handler)));
        }

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> ScheduleThen(Func<TNode, Scope, TNode> handler)
        {
            return (tokens, scope) =>
            {
                var document = scope.GetDocument<TToken, TNode, TModel>();
                return document.change(tokens, handler);
            };
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

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode>, Scope, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken, TNode>(handler);
            return _lexical;
        }
    }

    public abstract class BaseLexicalAnalysis<TToken, TNode, TModel> :  ILexicalAnalysis<TToken, TNode, TModel>,
                                                                        IDocumentHandler<TToken, TNode, TModel>
    {
        private List<ILexicalMatch<TToken, TNode, TModel>> _matchers = new List<ILexicalMatch<TToken, TNode, TModel>>();

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            document.change(LexicalPass);
        }

        ICompilerService<TToken, TNode, TModel> _compiler;
        private IEnumerable<TToken> LexicalPass(IEnumerable<TToken> tokens, Scope scope)
        {
            _compiler = scope.GetService<TToken, TNode, TModel>();

            var allTokens = tokens.ToArray();
            return TransformSpan(allTokens, new TokenSpan(0, allTokens.Length), scope);
        }

        private class MatchInfo
        {
            public TokenSpan Span { get; set; }
            public ILexicalMatchResult<TToken, TNode> Match { get; set; }
        }

        private IEnumerable<TToken> TransformSpan(TToken[] tokens, TokenSpan span, Scope scope)
        {
            var builders = new List<MatchInfo>();

            int currentToken = 0;
            while (currentToken < span.Length)
            {
                var remaining = Range(tokens, currentToken, span.Length - currentToken);

                ILexicalMatchResult<TToken, TNode> result = null;
                foreach (var matcher in _matchers)
                {
                    result = matcher.match(remaining, scope);
                    if (result != null)
                        break;
                }

                if (result != null)
                {
                    builders.Add(new MatchInfo { Span = new TokenSpan(currentToken, result.Consumed), Match = result });
                    currentToken += result.Consumed;
                }
                else
                    currentToken++;
            }

            return TransformBuilders(tokens, builders, scope);
        }

        private IEnumerable<TToken> TransformBuilders(TToken[] tokens, List<MatchInfo> builders, Scope scope)
        {
            int current = 0;
            foreach (var builder in builders)
            {
                if (builder.Span.Start > current)
                {
                    for (int i = current; i < builder.Span.Start; i++)
                        yield return tokens[current]; //literal
                }

                var matchedTokens = buildMatchResult(tokens, builder, scope);
                var transformer = builder.Match.Transform;

                Debug.Assert(transformer != null);
                var matchTokens = transformer.transform(matchedTokens, builder.Match, scope);
                foreach (var matchToken in matchTokens)
                    yield return matchToken;
            }

            for (int i = current; i < tokens.Length; i++)
                yield return tokens[current]; //finish
        }

        private IEnumerable<TToken> buildMatchResult(TToken[] tokens, MatchInfo builder, Scope scope)
        {
            List<LexicalMatchItem> resultItems = new List<LexicalMatchItem>();
            IEnumerable<TToken> resultTokens = new TToken[] { };

            int last = builder.Span.Start;
            int current = builder.Span.Start;
            foreach (var item in builder.Match.Items)
            {
                if (item.Span.Length > 1)
                {
                    IEnumerable<TToken> itemTokens;
                    if (!item.Literal)
                        itemTokens = TransformSpan(tokens, item.Span, scope);
                    else
                        itemTokens = Range(tokens, item.Span);

                    resultTokens = resultTokens.Union(itemTokens);
                    item.Span.Start = current;
                    current += resultTokens.Count();
                }
                else
                {
                    resultTokens = resultTokens.Union(new[] { tokens[item.Span.Start] });
                    item.Span.Start = current;
                    current++;
                }

                resultItems.Add(item);
            }

            builder.Match = new BaseLexicalMatchResult<TToken, TNode>(resultItems, builder.Match.Transform);
            return resultTokens;
        }

        private IEnumerable<TToken> Range(TToken[] tokens, TokenSpan span)
        {
            return Range(tokens, span.Start, span.Length);
        }

        private IEnumerable<TToken> Range(TToken[] tokens, int start, int length)
        {
            for (int i = 0; i < length; i++)
                yield return tokens[start + i];
        }

        public ILexicalMatch<TToken, TNode, TModel> match()
        {
            var result = createMatch();
            _matchers.Add(result);
            return result;
        }

        private ILexicalMatch<TToken, TNode, TModel> createMatch()
        {
            return new BaseLexicalMatch<TToken, TNode, TModel>(this, _compiler);
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
