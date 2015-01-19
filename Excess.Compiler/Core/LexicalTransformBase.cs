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
        protected struct TokenSpan
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

        protected struct TransformBinder
        {
            public TransformBinder(Func<IEnumerable<TToken>> output_)
            {
                output  = output_;
                extents = new TokenSpan();
            }

            public TransformBinder(Func<IEnumerable<TToken>> _output, int begin_, int end_)
            {
                output = _output;
                extents = new TokenSpan(begin_, end_);
            }

            public TransformBinder(Func<IEnumerable<TToken>> _output, TokenSpan _extents)
            {
                output = _output;
                extents = _extents;
            }

            public TokenSpan                 extents;
            public Func<IEnumerable<TToken>> output;
        }

        List<Func<ILexicalMatchResult, TransformBinder>> _binders = new List<Func<ILexicalMatchResult, TransformBinder>>();

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
                TokenSpan extents;

                if (before != null)
                {
                    extents = labelExtent(lexicalResult.context(), before);
                    extents.end = extents.begin;
                }
                else if (after != null)
                {
                    extents = labelExtent(lexicalResult.context(), after);
                    extents.begin = extents.end;
                }
                else
                {
                    throw new InvalidOperationException("Must specify either 'after' or 'before'");
                }

                return new TransformBinder(TokensFromString(tokens), extents);
            });

            return this;
        }

        public ILexicalTransform<TToken> replace(string named, string tokens)
        {
            _binders.Add(lexicalResult =>
            {
                if (named != null)
                    return new TransformBinder(TokensFromString(tokens), labelExtent(lexicalResult.context(), named));

                throw new InvalidOperationException("Must specify either 'after' or 'before'");
            });

            return this;
        }

        public ILexicalTransform<TToken> remove(string named)
        {
            _binders.Add(lexicalResult =>
            {
                if (named != null)
                    return new TransformBinder(EmptyTokens(), labelExtent(lexicalResult.context(), named));

                throw new InvalidOperationException("Must specify either 'name'");
            });

            return this;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult result)
        {
            var binderSelector = _binders
                .Select<Func<ILexicalMatchResult, TransformBinder>, TransformBinder>(f => f(result))
                .OrderBy(binder => binder.extents.begin);

            TransformBinder[] binders       = binderSelector.ToArray();
            int               currentToken  = 0;
            int               currentBinder = 0;
            int               binderCount   = binders.Length;
            int               skipTokens    = 0;

            foreach (var token in tokens)
            {
                if (skipTokens > 0)
                {
                    skipTokens--;
                    continue;
                }

                var binderBegin = -1;
                while (currentBinder < binderCount && binderBegin < currentToken)
                {
                    binderBegin = binders[currentBinder].extents.begin;
                    currentBinder++;
                }

                if (binderBegin == currentToken)
                {
                    var binder = binders[binderBegin];
                    var binderResult = binder.output();
                    foreach (var binderToken in binderResult)
                        yield return binderToken;

                    currentBinder++;
                    skipTokens = binder.extents.end = binderBegin;
                }
                else
                    yield return token;

                currentToken++;
            }
        }

        protected abstract IEnumerable<TToken> tokensFromString(string tokenstring);

        private TokenSpan labelExtent(IDictionary<string, object> context, string label)
        {
            TokenSpan? result = (TokenSpan?)context[label];
            return result != null ? (TokenSpan)result : new TokenSpan();
        }
    }

    public class LexicalFunctorTransform<TToken> : ILexicalTransform<TToken>
    {
        Func<IEnumerable<TToken>, ILexicalMatchResult, IEnumerable<TToken>> _functor;

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, ILexicalMatchResult, IEnumerable<TToken>> functor)
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

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult result)
        {
            return _functor(tokens, result);
        }
    }
}
