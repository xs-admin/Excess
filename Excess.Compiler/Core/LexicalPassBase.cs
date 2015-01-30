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

    public class BaseLexicalPass<TToken, TNode> 
    {
        IEnumerable<ILexicalMatch<TToken, TNode>> _matchers;
        ICompilerService<TToken, TNode>  _compiler;
        public BaseLexicalPass(Scope scope, IEnumerable<ILexicalMatch<TToken, TNode>> matchers)
        {
            _matchers = matchers;
            _scope = scope;
            _compiler = scope.GetService<TToken, TNode>();
        }

        protected Scope _scope;

        public TNode Parse(string text, Dictionary<string, SourceSpan> annotations)
        {
            var tokens = _compiler.ParseTokens(text).ToArray();
            var result = transformTokens(tokens, 0, tokens.Length, _matchers);

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

        private IEnumerable<TToken> transformTokens(TToken[] tokens, int begin, int end, IEnumerable<ILexicalMatch<TToken, TNode>> matchers)
        {
            for (int token = 0; token < end; token++)
            {
                IEnumerable<TToken> transformed = null;
                int                 consumed = 0;
                foreach (var matcher in matchers)
                {
                    transformed = matcher.transform(Range(tokens, token, end), _scope, out consumed);
                    if (transformed != null)
                        break;
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
