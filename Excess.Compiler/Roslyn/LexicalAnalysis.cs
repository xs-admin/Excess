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

    public class RoslynLexicalTransform : LexicalTransform<SyntaxToken, SyntaxNode>
    {
        public RoslynLexicalTransform()
        {
        }

        protected override IEnumerable<SyntaxToken> tokensFromString(string tokenString)
        {
            return RoslynCompiler.ParseTokens(tokenString);
        }

        protected override SyntaxToken markToken(SyntaxToken token, out string id)
        {
            id = RoslynCompiler.uniqueId();
            return markToken(token, id);
        }

        protected override SyntaxToken markToken(SyntaxToken token, string id)
        {
            return RoslynCompiler.SetLexicalId(token, id);
        }

        protected override SyntaxNode getParent(SyntaxNode current)
        {
            return current.Parent;
        }
    }

    public class RoslynLexicalMatch : BaseLexicalMatch<SyntaxToken, SyntaxNode>
    {
        public RoslynLexicalMatch(ILexicalAnalysis<SyntaxToken, SyntaxNode> lexical) :
            base(lexical)
        {
        }

        protected override bool isIdentifier(SyntaxToken token)
        {
            return RoslynCompiler.isLexicalIdentifier(token);
        }
    }

    public class RoslynLexicalAnalysis : LexicalAnalysis<SyntaxToken, SyntaxNode>
    {
        public override ILexicalTransform<SyntaxToken, SyntaxNode> transform()
        {
            return new RoslynLexicalTransform();
        }

        protected override ILexicalMatch<SyntaxToken, SyntaxNode> createMatch()
        {
            return new RoslynLexicalMatch(this);
        }

        public override IEnumerable<SyntaxToken> parseTokens(string tokenString)
        {
            var tokens = CSharp.ParseTokens(tokenString);
            foreach (var token in tokens)
            {
                if (token.CSharpKind() != SyntaxKind.EndOfFileToken)
                    yield return token;
            }
        }

        protected override SyntaxToken setLexicalId(SyntaxToken token, out string id)
        {
            return RoslynCompiler.SetLexicalId(token, out id);
        }
    }
}
