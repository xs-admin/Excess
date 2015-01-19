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

        ICompilerPass Compile(EventBus events, Scope scope);
    }

    public interface ICompiler<TToken, TNode>
    {
        ILexicalAnalysis<TToken> Lexical();
        ISyntaxAnalysis<TNode> Syntaxis();

        ICompilerPass Compile(string text, CompilerStage stage = CompilerStage.Started);
        ICompilerPass CompileAll(string text);
        ICompilerPass Advance(CompilerStage stage);
    }

}
