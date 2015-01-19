using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler
{
    public enum CompilerPass
    {
        Lexical,
        Syntactical,
        Semantical,
    }

    public enum CompilerPassResult
    {
        Success,
        Fail,
        NotFinished,
    }

    public interface ICompiler<TToken, TNode>
    {
        ILexicalAnalysis<TToken> Lexical();
        ISyntaxAnalysis<TNode> Syntaxis();

        CompilerPassResult DoPass(CompilerPass pass);
    }
}
