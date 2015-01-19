using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Core
{
    public abstract class CompilerBase<TToken, TNode> : ICompiler<TToken, TNode>
    {
        protected ILexicalAnalysis<TToken> _lexical;
        protected ISyntaxAnalysis<TNode>   _sintaxis;
        protected CompilerPass             _stage = CompilerPass.Lexical;

        public CompilerBase(ILexicalAnalysis<TToken> lexical, ISyntaxAnalysis<TNode> sintaxis)
        {
            _lexical  = lexical;
            _sintaxis = sintaxis;
        }

        public ILexicalAnalysis<TToken> Lexical()
        {
            return _lexical;
        }

        public ISyntaxAnalysis<TNode> Syntaxis()
        {
            return _sintaxis;
        }

        public CompilerPassResult DoPass(CompilerPass pass)
        {
            switch (pass)
            {
                case CompilerPass.Lexical:
                {
                    var matchers = _lexical.consume();
                    if (matchers == null || !matchers.Any())
                        return CompilerPassResult.Fail;
                    
                    return LexicalPass(matchers);
                    }
                case CompilerPass.Syntactical:
                {
                    var matchers = _sintaxis.consume();
                    if (matchers == null || !matchers.Any())
                        return CompilerPassResult.Success; 
                    
                    return SyntacticalPass(matchers);
                 }
                default: throw new NotImplementedException();
            }
        }

        protected abstract CompilerPassResult SyntacticalPass(IEnumerable<ISyntacticalMatch<TNode>> matchers);

        protected abstract CompilerPassResult LexicalPass(IEnumerable<ILexicalMatch<TToken>> matchers);
    }
}
