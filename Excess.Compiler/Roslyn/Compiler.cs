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
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynCompiler : CompilerBase<SyntaxToken, SyntaxNode>
    {
        public RoslynCompiler() : base(new RoslynLexicalAnalysis(), new SyntaxAnalysisBase<SyntaxNode>())
        {
        }

        public override ICompilerPass initialPass(string text)
        {
            return new LexicalPass(text);
        }

        public ExpressionSyntax CompileExpression(string expr)
        {
            var   pass   = new LexicalPass(expr);
            Scope scope  = new Scope();
            var   events = _lexical.produce();

            _events.schedule("lexical-pass", events);
            pass.Compile(_events, scope);

            return CSharp.ParseExpression(pass.NewText);
        }

        public static int GetSyntaxId(SyntaxNode node)
        {
            var annotation = node.GetAnnotations("xs-syntax-id").FirstOrDefault();
            if (annotation != null)
                return int.Parse(annotation.Data);

            return -1;
        }

        public static SyntaxNode SetSyntaxId(SyntaxNode node, int id)
        {
            return node
                .WithoutAnnotations("xs-syntax-id")
                .WithAdditionalAnnotations(new SyntaxAnnotation("xs-syntax-id", id.ToString()));
        }

    }
}
