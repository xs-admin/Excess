using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Excess.Compiler
{
    public enum TokenMatch
    {
        Match,
        UnMatch,
        MatchAndContinue,
    }

    public interface ILexicalMatchResult<TToken>
    {
        Scope Scope { get; set; }
        IEventBus Events { get; set; }
        IEnumerable<TToken> Tokens { get; set; }

        dynamic context();
    }

    public interface ILexicalMatch<TToken, TNode>
    {
        ILexicalMatch<TToken, TNode> token(char token, string named = null);
        ILexicalMatch<TToken, TNode> token(string token, string named = null);
        ILexicalMatch<TToken, TNode> token(Func<TToken, bool> matcher, string named = null);

        ILexicalMatch<TToken, TNode> any(params char[] anyOf);
        ILexicalMatch<TToken, TNode> any(params string[] anyOf);
        ILexicalMatch<TToken, TNode> any(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> any(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> any(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken, TNode> optional(params char[] anyOf);
        ILexicalMatch<TToken, TNode> optional(params string[] anyOf);
        ILexicalMatch<TToken, TNode> optional(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> optional(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> optional(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken, TNode> enclosed(char open, char close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode> enclosed(string open, string close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode> enclosed(Func<TToken, bool> open, 
                               Func<TToken, bool> close, 
                               string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode> many(params char[] anyOf);
        ILexicalMatch<TToken, TNode> many(params string[] anyOf);
        ILexicalMatch<TToken, TNode> many(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> many(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> many(Func<TToken, bool> tokens, string named = null);

        ILexicalMatch<TToken, TNode> manyOrNone(params char[] anyOf);
        ILexicalMatch<TToken, TNode> manyOrNone(params string[] anyOf);
        ILexicalMatch<TToken, TNode> manyOrNone(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> manyOrNone(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode> manyOrNone(Func<TToken, bool> tokens, string named = null);

        ILexicalAnalysis<TToken, TNode> then(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode> then(ILexicalTransform<TToken> transform);

        IEnumerable<TToken> transform(IEnumerable<TToken> enumerable, ILexicalMatchResult<TToken> result, out int consumed);
    }

    public interface ILexicalTransform<TToken>
    {
        ILexicalTransform<TToken> insert(string tokens, string before = null, string after = null);
        ILexicalTransform<TToken> replace(string named, string tokens);
        ILexicalTransform<TToken> remove(string named);

        IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken> result);
    }

    public enum ExtensionKind
    {
        Code,
        Member,
        Type,
        Modifier
    }

    public class LexicalExtension<TToken>
    {
        public ExtensionKind       Kind       { get; set; }
        public TToken              Keyword    { get; set; }
        public TToken              Identifier { get; set; }
        public IEnumerable<TToken> Arguments  { get; set; }
        public IEnumerable<TToken> Body       { get; set; }
    }

    public interface ILexicalAnalysis<TToken, TNode>
    {
        ILexicalMatch<TToken, TNode> match(); 
        ILexicalAnalysis<TToken, TNode> extension(string keyword, ExtensionKind kind, Func<LexicalExtension<TToken>, ILexicalMatchResult<TToken>, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode> extension(string keyword, ExtensionKind kind, Func<ISyntacticalMatchResult<TNode>, LexicalExtension<TToken>, TNode> handler);
        ILexicalTransform<TToken> transform();

        IEnumerable<CompilerEvent> produce();

        IEnumerable<TToken> parseTokens(string tokens);
    }
}
