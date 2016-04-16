using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class BaseLexicalMatchResult<TToken, TNode, TModel> : ILexicalMatchResult<TToken, TNode, TModel>
    {
        IEnumerable<LexicalMatchItem> _items;
        public BaseLexicalMatchResult(IEnumerable<LexicalMatchItem> items, ILexicalTransform<TToken, TNode, TModel> transform)
        {
            _items = items;
            Transform = transform;

            Consumed = 0;
            foreach (var item in items)
                Consumed += item.Span.Length;
        }

        public int Consumed { get; private set; }
        public IEnumerable<LexicalMatchItem> Items { get { return _items; } }
        public ILexicalTransform<TToken, TNode, TModel> Transform { get; set; }

        public IEnumerable<TToken> GetTokens(IEnumerable<TToken> tokens, TokenSpan span)
        {
            return tokens
                .Skip(span.Start)
                .Take(span.Length);
        }
    }

    public class BaseLexicalMatch<TToken, TNode, TModel> : ILexicalMatch<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> _lexical;
        ILexicalTransform<TToken, TNode, TModel> _transform;
        List<Func<MatchResultBuilder, Scope, bool>> _matchers = new List<Func<MatchResultBuilder, Scope, bool>>();

        public BaseLexicalMatch(ILexicalAnalysis<TToken, TNode, TModel> lexical)
        {
            _lexical = lexical;
        }

        private class MatchResultBuilder
        {

            IEnumerable<TToken> _tokens;
            ILexicalTransform<TToken, TNode, TModel> _transform;
            bool _isDocumentStart;

            public MatchResultBuilder(IEnumerable<TToken> tokens, ILexicalTransform<TToken, TNode, TModel> transform, bool isDocumentStart)
            {
                _tokens = tokens;
                _transform = transform;
                _isDocumentStart = isDocumentStart;
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
                _tokens = _tokens.Skip(count);
                return result;
            }

            List<LexicalMatchItem> _items = new List<LexicalMatchItem>();
            public void AddResult(int length, string identifier, bool literal = false)
            {
                _items.Add(new LexicalMatchItem(Consume(length), identifier, literal));
            }

            public bool isDocumentStart()
            {
                return _isDocumentStart && _current == 0;
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

            internal ILexicalMatchResult<TToken, TNode, TModel> GetResult()
            {
                return new BaseLexicalMatchResult<TToken, TNode, TModel>(_items, _transform);
            }
        }

        public ILexicalMatchResult<TToken, TNode, TModel> match(IEnumerable<TToken> tokens, Scope scope, bool isDocumentStart)
        {
            var builder = new MatchResultBuilder(tokens, _transform, isDocumentStart);
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

                int depthCount = 1;
                if (start != null)
                    match.AddResult(1, start);
                else
                    depthCount = 0; //expect an open token

                int contentLength = 0;
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
                                match.AddResult(contentLength, contents);
                                match.AddResult(1, end);
                            }
                            else
                                match.AddResult(contentLength + 1, contents);

                            return true;
                        }
                    }

                    contentLength++;
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

        private static Func<MatchResultBuilder, Scope, bool> MatchIdentifier(string named = null, bool matchNone = false, bool matchDocumentStart = false)
        {
            return (match, scope) =>
            {
                ICompilerService<TToken, TNode, TModel> compiler = scope.GetService<TToken, TNode, TModel>();
                if (compiler.isIdentifier(match.Peek()))
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
                            match.AddResult(contentLength, named);
                            match.AddResult(1, matchNamed);
                        }
                        else
                            match.AddResult(contentLength + 1, named);

                        return true;
                    }

                    contentLength++;
                }

                return false;
            };
        }

        public ILexicalMatch<TToken, TNode, TModel> any(Func<TToken, bool> handler, string named = null, bool matchDocumentStart = false)
        {
            _matchers.Add(MatchOne(handler, named, matchDocumentStart: matchDocumentStart));
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

        public ILexicalMatch<TToken, TNode, TModel> identifier(string named = null, bool optional = false)
        {
            _matchers.Add(MatchIdentifier(named, optional));
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
            return then(new LexicalFunctorTransform<TToken, TNode, TModel>(ScheduleSyntactical(handler)));
        }

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> ScheduleSyntactical(Func<TNode, Scope, TNode> handler)
        {
            return (tokens, scope) =>
            {
                var document = scope.GetDocument<TToken, TNode, TModel>();
                return document.change(tokens, handler);
            };
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler)
        {
            return then(new LexicalFunctorTransform<TToken, TNode, TModel>(ScheduleSemantical(handler)));
        }

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> ScheduleSemantical(Func<TNode, TNode, TModel, Scope, TNode> handler)
        {
            return (tokens, scope) =>
            {
                var document = scope.GetDocument<TToken, TNode, TModel>();
                return document.change(tokens, handler);
            };
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken, TNode, TModel>(handler);
            return _lexical;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(ILexicalTransform<TToken, TNode, TModel> transform)
        {
            _transform = transform;
            return _lexical;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> handler)
        {
            _transform = new LexicalFunctorTransform<TToken, TNode, TModel>(handler);
            return _lexical;
        }
    }

    public class BaseNormalizer<TToken, TNode, TModel> :  INormalizer<TToken, TNode, TModel>
    {
        BaseLexicalAnalysis<TToken, TNode, TModel> _owner;
        public BaseNormalizer(BaseLexicalAnalysis<TToken, TNode, TModel> owner)
        {
            _owner = owner;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> with(
            Func<TNode, IEnumerable<TNode>, Scope, TNode> statements = null,
            Func<TNode, IEnumerable<TNode>, Scope, TNode> members    = null,
            Func<TNode, IEnumerable<TNode>, Scope, TNode> types      = null,
            Func<TNode, Scope, TNode>                     then       = null)
        {
            _owner.normalize(statements, members, types, then);
            return _owner;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> statements(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler)
        {
            _owner.normalize(statements: handler);
            return _owner;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> members(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler)
        {
            _owner.normalize(members: handler);
            return _owner;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> types(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler)
        {
            _owner.normalize(types: handler);
            return _owner;
        }

        public ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler)
        {
            _owner.normalize(then: handler);
            return _owner;
        }

    }

    public abstract class BaseLexicalAnalysis<TToken, TNode, TModel> :  ILexicalAnalysis<TToken, TNode, TModel>,
                                                                        IDocumentInjector<TToken, TNode, TModel>
    {
        private List<ILexicalMatch<TToken, TNode, TModel>> _matchers = new List<ILexicalMatch<TToken, TNode, TModel>>();

        public void apply(IDocument<TToken, TNode, TModel> document)
        {
            document.change(LexicalPass(document.Mapper));

            if (_normalizeStatements != null || _normalizeMembers != null || _normalizeTypes != null)
                document.change(normalize, "normalize");
        }

        public INormalizer<TToken, TNode, TModel> normalize()
        {
            return new BaseNormalizer<TToken, TNode, TModel>(this);
        }

        protected abstract TNode normalize(TNode node, Scope scope);

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> LexicalPass(IMappingService<TToken, TNode> mapper)
        {
            return (tokens, scope) =>
            {
                if (mapper != null)
                    tokens = tokens.Select(token => mapper.Transform(token));

                var allTokens = tokens.ToArray();
                return TransformSpan(allTokens, new TokenSpan(0, allTokens.Length), scope);
            };
        }

        private class MatchInfo
        {
            public TokenSpan Span { get; set; }
            public ILexicalMatchResult<TToken, TNode, TModel> Match { get; set; }
        }

        private IEnumerable<TToken> TransformSpan(TToken[] tokens, TokenSpan span, Scope scope)
        {
            var builders = new List<MatchInfo>();

            int currentToken = 0;
            while (currentToken < span.Length)
            {
                var remaining = Range(tokens, span.Start + currentToken, span.Length - currentToken);

                ILexicalMatchResult<TToken, TNode, TModel> result = null;
                foreach (var matcher in _matchers)
                {
                    result = matcher.match(remaining, scope, currentToken == 0);
                    if (result != null)
                        break;
                }

                if (result != null)
                {
                    builders.Add(new MatchInfo { Span = new TokenSpan(span.Start + currentToken, result.Consumed), Match = result });
                    currentToken += result.Consumed;
                }
                else
                    currentToken++;
            }

            var returnValue = TransformBuilders(tokens, span, builders, scope);
            return returnValue;
        }

        private IEnumerable<TToken> TransformBuilders(TToken[] tokens, TokenSpan span, List<MatchInfo> builders, Scope scope)
        {
            int current = span.Start;
            foreach (var builder in builders)
            {
                if (builder.Span.Start > current)
                {
                    for (int i = current; i < builder.Span.Start; i++)
                        yield return tokens[i]; //literal

                    current = builder.Span.Start;
                }

                int consumed;
                var matchedTokens = buildMatchResult(tokens, builder, scope, out consumed);
                var transformer   = builder.Match.Transform;

                current += consumed;
                Debug.Assert(transformer != null);
                var matchTokens = transformer.transform(matchedTokens, builder.Match, scope);
                foreach (var matchToken in matchTokens)
                    yield return matchToken;
            }

            for (int i = current; i < span.Start + span.Length; i++)
                yield return tokens[i]; //finish
        }

        private IEnumerable<TToken> buildMatchResult(TToken[] tokens, MatchInfo builder, Scope scope, out int consumed)
        {
            List<LexicalMatchItem> resultItems = new List<LexicalMatchItem>();
            List<TToken> resultTokens = new List<TToken>();

            int start = builder.Span.Start;
            foreach (var item in builder.Match.Items)
            {
                var newItem = new LexicalMatchItem(new TokenSpan(resultTokens.Count, item.Span.Length), item.Identifier, item.Literal);
                if (item.Span.Length > 0)
                {
                    var span = new TokenSpan(start + item.Span.Start, item.Span.Length);
                    if (item.Span.Length > 1)
                    {
                        IEnumerable<TToken> itemTokens;
                        if (!item.Literal)
                            itemTokens = TransformSpan(tokens, span, scope);
                        else
                            itemTokens = Range(tokens, span);

                        int oldCount = resultTokens.Count;
                        resultTokens.AddRange(itemTokens);
                        int newCount = itemTokens.Count();

                        newItem.Span.Length = resultTokens.Count - oldCount;
                    }
                    else
                        resultTokens.Add(tokens[span.Start]);
                }

                resultItems.Add(newItem);
            }

            builder.Match = new BaseLexicalMatchResult<TToken, TNode, TModel>(resultItems, builder.Match.Transform);
            consumed = builder.Span.Length;
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
            return new BaseLexicalMatch<TToken, TNode, TModel>(this);
        }

        protected Func<TNode, IEnumerable<TNode>, Scope, TNode> _normalizeStatements;
        protected Func<TNode, IEnumerable<TNode>, Scope, TNode> _normalizeMembers;
        protected Func<TNode, IEnumerable<TNode>, Scope, TNode> _normalizeTypes;
        protected Func<TNode, Scope, TNode> _normalizeThen;
        public void normalize(Func<TNode, IEnumerable<TNode>, Scope, TNode> statements = null, 
                              Func<TNode, IEnumerable<TNode>, Scope, TNode> members    = null,
                              Func<TNode, IEnumerable<TNode>, Scope, TNode> types      = null,
                              Func<TNode, Scope, TNode>                     then       = null)
        {
            if (statements != null)
                _normalizeStatements = statements; //overrides

            if (members != null)
                _normalizeMembers = members;

            if (types != null)
                _normalizeTypes = types;

            if (then != null)
                _normalizeThen = then;
        }

        public virtual ILexicalTransform<TToken, TNode, TModel> transform()
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

                //schedule the processing of these extensions for a time we actally have syntax
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

        public IGrammarAnalysis<TGrammar, GNode, TToken, TNode> grammar<TGrammar, GNode>(string keyword, ExtensionKind kind) where TGrammar : IGrammar<TToken, TNode, GNode>, new()
        {
            return new BaseGrammarAnalysis<TToken, TNode, TModel, GNode, TGrammar>(this, keyword, kind);
        }
    }
}
