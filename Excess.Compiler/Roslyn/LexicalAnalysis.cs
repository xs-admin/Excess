using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynLexicalTransform : LexicalTransform<SyntaxToken>
    {
        protected override IEnumerable<SyntaxToken> tokensFromString(string tokenString)
        {
            return CSharp.ParseTokens(tokenString);
        }
    }

    public class RoslynLexicalAnalysis : LexicalAnalysis<SyntaxToken>
    {
        public override ILexicalTransform<SyntaxToken> transform()
        {
            return new RoslynLexicalTransform();
        }
    }
}
