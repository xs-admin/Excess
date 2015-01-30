using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public enum CompilerStage
    {
        Started,
        Lexical,
        Syntactical,
        Semantical,
        Finished,
    }

    public interface ICompiler<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> Lexical();
        ISyntaxAnalysis<TToken, TNode, TModel> Sintaxis();

        bool Compile(string text, CompilerStage stage = CompilerStage.Started);
        bool CompileAll(string text);
        bool Advance(CompilerStage stage);
    }

    public interface ICompilerService<TToken, TNode, TModel>
    {
        string TokenToString(TToken token, out string xsId);
        string TokenToString(TToken token, out int xsId);
        string TokenToString(TToken token);
        TToken MarkToken(TToken token, out int xsId);
        TToken MarkToken(TToken token);
        TNode MarkNode(TNode node, out int xsId);
        TNode MarkNode(TNode node);
        TNode MarkTree(TNode node);
        int GetExcessId(TToken token);
        int GetExcessId(TNode node);

        IEnumerable<TToken> ParseTokens(string text);
        TNode Parse(string text);
        IEnumerable<TToken> MarkTokens(IEnumerable<TToken> tokens, out int xsId);
    }
}
