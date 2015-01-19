using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class LexicalPass : BaseLexicalPass<SyntaxToken>
    {
        static LexicalPass()
        {
            PassId    = "lexical-transform";
            PassStage = CompilerStage.Lexical;
        }

        public LexicalPass(string text) :
            base(text)
        {
        }

        protected override ICompilerPass continuation(string transformed)
        {
            var root = CSharp.ParseCompilationUnit(transformed);
            throw new NotImplementedException(); 
        }

        protected override IEnumerable<SyntaxToken> parseTokens(string text)
        {
            return CSharp.ParseTokens(text);
        }
    }
}
