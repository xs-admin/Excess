using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Diagnostics;
using Excess.Compiler.Core;

namespace Excess.Compiler
{
    public class TokenSpan
    {
        public TokenSpan(int start, int length)
        {
            Start  = start;
            Length = length;
        }

        public int Start { get; set; }
        public int Length { get; set; }
    }

    public class LexicalMatchItem
    {
        public LexicalMatchItem(TokenSpan span, string identifier, bool literal = false)
        {
            Span = span;
            Identifier = identifier;
            Literal = false;
        }

        public TokenSpan Span { get; set; }
        public string Identifier { get; set; }
        public bool Literal { get; set; }
    }

    public interface ILexicalMatchResult<TToken, TNode, TModel>
    {
        int Consumed { get; }
        IEnumerable<LexicalMatchItem> Items { get; }
        ILexicalTransform<TToken, TNode, TModel> Transform { get; set; }
        IEnumerable<TToken> GetTokens(IEnumerable<TToken> tokens, TokenSpan span);
    }

    public interface ILexicalMatch<TToken, TNode, TModel>
    {
        ILexicalMatch<TToken, TNode, TModel> token(char token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> token(string token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> token(Func<TToken, bool> matcher, string named = null);

        ILexicalMatch<TToken, TNode, TModel> any(params char[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> any(params string[] anyOf);
        ILexicalMatch<TToken, TNode, TModel> any(char[] anyOf, string named = null, bool matchDocumentStart = false);
        ILexicalMatch<TToken, TNode, TModel> any(string[] anyOf, string named = null, bool matchDocumentStart = false);
        ILexicalMatch<TToken, TNode, TModel> any(Func<TToken, bool> anyOf, string named = null, bool matchDocumentStart = false);

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

        ILexicalMatch<TToken, TNode, TModel> until(char token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> until(string token, string named = null);
        ILexicalMatch<TToken, TNode, TModel> until(Func<TToken, bool> matcher, string named = null);

        ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, Scope, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(Func<IEnumerable<TToken>, ILexicalMatchResult<TToken, TNode, TModel>, Scope, IEnumerable<TToken>> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(ILexicalTransform<TToken, TNode, TModel> transform);

        ILexicalMatchResult<TToken, TNode, TModel> match(IEnumerable<TToken> tokens, Scope scope, bool isDocumentStart);
    }

    public interface ILexicalTransform<TToken, TNode, TModel>
    {
        ILexicalTransform<TToken, TNode, TModel> insert(string tokens, string before = null, string after = null);
        ILexicalTransform<TToken, TNode, TModel> replace(string named, string tokens);
        ILexicalTransform<TToken, TNode, TModel> remove(string named);

        ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode> handler, string referenceToken = null);
        ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler, string referenceToken = null);
        ILexicalTransform<TToken, TNode, TModel> then(Func<TNode, TNode, TModel, Scope, TNode> handler, string referenceToken = null);

        IEnumerable<TToken> transform(IEnumerable<TToken> tokens, ILexicalMatchResult<TToken, TNode, TModel> match, Scope scope);
    }

    public enum ExtensionKind
    {
        None,
        Expression,
        Code,
        Member,
        MemberType,
        Type,
        TypeCode,
        Modifier //td: !!
    }

    public class LexicalExtension<TToken>
    {
        public ExtensionKind       Kind       { get; set; }
        public TToken              Keyword    { get; set; }
        public TToken              Identifier { get; set; }
        public IEnumerable<TToken> Arguments  { get; set; }
        public IEnumerable<TToken> Body       { get; set; }
        public int                 BodyStart  { get; set; }
    }

    public interface INormalizer<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> with(Func<TNode, IEnumerable<TNode>, Scope, TNode> statements = null,
                                                     Func<TNode, IEnumerable<TNode>, Scope, TNode> members = null,
                                                     Func<TNode, IEnumerable<TNode>, Scope, TNode> types = null,
                                                     Func<TNode, Scope, TNode> then = null);

        ILexicalAnalysis<TToken, TNode, TModel> statements(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> members(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> types(Func<TNode, IEnumerable<TNode>, Scope, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> then(Func<TNode, Scope, TNode> handler);
    }

    public interface ILexicalAnalysis<TToken, TNode, TModel>
    {
        ILexicalMatch<TToken, TNode, TModel> match();
        ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<TNode, Scope, LexicalExtension<TToken>, TNode> handler);
        ILexicalAnalysis<TToken, TNode, TModel> extension(string keyword, ExtensionKind kind, Func<IEnumerable<TToken>, Scope, LexicalExtension<TToken>, IEnumerable<TToken>> handler);
        IGrammarAnalysis<GNode, TToken, TNode> grammar<TGrammar, GNode>(string keyword, ExtensionKind kind) where TGrammar : IGrammar<TToken, TNode, GNode>, new();
        IGrammarAnalysis<GNode, TToken, TNode> grammar<TGrammar, GNode>(string keyword, ExtensionKind kind, TGrammar grammar) where TGrammar : IGrammar<TToken, TNode, GNode>;

        //indented grammars
        IIndentationGrammarAnalysis<TToken, TNode, GNode> indented<GNode, GRoot>(
            string keyword, 
            ExtensionKind kind,
            Action<GRoot, LexicalExtension<TToken>> init) where GRoot : GNode, new();

        //deprecated
        INormalizer<TToken, TNode, TModel> normalize();

        //transforms
        ILexicalTransform<TToken, TNode, TModel> transform();
    }
}
