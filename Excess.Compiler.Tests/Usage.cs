using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Core;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Excess.Compiler.XS;

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

            string lexicalResult = compiler.ApplyLexicalPass("my_ext(int i) { code(); }");

            Assert.IsTrue(lexicalResult == "my_ext_replaced int i = code(); ");
        }

        private IEnumerable<SyntaxToken> myExtLexical(LexicalExtension<SyntaxToken> extension, ILexicalMatchResult<SyntaxToken, SyntaxNode> result)
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

            var tree = compiler.ApplySyntacticalPass("void main() { my_ext(int i) { code(); } }");
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

            var tree = compiler.ApplySyntacticalPass("class foo { } class bar { bar() {} }");

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

              
            var scopeTree = compiler.ApplySyntacticalPass("class foo { public void Method() {} int myProp {get; set;} }");
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

        [TestMethod]
        public void SyntacticalExtensions()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var sintaxis = compiler.Sintaxis();

            SyntaxTree tree;

            //code extension
            sintaxis
                .extension("codeExtension", ExtensionKind.Code, codeExtension);

            tree = compiler.ApplySyntacticalPass("class foo { void bar() {var v = codeExtension(param: \"foobar\") {bar();}} }");
            Assert.IsTrue(tree.ToString() == "class foo { public void myMethod() {} foo (){}}");

            //member extension
            sintaxis
                .extension("memberExtension", ExtensionKind.Member, memberExtension);

            tree = compiler.ApplySyntacticalPass("class foo { memberExtension(param: \"foobar\") {int x = 3;} }");
            Assert.IsTrue(tree.ToString() == "class foo { public void myMethod() {} foo (){}}");

            //type extension
            sintaxis
                .extension("typeExtension", ExtensionKind.Type, typeExtension)
                .extension("typeCodeExtension", ExtensionKind.Type, typeCodeExtension);

            tree = compiler.ApplySyntacticalPass("public typeExtension foo(param: \"foobar\") { void bar() {} }");
            Assert.IsTrue(tree.ToString() == "class foo { public void myMethod() {} foo (){}}");

            tree = compiler.ApplySyntacticalPass("typeCodeExtension(param: \"foobar\") { int i = 0; }");
            Assert.IsTrue(tree.ToString() == "class foo { public void myMethod() {} foo (){}}");
        }

        private IEnumerable<SyntaxNode> typeCodeExtension(ISyntacticalMatchResult<SyntaxNode> arg1, SyntacticalExtension<SyntaxNode> arg2)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<SyntaxNode> typeExtension(ISyntacticalMatchResult<SyntaxNode> arg1, SyntacticalExtension<SyntaxNode> arg2)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<SyntaxNode> memberExtension(ISyntacticalMatchResult<SyntaxNode> arg1, SyntacticalExtension<SyntaxNode> arg2)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<SyntaxNode> codeExtension(ISyntacticalMatchResult<SyntaxNode> arg1, SyntacticalExtension<SyntaxNode> arg2)
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void XSFunctions()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            XSModule.Apply(compiler);

            //as lambda
            ExpressionSyntax exprFunction = compiler.CompileExpression("call(10, function(x, y) {})");
            Assert.IsTrue(exprFunction.DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Any());

            //as typed method
            string result = compiler.ApplyLexicalPass("class foo { public int function bar() {}}");
            Assert.IsTrue(result == "class foo { public int bar() {}}");

            SyntaxTree tree = null;
            string     text = null;
            
            //as untyped method
            tree = compiler.ApplySyntacticalPass("class foo { public function bar() {}}", out text);
            Assert.IsTrue(text == "class foo\r\n{\r\n    public void bar()\r\n    {\r\n    }\r\n}");
        }
    }
}
