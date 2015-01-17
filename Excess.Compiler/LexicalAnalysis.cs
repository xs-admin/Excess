using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public interface ILexicalMatch
    {
        ILexicalMatch tokens(params char[] anyOf);
        ILexicalMatch tokens(params string[] anyOf);
        ILexicalMatch tokens(params SyntaxKind[] anyOf);
        ILexicalMatch tokens(char[] anyOf, string named = null);
        ILexicalMatch tokens(string[] anyOf, string named = null);
        ILexicalMatch tokens(SyntaxKind[] anyOf, string named = null);
        ILexicalMatch tokens(Func<SyntaxToken, bool> anyOf, string named = null);

        ILexicalMatch optional(params char[] anyOf);
        ILexicalMatch optional(params string[] anyOf);
        ILexicalMatch optional(params SyntaxKind[] anyOf);
        ILexicalMatch optional(char[] anyOf, string named = null);
        ILexicalMatch optional(string[] anyOf, string named = null);
        ILexicalMatch optional(SyntaxKind[] anyOf, string named = null);
        ILexicalMatch optional(Func<SyntaxToken, bool> anyOf, string named = null);

        ILexicalMatch enclosed(char open, char close, string start = null, string end = null, string contents = null);
        ILexicalMatch enclosed(string open, string close, string start = null, string end = null, string contents = null);
        ILexicalMatch enclosed(SyntaxKind open, SyntaxKind close, string start = null, string end = null, string contents = null);
        ILexicalMatch enclosed(Func<SyntaxToken, bool> open, 
                               Func<SyntaxToken, bool> close, 
                               string start = null, string end = null, string contents = null);
        ILexicalMatch many(params char[] anyOf);
        ILexicalMatch many(params string[] anyOf);
        ILexicalMatch many(params SyntaxKind[] anyOf);
        ILexicalMatch many(char[] anyOf, string named = null);
        ILexicalMatch many(string[] anyOf, string named = null);
        ILexicalMatch many(SyntaxKind[] anyOf, string named = null);
        ILexicalMatch many(Func<SyntaxToken, bool> tokens, string named = null);

        ILexicalMatch manyOrNone(params char[] anyOf);
        ILexicalMatch manyOrNone(params string[] anyOf);
        ILexicalMatch manyOrNone(params SyntaxKind[] anyOf);
        ILexicalMatch manyOrNone(char[] anyOf, string named = null);
        ILexicalMatch manyOrNone(string[] anyOf, string named = null);
        ILexicalMatch manyOrNone(SyntaxKind[] anyOf, string named = null);
        ILexicalMatch manyOrNone(Func<SyntaxToken, bool> tokens, string named = null);

        ILexicalAnalysis then(Func<ILexicalMatchResult, IEnumerable<SyntaxToken>> handler);
        ILexicalAnalysis then(ILexicalTransform transform);
    }

    public interface ILexicalTransform
    {
        ILexicalTransform insert();
        ILexicalTransform replace();
        ILexicalTransform remove();
    }

    public interface ILexicalAnalysis
    {
        ILexicalMatch     match();
        ILexicalTransform transform();
        IEnumerable<ILexicalMatch> matches();
    }
}
