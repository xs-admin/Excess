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

            tree = compiler.ApplySyntacticalPass("class foo { void bar() {codeExtension() {bar();}} }");
            Assert.IsTrue(tree
                .GetRoot()
                .DescendantNodes()
                .OfType<StatementSyntax>()
                .Count() == 5); //must have added a couple of statements

            tree = compiler.ApplySyntacticalPass("class foo { void bar() {var ce = codeExtension() {bar();}} }");
            var localDeclStatement = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(localDeclStatement);
            Assert.AreEqual(localDeclStatement.ToString(), "var ce = bar(7);");

            tree = compiler.ApplySyntacticalPass("class foo { void bar() {ce = codeExtension() {bar();}} }");
            var assignmentStatement = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(assignmentStatement);
            Assert.AreEqual(assignmentStatement.ToString(), "ce = bar(7);");

            //member extension
            sintaxis
                .extension("memberExtension", ExtensionKind.Member, memberExtension);

            tree = compiler.ApplySyntacticalPass("class foo { memberExtension(param: \"foobar\") {int x = 3;} }");
            var method = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(method);
            Assert.AreEqual(method.ParameterList.Parameters.Count, 0);
            Assert.AreEqual(method.Body.Statements.Count, 3);

            //type extension
            sintaxis
                .extension("typeExtension", ExtensionKind.Type, typeExtension);

            tree = compiler.ApplySyntacticalPass("public typeExtension foo(param: \"foobar\") { bar(); }");

            var @class = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(@class);
            Assert.AreEqual(@class.Identifier.ToString(), "foo");
            var classMethod = @class
                .Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(classMethod);
            Assert.IsTrue(classMethod
                .Body
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Count() == 1);
        }

        private SyntaxNode codeExtension(ISyntacticalMatchResult<SyntaxNode> result, SyntacticalExtension<SyntaxNode> extension)
        {
            if (extension.Kind == ExtensionKind.Code)
            {
                var codeBlock = extension.Body as BlockSyntax;
                Assert.IsNotNull(extension.Body);
                return codeBlock.AddStatements(
                    new[] {
                    CSharp.ParseStatement("var myFoo = 5;"),
                    CSharp.ParseStatement("bar(myFoo);")
                });
            }

            Assert.AreEqual(extension.Kind, ExtensionKind.Expression);
            return CSharp.ParseExpression("bar(7)");
        }

        private SyntaxNode memberExtension(ISyntacticalMatchResult<SyntaxNode> result, SyntacticalExtension<SyntaxNode> extension)
        {
            var memberDecl = result.Node as MethodDeclarationSyntax;
            Assert.IsNotNull(memberDecl);

            return memberDecl
                .WithReturnType(CSharp.ParseTypeName("int"))
                .WithIdentifier(CSharp.ParseToken("anotherName"))
                .WithParameterList(CSharp.ParameterList())
                .WithBody(memberDecl.Body
                    .AddStatements(new[] {
                        CSharp.ParseStatement("var myFoo = 5;"),
                        CSharp.ParseStatement("bar(myFoo);")}));
        }

        private SyntaxNode typeExtension(ISyntacticalMatchResult<SyntaxNode> result, SyntacticalExtension<SyntaxNode> extension)
        {
            return CSharp.ClassDeclaration(extension.Identifier)
                .WithMembers(CSharp.List<MemberDeclarationSyntax>( new[] {
                        CSharp.MethodDeclaration(CSharp.ParseTypeName("int"), "myMethod")
                            .WithBody((BlockSyntax)extension.Body)
                }));
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
