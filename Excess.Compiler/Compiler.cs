using System.Collections.Generic;

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

    public interface ICompilerInjector<TToken, TNode, TModel>
    {
        void apply(ICompiler<TToken, TNode, TModel> compiler);
    }

    public interface ICompiler<TToken, TNode, TModel>
    {
        ILexicalAnalysis<TToken, TNode, TModel> Lexical();
        ISyntaxAnalysis<TToken, TNode, TModel> Syntax();
        ISemanticAnalysis<TToken, TNode, TModel> Semantics();
        IInstanceAnalisys<TNode> Instance();

        Scope Scope { get; }

        bool Compile(string text, CompilerStage stage = CompilerStage.Started);
        bool CompileAll(string text);
        bool Advance(CompilerStage stage);
    }

    public interface ICompilerService<TToken, TNode, TModel>
    {
        string TokenToString(TToken token, out string xsId, out string mappingId);
        string TokenToString(TToken token, out string xsId);
        string TokenToString(TToken token, out int xsId);
        string TokenToString(TToken token);
        TToken MarkToken(TToken token, out int xsId);
        TToken MarkToken(TToken token);
        TNode MarkNode(TNode node, out int xsId);
        TNode MarkNode(TNode node);
        TToken InitToken(TToken token, int xsId);
        TNode MarkTree(TNode node);
        int GetExcessId(TToken token);
        int GetExcessId(TNode node);
        bool isIdentifier(TToken token);
        int GetOffset(TToken token);

        IEnumerable<TToken> ParseTokens(string text);
        TNode Parse(string text);
        IEnumerable<TToken> MarkTokens(IEnumerable<TToken> tokens, out int xsId);
        IEnumerable<TNode> Find(TNode node, IEnumerable<string> xsIds);
        TNode Find(TNode node, SourceSpan value);
        IEnumerable<TToken> NodeTokens(TNode node);
        int TokenOffset(TToken token);

        TNode ParseCodeFromTokens(IEnumerable<TToken> tokens);
        TNode ParseParamListFromTokens(IEnumerable<TToken> tokens);
        TNode ParseMembersFromTokens(IEnumerable<TToken> tokens);
        TNode ParseArgumentListFromTokens(IEnumerable<TToken> tokens); 
    }
}
