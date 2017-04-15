using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class LexicalTransform<TToken, TNode, TModel> : ILexicalTransform<TToken, TNode, TModel>
    {
        private IEnumerable<TToken> TokensFromString(string tokenString, Scope scope)
        {
            var compiler = scope.GetService<TToken, TNode, TModel>();
            return compiler.ParseTokens(tokenString);
        }

        private IEnumerable<TToken> TokensFromNamed(string named, IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode, TModel> match, Scope scope)
        {
            var item = match
                .Items
                .First(i => i.Identifier == named);

            return match.GetTokens(tokens, item.Span);
        }

        private IEnumerable<TToken> EmptyTokens(IEnumerable<TToken> tokens, Scope scope)
        {
            return new TToken[] { };
        }

        class Transformer
        {
            public string Item { get; set; }
            public Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> Handler { get; set; }
            public Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> MatchHandler { get; set; }
            public int Priority { get; set; }
        }

        List<Transformer> _transformers = new List<Transformer>();

        public ILexicalTransform<TToken, TNode, TModel> insert(string tokenString, string before = null, string after = null)
        {

            if (before != null)
                AddTransformer(before, (tokens, scope) => TokensFromString(tokenString, scope).Union(tokens), 0);
            else if (after != null)
                AddTransformer(after, (tokens, scope) => tokens.Union(TokensFromString(tokenString, scope)), 0);
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

        private void AddTransformer(string target, Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> handler, int priority)
        {
            _transformers.Add(new Transformer
            {
                Item = target,
                MatchHandler = handler,
                Priority = priority
            });
        }

        public ILexicalTransform<TToken, TNode, TModel> replace(string named, string tokenString, string otherNamed)
        {
            if (named != null)
            {
                if (tokenString != null)
                    AddTransformer(named, (tokens, scope) => TokensFromString(tokenString, scope), -1);
                else if (otherNamed != null)
                    AddTransformer(named, (tokens, match, scope) => TokensFromNamed(otherNamed, tokens, match, scope), -1);
                else
                    throw new InvalidOperationException("Must specify either 'tokenString' or 'otherNamed'");
            }
            else
                throw new InvalidOperationException("Must specify 'named'");

            return this;
        }

        public ILexicalTransform<TToken, TNode, TModel> remove(string named)
        {
            if (named != null)
            {
                AddTransformer(named, (tokens, scope) => new TToken[] { }, -1);
            }
            else
                throw new InvalidOperationException("Must specify 'named'");

            return this;
        }

        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode> handler, string referenceToken = null)
        {
            return then((node, scope) => handler(node), referenceToken);
        }

        string _refToken;
        Func<TNode, Scope, TNode> _syntactical;
        Func<TNode, TNode, TModel, Scope, TNode> _semantical;
        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler, string referenceToken = null)
        {
            Debug.Assert(_syntactical == null && _semantical == null);
            _refToken = referenceToken;
            _syntactical = handler;

            return this;
        }

        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler, string referenceToken = null)
        {
            Debug.Assert(_syntactical == null && _semantical == null);
            _refToken = referenceToken;
            _semantical = handler;

            return this;
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode, TModel> match, Scope scope)
        {
            var sorted    = _transformers.OrderBy(t => t.Priority);
            var compiler  = scope.GetService<TToken, TNode, TModel>();
            var needsMark = _syntactical != null;
            int id        = -1;

            foreach (var item in match.Items)
            {
                IEnumerable<TToken> result = match.GetTokens(tokens, item.Span);
                foreach (var transformer in sorted)
                {
                    if (transformer.Item == item.Identifier)
                    {
                        if (transformer.Handler != null)
                            result = transformer.Handler(result, scope);
                        else
                            result = transformer.MatchHandler(tokens, match, scope);
                    }
                }

                if (_refToken != null)
                    needsMark = item.Identifier == _refToken;

                foreach (var token in result)
                {
                    if (needsMark) //port: all the markings should be gone
                    {
                        TToken marked;
                        if (id < 0)
                        {
                            var document = scope.GetDocument<TToken, TNode, TModel>();
                            if (_syntactical != null)
                                marked = document.change(token, _syntactical);
                            else
                            {
                                Debug.Assert(_semantical != null);
                                marked = document.change(token, _semantical);
                            }

                            id = compiler.GetExcessId(marked);
                        }
                        else
                            marked = compiler.InitToken(token, id);

                        yield return marked;
                    }
                    else 
                        yield return token;
                }
            }
        }
    }

    public class LexicalFunctorTransform<TToken, TNode, TModel> : ILexicalTransform<TToken, TNode, TModel>
    {
        Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> _functor;

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> functor)
        {
            _functor = WithScope(functor);
        }

        public LexicalFunctorTransform(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> functor)
        {
            _functor = functor;
        }

        private static Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> WithScope(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> functor)
        {
            return (tokens, match, scope) =>
            {
                foreach (var item in match.Items)
                {
                    if (item.Identifier != null && item.Span.Length > 0)
                    {
                        var idTokens = match.GetTokens(tokens, item.Span);

                        if (item.Span.Length == 1)
                            scope.set(item.Identifier, idTokens.First());
                        else
                            scope.set(item.Identifier, idTokens);
                    }
                }

                return functor(tokens, scope);
            };
        }

        public ILexicalTransform<TToken, TNode, TModel> insert(string tokens, string before = null, string after = null)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> replace(string named, string tokens, string otherNamed)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> remove(string named)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode> handler, string token)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, Scope , TNode> handler, string token)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler, string token)
        {
            throw new InvalidOperationException();
        }

        public ILexicalTransform<TToken, TNode, TModel> then(ISyntaxTransform<TNode> transform, string token)
        {
            throw new InvalidOperationException();
        }

        public IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode, TModel> match, Scope scope)
        {
            return _functor(tokens, match, scope);
        }
    }
}
