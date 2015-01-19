using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Excess.Compiler.Roslyn
{
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class Compiler : CompilerBase<SyntaxToken, SyntaxNode>
    {
        public Compiler() : base(new RoslynLexicalAnalysis(), new SyntaxAnalysisBase<SyntaxNode>())
        {
        }

        public override ICompilerPass initialPass(string text)
        {
            return new LexicalPass(text);
        }

        //SyntaxTree _tree;
        //protected override CompilerPassResult SyntacticalPass(IEnumerable<ISyntacticalMatch<SyntaxNode>> matchers)
        //{
        //    if (_tree == null)
        //    {
        //        Debug.Assert(_tokens != null);
        //        _tree = ParseTokens(_tokens);
        //    }

        //    SyntaxRewriter pass = new SyntaxRewriter(matchers);
        //    var transformed = pass.Visit(_tree.GetRoot());
        //    _tree = transformed.SyntaxTree;
        //    return CompilerPassResult.Success;
        //}

        //private SyntaxTree ParseTokens(IEnumerable<SyntaxToken> tokens)
        //{
        //    StringBuilder newText = new StringBuilder();
        //    foreach (var token in tokens)
        //        newText.Append(token.ToFullString());

        //    return CSharp.ParseCompilationUnit(newText.ToString()).SyntaxTree;
        //}
    }
}
