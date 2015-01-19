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

    public interface ILexicalMatchResult
    {
        dynamic context();
        void    context_set(string name, dynamic value);
    }

    public interface ILexicalMatch<TToken>
    {
        ILexicalMatch<TToken> tokens(params char[] anyOf);
        ILexicalMatch<TToken> tokens(params string[] anyOf);
        ILexicalMatch<TToken> tokens(char[] anyOf, string named = null);
        ILexicalMatch<TToken> tokens(string[] anyOf, string named = null);
        ILexicalMatch<TToken> tokens(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken> optional(params char[] anyOf);
        ILexicalMatch<TToken> optional(params string[] anyOf);
        ILexicalMatch<TToken> optional(char[] anyOf, string named = null);
        ILexicalMatch<TToken> optional(string[] anyOf, string named = null);
        ILexicalMatch<TToken> optional(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken> enclosed(char open, char close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken> enclosed(string open, string close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken> enclosed(Func<TToken, bool> open, 
                               Func<TToken, bool> close, 
                               string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken> many(params char[] anyOf);
        ILexicalMatch<TToken> many(params string[] anyOf);
        ILexicalMatch<TToken> many(char[] anyOf, string named = null);
        ILexicalMatch<TToken> many(string[] anyOf, string named = null);
        ILexicalMatch<TToken> many(Func<TToken, bool> tokens, string named = null);

        ILexicalMatch<TToken> manyOrNone(params char[] anyOf);
        ILexicalMatch<TToken> manyOrNone(params string[] anyOf);
        ILexicalMatch<TToken> manyOrNone(char[] anyOf, string named = null);
        ILexicalMatch<TToken> manyOrNone(string[] anyOf, string named = null);
        ILexicalMatch<TToken> manyOrNone(Func<TToken, bool> tokens, string named = null);

        ILexicalAnalysis<TToken> then(Func<IEnumerable<TToken>, ILexicalMatchResult, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken> then(ILexicalTransform<TToken> transform);

        IEnumerable<TToken> transform(IEnumerable<TToken> enumerable, out int consumed);
    }

    public interface ILexicalTransform<TToken>
    {
        ILexicalTransform<TToken> insert(string tokens, string before = null, string after = null);
        ILexicalTransform<TToken> replace(string named, string tokens);
        ILexicalTransform<TToken> remove(string named);

        IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult result);
    }

    public interface ILexicalAnalysis<TToken>
    {
        ILexicalMatch<TToken> match();
        ILexicalTransform<TToken> transform();

        IEnumerable<CompilerEvent> produce();
    }
}
