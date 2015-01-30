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

    public interface ICompilerPass
    {
        string Id { get; }
        CompilerStage Stage { get; }

        ICompilerPass Compile(IEventBus events, Scope scope);
    }

    public interface ICompiler<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> Lexical();
        ISyntaxAnalysis<TToken, TNode, TModel> Sintaxis();

        ICompilerPass Compile(string text, CompilerStage stage = CompilerStage.Started);
        ICompilerPass CompileAll(string text);
        ICompilerPass Advance(CompilerStage stage);
    }

    public interface ICompilerService<TToken, TNode>
    {
        string TokenToString(TToken token, out string xsId);
        string TokenToString(TToken token);
        TToken MarkToken(TToken token, out int xsId);
        TToken MarkToken(TToken token, int xsId);
        IEnumerable<TToken> ParseTokens(string text);
        TNode MarkNode(TNode node, out int xsId);
        TNode MarkNode(TNode node, string xsId);
        TNode MarkTree(TNode node);
        int GetExcessId(TToken token);
        int GetExcessId(TNode node);
        TNode Parse(string text);
    }
}
