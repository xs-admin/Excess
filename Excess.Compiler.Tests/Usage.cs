using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class Usage
    {
        [TestMethod]
        public void LexicalTokenMatching()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var lexical = compiler.Lexical();
            lexical
                .match()
                    .any('(', '=', ',')
                    .token("function", named: "fn")
                    .enclosed('(', ')')
                    .token('{', named: "brace")
                    .then(compiler.Lexical().transform()
                        .remove("fn")
                        .insert("=>", before: "brace"))
                .match()
                    .any(new[] { '(', '=', ',' }, named: "start")
                    .enclosed('[', ']', start: "open", end: "close")
                    .then(compiler.Lexical().transform()
                        .insert("new []", after: "start")
                        .replace("open",  "{")
                        .replace("close", "}"));

            var events  = lexical.produce();
            int evCount = events.Count();

            Assert.IsTrue(evCount == 1);

            ExpressionSyntax exprFunction = compiler.CompileExpression("call(10, function(x, y) {})");
            Assert.IsTrue(exprFunction.DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Any());

            ExpressionSyntax exprArray = compiler.CompileExpression("call([1, 2, 3], 4, [5, 6, 7])");
            Assert.IsTrue(exprArray.DescendantNodes()
                .OfType<ImplicitArrayCreationExpressionSyntax>()
                .Count() == 2);
        }

        [TestMethod]
        public void LexicalExtension()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var lexical = compiler.Lexical();
            lexical
                .extension("my_ext", ExtensionKind.Code, myExtLexical);

            string lexicalResult = compiler.ApplyLexicalPasss("my_ext(int i) { code(); }");

            Assert.IsTrue(lexicalResult == "my_ext_replaced int i = code(); ");
        }

        private IEnumerable<SyntaxToken> myExtLexical(LexicalExtension<SyntaxToken> extension, ILexicalMatchResult<SyntaxToken> result)
        {
            string testResult = "my_ext_replaced "
                              + RoslynCompiler.TokensToString(extension.Arguments)
                              + " = "
                              + RoslynCompiler.TokensToString(extension.Body);

            return RoslynCompiler.ParseTokens(testResult);
        }

        [TestMethod]
        public void SyntacticalExtension()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var lexical = compiler.Lexical();
            lexical
                .extension("my_ext", ExtensionKind.Code, myExtSyntactical);

            var tree = compiler.ApplySyntacticalPasss("void main() { my_ext(int i) { code(); } }");
            Assert.IsTrue(tree.ToString() == "void main() { my_ext(int i=>{code(); });}");
        }

        private SyntaxNode myExtSyntactical(ISyntacticalMatchResult<SyntaxNode> result, LexicalExtension<SyntaxToken> extension)
        {
            Assert.IsTrue(result.Node is ExpressionStatementSyntax);
            var node = result.Node as ExpressionStatementSyntax;

            Assert.IsTrue(node.ToString() == "__extension();");

            var call = (node.Expression as InvocationExpressionSyntax)
                .WithExpression(CSharp.ParseExpression("my_ext"))
                .WithArgumentList(CSharp.ArgumentList(CSharp.SeparatedList(new[] {
                    CSharp.Argument(CSharp.ParenthesizedLambdaExpression(
                        parameterList: RoslynCompiler.ParseParameterList(extension.Arguments), 
                        body:          RoslynCompiler.ParseCode(extension.Body)))})));
            
            return node.WithExpression(call);
        }
    }
}
