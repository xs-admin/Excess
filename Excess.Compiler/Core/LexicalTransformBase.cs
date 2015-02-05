using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class LexicalTransform<TToken, TNode, TModel> : ILexicalTransform<TToken, TNode>
    {
        private IEnumerable<TToken> TokensFromString(string tokenString, Scope scope)
        {
            var compiler = scope.GetService<TToken, TNode, TModel>();
            return compiler.ParseTokens(tokenString);
        }

        private IEnumerable<TToken> EmptyTokens(IEnumerable<TToken> tokens, Scope scope)
        {
            return new TToken[] { };
        }

        class Transformer
        {
            public string Item { get; set; }
            public Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> Handler { get; set; }
            public int Priority { get; set; }
        }

        List<Transformer> _transformers = new List<Transformer>();

        public ILexicalTransform<TToken, TNode> insert(string tokenString, string before = null, string after = null)
        {

            if (before != null)
                AddTransformer(before, (tokens, scope) => TokensFromString(tokenString, scope).Union(tokens), 0);
            else if (after != null)
                AddTransformer(after, (tokens, scope) => TokensFromString(tokenString, scope).Union(tokens), 0);
            else
            {
                throw new InvalidOperationException("Must specify either 'after' or 'before'");
            }

            return this;
        }

        private void AddTransformer(string target, Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> handler, int priority)
        {
            _transformers.Add(new Transformer
            {
                Item = target,
                Handler = handler,
                Priority = priority
            });
        }

        public ILexicalTransform<TToken, TNode> replace(string named, string tokenString)
        {
            if (named != null)
                AddTransformer(named, (tokens, scope) => TokensFromString(tokenString, scope), -1);
            else
                throw new InvalidOperationException("Must specify 'named'");

            return this;
        }

        public ILexicalTransform<TToken, TNode> remove(string named)
        {
            if (named != null)
            {
                AddTransformer(named, (tokens, scope) => new TToken[] { }, -1);
            }
            else
                throw new InvalidOperationException("Must specify 'named'");

            return this;
        }

        public ILexicalTransform<TToken, TNode> then(Func<TNode, TNode> handler, string referenceToken = null)
        {
            return then((node, scope) => handler(node), referenceToken);
        }

        string                    _refToken;
        Func<TNode, Scope, TNode> _syntactical;
        public ILexicalTransform<TToken, TNode> then(Func<TNode, Scope, TNode> handler, string referenceToken = null)
        {
            Debug.Assert(_syntactical == null);
            _refToken = referenceToken;
            _syntactical = handler;

            return this;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode> match, Scope scope)
        {
            var sorted = _transformers.OrderBy(t => t.Priority);

            foreach (var item in match.Items)
            {
                IEnumerable<TToken> result = match.GetTokens(tokens, item.Identifier);
                foreach (var transformer in sorted)
                {
                    if (transformer.Item == item.Identifier)
                        result = transformer.Handler(result, scope);
                }

                foreach (var token in result)
                    yield return token;
            }
        }
    }

    public class LexicalFunctorTransform<TToken, TNode> : ILexicalTransform<TToken, TNode>
    {
        Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode>, Scope, IEnumerable<TToken>> _functor;

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> functor)
        {
            _functor = WithScope(functor);
        }

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode>, Scope, IEnumerable<TToken>> functor)
        {
            _functor = functor;
        }

        private static Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode>, Scope, IEnumerable<TToken>> WithScope(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> functor)
        {
            return (tokens, match, scope) =>
            {
                foreach (var item in match.Items)
                {
                    if (item.Identifier != null)
                        scope.set(item.Identifier, match.GetTokens(tokens, item.Identifier));
                }

                return functor(tokens, scope);
            };
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

        public ILexicalTransform<TToken, TNode> then(Func<TNode, TNode> handler, string token)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> then(Func<TNode, Scope , TNode> handler, string token)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode> then(ISyntaxTransform<TNode> transform, string token)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode> match, Scope scope)
        {
            return _functor(tokens, match, scope);
        }
    }
}
