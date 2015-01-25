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

        [TestMethod]
        public void SyntacticalMatching()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var sintaxis = compiler.Sintaxis();

            //simple match
            sintaxis
                .match<ClassDeclarationSyntax>(c => !c.Members.OfType<ConstructorDeclarationSyntax>().Any())
                    .then(sintaxis.transform(addConstructor));

            var tree = compiler.ApplySyntacticalPasss("class foo { } class bar { bar() {} }");

            Assert.IsTrue(tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Count() == 2); //must have added a constructor to "foo"

            //scope match & transform
            sintaxis
                .match<ClassDeclarationSyntax>(c => c.Identifier.ToString() == "foo")
                    .descendants<MethodDeclarationSyntax>(named: "methods")
                    .descendants<PropertyDeclarationSyntax>(prop => prop.Identifier.ToString().StartsWith("my"), named: "myProps")
                .then(sintaxis.transform()
                    .replace("methods", method => ((MethodDeclarationSyntax)method)
                        .WithIdentifier(CSharp.ParseToken("my" + ((MethodDeclarationSyntax)method).Identifier.ToString())))
                    .remove("myProps"));


            var scopeTree = compiler.ApplySyntacticalPasss("class foo { public void Method() {} int myProp {get; set;} }");
            Assert.IsTrue(scopeTree.ToString() == "class foo { public void myMethod() {} foo (){}}");

            Assert.IsTrue(scopeTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Count() == 1); //must have added a constructor to "foo", since the sintaxis is the same
        }

    private static SyntaxNode addConstructor(SyntaxNode node)
        {
            var classDeclaration = node as ClassDeclarationSyntax;
            Assert.IsTrue(classDeclaration != null);

            return classDeclaration
                .AddMembers(CSharp.ConstructorDeclaration(classDeclaration.Identifier)
                    .WithBody(CSharp.Block()));
        }
    }
}
