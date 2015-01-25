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
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    public class RoslynCompiler : CompilerBase<SyntaxToken, SyntaxNode>
    {
        public RoslynCompiler() : base(new RoslynLexicalAnalysis(), new RoslynSyntaxAnalysis())
        {
        }

        public override ICompilerPass initialPass(string text)
        {
            return new LexicalPass(text);
        }

        //out of interface methods, used for testing
        public ExpressionSyntax CompileExpression(string expr)
        {
            var   pass   = new LexicalPass(expr);
            Scope scope  = new Scope();
            var   events = _lexical.produce();

            _events.schedule("lexical-pass", events);

            pass.Compile(_events, scope);

            return CSharp.ParseExpression(pass.NewText);
        }

        public string ApplyLexicalPasss(string text)
        {
            Scope scope = new Scope();

            var pass = new LexicalPass(text);
            var events = _lexical.produce();

            _events.schedule("lexical-pass", events);

            pass.Compile(_events, scope);

            return pass.NewText;
        }

        public SyntaxTree ApplySyntacticalPasss(string text)
        {
            Scope scope = new Scope();

            var lexicalPass   = new LexicalPass(text);
            var lexicalEvents = _lexical.produce();

            _events.schedule("lexical-pass", lexicalEvents);

            var syntacticalPass   = lexicalPass.Compile(_events, scope);
            var syntacticalEvents = _sintaxis.produce();

            _events.schedule("syntactical-pass", syntacticalEvents);
            syntacticalPass.Compile(_events, scope);

            return ((SyntacticalPass)syntacticalPass).Tree;
        }

        //high level utils
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

        public static int GetLexicalId(SyntaxToken token)
        {
            var annotation = token.GetAnnotations("xs-lexical-id").FirstOrDefault();
            if (annotation != null)
                return int.Parse(annotation.Data);

            return -1;
        }

        public static SyntaxToken SetLexicalId(SyntaxToken token, int id)
        {
            return token
                .WithoutAnnotations("xs-lexical-id")
                .WithAdditionalAnnotations(new SyntaxAnnotation("xs-lexical-id", id.ToString()));
        }

        public static SyntaxToken MarkToken(SyntaxToken token, string mark, object value)
        {
            var result = value == null ? new SyntaxAnnotation(mark) :
                                         new SyntaxAnnotation(mark, value.ToString());

            return token
                .WithoutAnnotations(mark)
                .WithAdditionalAnnotations(result);
        }

        public static IEnumerable<SyntaxToken> ParseTokens(string text)
        {
            var tokens = CSharp.ParseTokens(text);
            foreach (var token in tokens)
            {
                if (token.CSharpKind() != SyntaxKind.EndOfFileToken)
                    yield return token;
            }
        }

        public static string TokensToString(IEnumerable<SyntaxToken> tokens)
        {
            StringBuilder result = new StringBuilder();
            foreach (var token in tokens)
                result.Append(token.ToFullString());

            return result.ToString();
        }

        public static BlockSyntax ParseCode(IEnumerable<SyntaxToken> tokens)
        {
            string code = TokensToString(tokens); //td: mapping
            return (BlockSyntax)CSharp.ParseStatement("{" + code + "}");
        }

        public static ParameterListSyntax ParseParameterList(IEnumerable<SyntaxToken> parameters)
        {
            string parameterString = TokensToString(parameters); //td: mapping
            return CSharp.ParseParameterList(parameterString);
        }
    }
}
