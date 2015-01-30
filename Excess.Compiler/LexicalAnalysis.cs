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
        MatchAndStay,
    }

    public interface ILexicalMatch<TToken, TNode, TModel>
    {
        ILexicalMatch<TToken, TNode, TModel> token(char token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> token(string token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> token(Func<TToken, bool> matcher, string named = null);

        ILexicalMatch<TToken, TNode, TModel> any(params char[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> any(params string[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> any(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> any(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> any(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken, TNode, TModel> optional(params char[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> optional(params string[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> optional(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> optional(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> optional(Func<TToken, bool> anyOf, string named = null);

        ILexicalMatch<TToken, TNode, TModel> enclosed(char open, char close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode, TModel> enclosed(string open, string close, string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode, TModel> enclosed(Func<TToken, bool> open, 
                               Func<TToken, bool> close, 
                               string start = null, string end = null, string contents = null);
        ILexicalMatch<TToken, TNode, TModel> many(params char[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> many(params string[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> many(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> many(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> many(Func<TToken, bool> tokens, string named = null);

        ILexicalMatch<TToken, TNode, TModel> manyOrNone(params char[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> manyOrNone(params string[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> manyOrNone(char[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> manyOrNone(string[] anyOf, string named = null);
        ILexicalMatch<TToken, TNode, TModel> manyOrNone(Func<TToken, bool> tokens, string named = null);

        ILexicalMatch<TToken, TNode, TModel> identifier(string named = null, bool optional = false);

        ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(ILexicalTransform<TToken, TNode> transform);
    }

    public interface ILexicalTransform<TToken, TNode>
    {
        ILexicalTransform<TToken, TNode> insert(string tokens, string before = null, string after = null);
        ILexicalTransform<TToken, TNode> replace(string named, string tokens);
        ILexicalTransform<TToken, TNode> remove(string named);

        ILexicalTransform<TToken, TNode> then(string named, Func<TNode, TNode> handler);
        ILexicalTransform<TToken, TNode> then(string named, Func<TNode, Scope, TNode> handler);
        ILexicalTransform<TToken, TNode> then(string named, ISyntaxTransform<TNode> transform);

        IEnumerable<TToken> transform(IEnumerable<TToken> tokens, Scope result);
    }

    public enum ExtensionKind
    {
        Expression,
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

    public interface ILexicalAnalysis<TToken, TNode, TModel>
    {
        ILexicalMatch<TToken, TNode, TModel> match(); 
        ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<IEnumerable<TToken>, Scope, LexicalExtension<TToken>, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler);
        ILexicalTransform<TToken, TNode> transform();
    }
}
