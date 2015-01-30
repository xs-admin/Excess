using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public class PendingExtension<TToken, TNode>
    {
        public LexicalExtension<TToken> Extension { get; set; }
        public SourceSpan Span { get; set; }
        public TNode Node { get; set; }
        public Func<Scope, LexicalExtension<TToken>, TNode> Handler { get; set; }
    }

    public class BaseLexicalPass<TToken, TNode, TModel> 
    {
        IEnumerable<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>> _transformers;
        ICompilerService<TToken, TNode> _compiler;
        public BaseLexicalPass(Scope scope, IEnumerable<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>> transformers)
        {
            _transformers = transformers;
            _scope = scope;
            _compiler = scope.GetService<TToken, TNode>();
        }

        protected Scope _scope;

        public TNode Parse(string text, Dictionary<string, SourceSpan> annotations)
        {
            var tokens = _compiler.ParseTokens(text).ToArray();
            var result = transformTokens(tokens, 0, tokens.Length, _transformers);

            //calculate new text
            //td: !! mapping info

            StringBuilder newText = new StringBuilder();
            string        currId  = null;
            foreach (var token in result)
            {
                string excessId;
                string toInsert = _compiler.TokenToString(token, out excessId);

                //store the actual position in the transformed stream of any tokens pending processing
                if (excessId != currId)
                {
                    if (excessId != null)
                        annotations[excessId] = new SourceSpan(newText.Length, toInsert.Length);

                    currId = excessId;
                }
                else if (excessId != null)
                {
                    //augment span
                    SourceSpan span = annotations[excessId];
                    span.Length += toInsert.Length;
                }
                else
                    currId = null;

                newText.Append(toInsert);
            }

            var root = _compiler.Parse(newText.ToString());
            return _compiler.MarkTree(root);
        }

        private static IEnumerable<TToken> Range(TToken[] tokens, int begin, int end)
        {
            for (int i = begin; i < end; i++)
                yield return tokens[i];
        }

        private IEnumerable<TToken> transformTokens(TToken[] tokens, int begin, int end, IEnumerable<Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>>> transformers)
        {
            for (int token = 0; token < end; token++)
            {
                IEnumerable<TToken> transformed = null;
                int                 consumed = 0;
                Scope               scope = new Scope();
                foreach (var transformer in transformers)
                {
                    transformed = transformer(Range(tokens, token, end), scope);
                    if (transformed != null)
                    {
                        consumed = (int)scope.get("consumed");
                        break;
                    }
                }

                if (transformed == null)
                    yield return tokens[token];
                else
                {
                    foreach (var tt in transformed)
                    {
                        yield return tt;
                    }

                    token += consumed - 1;
                }
            }
        }
    }
}
