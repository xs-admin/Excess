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
        public Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode> Handler { get; set; }
    }

    public abstract class BaseLexicalPass<TToken, TNode> : BasePass
    {
        string _text;
        public BaseLexicalPass(string text)
        {
            _text = text;
        }

        protected Scope _scope;
        protected IEventBus _events;

        public override ICompilerPass Compile(IEventBus events, Scope scope)
        {
            _scope = scope;
            _events = events;

            var myEvents = events.poll(passId());
            var matchEvents = myEvents.OfType<LexicalMatchEvent<TToken,TNode>>();

            var tokens   = parseTokens(_text).ToArray();
            var matchers = GetMatchers(matchEvents);
            IEnumerable<TToken> result = transformTokens(tokens, 0, tokens.Length, matchers);

            //calculate new text
            //td: !! mapping info

            Dictionary<string, SourceSpan> pending = new Dictionary<string, SourceSpan>();

            StringBuilder newText = new StringBuilder();
            string        currId  = null;
            foreach (var token in result)
            {
                string lexicalId;
                string toInsert = tokenToString(token, out lexicalId);

                //store the actual position in the transformed stream of any tokens pending processing
                if (lexicalId != currId)
                {
                    if (currId == null) //start sequence
                        pending[lexicalId] = new SourceSpan(newText.Length, toInsert.Length);

                    currId = lexicalId;
                }
                else if (lexicalId != null)
                {
                    //augment span
                    SourceSpan span = pending[lexicalId];
                    span.Length += toInsert.Length;
                }
                else
                    currId = null;

                newText.Append(toInsert);
            }

            var pendingExtensions = new List<PendingExtension<TToken, TNode>>();
            if (pending.Any())
            {
                events.poll<LexicalExtensionEvent<TToken, TNode>>().All(ev =>
                {
                    SourceSpan span = pending[ev.LexicalId];
                    pendingExtensions.Add(new PendingExtension<TToken, TNode>
                    {
                        Span = span,
                        Extension = ev.Extension,
                        Handler = ev.Handler,
                    });

                    return true;
                });
            }

            var pendingTransforms = new Dictionary<SourceSpan, ISyntaxTransform<TNode>>();
            if (pending.Any())
            {
                events.poll<LexicalSyntaxTransformEvent<TNode>>().All(ev =>
                {
                    SourceSpan span = pending[ev.LexicalId];
                    pendingTransforms[span] = ev.Transform;
                    return true;
                });
            }

            return continuation(events, scope, newText.ToString(), pendingExtensions, pendingTransforms);
        }

        protected abstract string tokenToString(TToken token, out string lexicalId);

        protected abstract ICompilerPass continuation(IEventBus events, Scope scope, string transformed, IEnumerable<PendingExtension<TToken, TNode>> extensions, IDictionary<SourceSpan, ISyntaxTransform<TNode>> transforms);

        private IEnumerable<ILexicalMatch<TToken, TNode>> GetMatchers(IEnumerable<LexicalMatchEvent<TToken, TNode>> events)
        {
            foreach (var ev in events)
            {
                foreach (var matcher in ev.Matchers)
                    yield return matcher;
            }
        }

        protected abstract IEnumerable<TToken> parseTokens(string text);

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
                ILexicalMatchResult<TToken, TNode> result = new LexicalMatchResult<TToken, TNode>(new Scope(), _events);
                foreach (var matcher in matchers)
                {
                    transformed = matcher.transform(Range(tokens, token, end), result, out consumed);
                    if (transformed != null)
                        break;
                }

                if (transformed == null)
                    yield return tokens[token];
                else
                {
                    if (result.SyntacticalTransform != null)
                    {
                        string markId;
                        transformed = markTokens(transformed, out markId);
                        _events.schedule(new LexicalSyntaxTransformEvent<TNode>(markId, result.SyntacticalTransform));
                    }

                    foreach (var tt in transformed)
                        yield return tt;

                    token += consumed - 1;
                }
            }
        }

        protected abstract IEnumerable<TToken> markTokens(IEnumerable<TToken> transformed, out string id);
    }
}
