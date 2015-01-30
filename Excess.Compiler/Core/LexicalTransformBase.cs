using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class LexicalTransform<TToken, TNode> : ILexicalTransform<TToken, TNode>
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
            public TransformBinder(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> output_, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output  = output_;
                extents = new TokenSpan();
                existingToken = existingToken_;
            }

            public TransformBinder(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> _output, int begin_, int end_, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output = _output;
                extents = new TokenSpan(begin_, end_);
                existingToken = existingToken_;
            }

            public TransformBinder(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> _output, TokenSpan _extents, ExistingToken existingToken_ = ExistingToken.Remove)
            {
                output = _output;
                extents = _extents;
                existingToken = existingToken_;
            }

            public TokenSpan                                             extents;
            public Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> output;
            public ExistingToken                                         existingToken;
        }

        List<Func<Scope, TransformBinder>> _binders = new List<Func<Scope, TransformBinder>>();

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> TokensFromString(string tokenString)
        {
            return (tokens, scope) =>
            {
                var compiler = scope.GetService<TToken, TNode>();
                return compiler.ParseTokens(tokenString);
            };
        }

        private IEnumerable<TToken> EmptyTokens(IEnumerable<TToken> tokens, Scope scope)
        {
            return new TToken[] { };
        }

        public ILexicalTransform<TToken, TNode> insert(string tokens, string before = null, string after = null)
        {
            _binders.Add(scope =>
            {
                TokenSpan     extents;
                ExistingToken existing = ExistingToken.PreInsert;
                if (before != null)
                {
                    extents = labelExtent(scope, before);
                    extents.end = extents.begin;
                    existing = ExistingToken.PostInsert;
                }
                else if (after != null)
                {
                    extents = labelExtent(scope, after);
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

        public ILexicalTransform<TToken, TNode> replace(string named, string tokens)
        {
            _binders.Add(scope =>
            {
                if (named != null)
                    return new TransformBinder(TokensFromString(tokens), labelExtent(scope, named));

                throw new InvalidOperationException("Must specify either 'after' or 'before'");
            });

            return this;
        }

        public ILexicalTransform<TToken, TNode> remove(string named)
        {
            _binders.Add(scope =>
            {
                if (named != null)
                    return new TransformBinder(EmptyTokens, labelExtent(scope, named));

                throw new InvalidOperationException("Must specify 'named'");
            });

            return this;
        }

        ISyntaxTransform<TNode> _syntactical;

        public ILexicalTransform<TToken, TNode> then(string named, Func<TNode, TNode> handler)
        {

            return then(named, new FunctorSyntaxTransform<TNode>(handler));
        }

        public ILexicalTransform<TToken, TNode> then(string named, Func<TNode, Scope , TNode> handler)
        {
            return then(named, new FunctorSyntaxTransform<TNode>(handler));
        }

        public ILexicalTransform<TToken, TNode> then(string named, ISyntaxTransform<TNode> transform)
        {
            Debug.Assert(_syntactical == null);
            _syntactical = transform;

            _binders.Add(scope =>
            {
                if (named != null)
                    return new TransformBinder(MarkTokens(), labelExtent(scope, named));

                throw new InvalidOperationException("Must specify 'named'");
            });

            return this;
        }

        private Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> MarkTokens()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, Scope scope)
        {
            var binderSelector = _binders
                .Select<Func<Scope, TransformBinder>, TransformBinder>(f => f(scope))
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
                    var binderResult = binder.output(Range(tokens, binder.extents), scope);

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

        private IEnumerable<TToken> Range(IEnumerable<TToken> tokens, TokenSpan extents)
        {
            int current = -1;
            foreach (var token in tokens)
            {
                current++;
                if (extents.end < current)
                    break;

                if (extents.begin > current)
                    continue;

                yield return token;
            }
        }

        private TokenSpan labelExtent(Scope scope, string label)
        {
            dynamic context = scope;
            object value    = scope.get<object>(label);
            if (value != null)
            {
                if (value is TokenSpan)
                    return (TokenSpan)value;

                if (value is TToken)
                {
                    TToken valueToken = (TToken)value;
                    int    idx = 0;
                    foreach (var token in context.Tokens)
                    {
                        if (valueToken.Equals(token))
                        {
                            TokenSpan retValue = new TokenSpan(idx, idx);
                            scope.set(label, retValue);
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
                    foreach (var token in context.Tokens)
                    {
                        if (firstToken.Equals(token))
                        {
                            TokenSpan retValue = new TokenSpan(idx, idx + tokenCount);
                            scope.set(label, retValue);
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

    public class LexicalFunctorTransform<TToken, TNode> : ILexicalTransform<TToken, TNode>
    {
        Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> _functor;

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> functor)
        {
            _functor = functor;
        }

        public ILexicalTransform<TToken, TNode> insert(string tokens, string before = null, string after = null)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> replace(string named, string tokens)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> remove(string named)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> then(string token, Func<TNode, TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> then(string token, Func<TNode, Scope , TNode> handler)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> then(string token, ISyntaxTransform<TNode> transform)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, Scope scope)
        {
            return _functor(tokens, scope);
        }
    }
}
