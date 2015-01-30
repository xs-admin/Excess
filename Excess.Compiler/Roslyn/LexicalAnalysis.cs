using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Excess.Compiler.Core;

namespace Excess.Compiler.Roslyn
{
    using Microsoft.CodeAnalysis.CSharp;
    using System.Diagnostics;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynLexicalMatch : BaseLexicalMatch<SyntaxToken, SyntaxNode, SemanticModel>
    {
        public RoslynLexicalMatch(ILexicalAnalysis<SyntaxToken, SyntaxNode, SemanticModel> lexical) :
            base(lexical)
        {
        }

        protected override bool isIdentifier(SyntaxToken token)
        {
            return RoslynCompiler.isLexicalIdentifier(token);
        }
    }

    public class RoslynLexicalAnalysis : LexicalAnalysis<SyntaxToken, SyntaxNode, SemanticModel>
    {
        protected override ILexicalMatch<SyntaxToken, SyntaxNode, SemanticModel> createMatch()
        {
            return new RoslynLexicalMatch(this);
        }

        public override ILexicalTransform<SyntaxToken, SyntaxNode> transform()
        {
            throw new NotImplementedException();
        }

    }
}
