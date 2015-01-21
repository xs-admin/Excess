using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class LexicalTransform<TToken> : ILexicalTransform<TToken>
    {
        protected class TokenSpan
        {
            public TokenSpan()
            {
                begin = -1;
                end   = -1;
            }

            public TokenSpan(int _begin, int _end)
            {
                begin = _begin;
                end = _end;
            }

            public int begin;
            public int end;
        }

        public enum ExistingToken
        {
            Remove,
            PreInsert,
            PostInsert
        }

        protected struct TransformBinder
        {
            public TransformBinder(Func<IEnumerable<TToken>> output_, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output  = output_;
                extents = new TokenSpan();
                existingToken = existingToken_;
            }

            public TransformBinder(Func<IEnumerable<TToken>> _output, int begin_, int end_, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output = _output;
                extents = new TokenSpan(begin_, end_);
                existingToken = existingToken_;
            }

            public TransformBinder(Func<IEnumerable<TToken>> _output, TokenSpan _extents, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output = _output;
                extents = _extents;
                existingToken = existingToken_;
            }

            public TokenSpan                 extents;
            public Func<IEnumerable<TToken>> output;
            public ExistingToken             existingToken;
        }

        List<Func<ILexicalMatchResult<TToken>, TransformBinder>> _binders = new List<Func<ILexicalMatchResult<TToken>, TransformBinder>>();

        private Func<IEnumerable<TToken>> TokensFromString(string tokens)
        {
            return () => tokensFromString(tokens);
        }

        private Func<IEnumerable<TToken>> EmptyTokens()
        {
            return () => new TToken[] { };
        }
        

        public ILexicalTransform<TToken> insert(string tokens, string before = null, string after = null)
        {
            _binders.Add(lexicalResult =>
            {
                TokenSpan     extents;
                ExistingToken existing = ExistingToken.PreInsert;
                if (before != null)
                {
                    extents = labelExtent(lexicalResult, before);
                    extents.end = extents.begin;
                    existing = ExistingToken.PostInsert;
                }
                else if (after != null)
                {
                    extents = labelExtent(lexicalResult, after);
                    extents.begin = extents.end;
                }
                else
                {
                    throw new InvalidOperationException("Must specify either 'after' or 'before'");
                }

                return new TransformBinder(TokensFromString(tokens), extents, existing);
            });

            return this;
        }

        public ILexicalTransform<TToken> replace(string named, string tokens)
        {
            _binders.Add(lexicalResult =>
            {
                if (named != null)
                    return new TransformBinder(TokensFromString(tokens), labelExtent(lexicalResult, named));

                throw new InvalidOperationException("Must specify either 'after' or 'before'");
            });

            return this;
        }

        public ILexicalTransform<TToken> remove(string named)
        {
            _binders.Add(lexicalResult =>
            {
                if (named != null)
                    return new TransformBinder(EmptyTokens(), labelExtent(lexicalResult, named));

                throw new InvalidOperationException("Must specify 'named'");
            });

            return this;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken> result)
        {
            var binderSelector = _binders
                .Select<Func<ILexicalMatchResult<TToken>, TransformBinder>, TransformBinder>(f => f(result))
                .OrderBy(binder => binder.extents.begin);

            TransformBinder[] binders       = binderSelector.ToArray();
            int               currentToken  = 0;
            int               currentBinder = -1;
            int               binderCount   = binders.Length;
            int               skipTokens    = 0;
            int               binderBegin   = -1;

            foreach (var token in tokens)
            {
                if (skipTokens > 0)
                {
                    skipTokens--;
                    continue;
                }

                while (currentBinder < binderCount && binderBegin < currentToken)
                {
                    currentBinder++;
                    binderBegin = binders[currentBinder].extents.begin;
                }

                if (binderBegin == currentToken)
                {
                    var binder = binders[currentBinder];
                    var binderResult = binder.output();

                    if (binder.existingToken == ExistingToken.PreInsert)
                        yield return token;

                    foreach (var binderToken in binderResult)
                        yield return binderToken;

                    if (binder.existingToken == ExistingToken.PostInsert)
                        yield return token;

                    skipTokens = binder.extents.end - binderBegin;

                    currentBinder++;
                    if (currentBinder < binderCount)
                        binderBegin = binders[currentBinder].extents.begin;
                    else
                        binderBegin = int.MaxValue;
                }
                else
                    yield return token;

                currentToken++;
            }
        }

        protected abstract IEnumerable<TToken> tokensFromString(string tokenstring);

        private TokenSpan labelExtent(ILexicalMatchResult<TToken> result, string label)
        {
            object value = result.context().get<object>(label);
            if (value != null)
            {
                if (value is TokenSpan)
                    return (TokenSpan)value;

                if (value is TToken)
                {
                    TToken valueToken = (TToken)value;
                    int    idx = 0;
                    foreach (var token in result.Tokens)
                    {
                        if (valueToken.Equals(token))
                        {
                            TokenSpan retValue = new TokenSpan(idx, idx);
                            result.context().set(label, retValue);
                            return retValue;
                        }

                        idx++;
                    }

                    return null;
                }

                if (value is IEnumerable<TToken>)
                {
                    var tokens     = value as IEnumerable<TToken>;
                    var tokenCount = tokens.Count();
                    if (tokenCount <= 0)
                        return null;

                    var firstToken = tokens.First();
                    int idx = 0;
                    foreach (var token in result.Tokens)
                    {
                        if (firstToken.Equals(token))
                        {
                            TokenSpan retValue = new TokenSpan(idx, idx + tokenCount);
                            result.context().set(label, retValue);
                            return retValue;
                        }

                        idx++;
                    }

                    return null;
                }
            }


            return null;
        }
    }

    public class LexicalFunctorTransform<TToken> : ILexicalTransform<TToken>
    {
        Func<IEnumerable<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> _functor;

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> functor)
        {
            _functor = functor;
        }

        public ILexicalTransform<TToken> insert(string tokens, string before = null, string after = null)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken> replace(string named, string tokens)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken> remove(string named)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken> result)
        {
            return _functor(tokens, result);
        }
    }
}
