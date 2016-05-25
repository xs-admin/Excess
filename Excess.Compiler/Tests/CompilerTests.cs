using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;

namespace Tests
{
    using Excess.Compiler;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [TestClass]
    public class CompilerTests
    {
        [TestMethod]
        public void LexicalTokenMatching()
        {
            var compiler = new RoslynCompiler();
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
                        .replace("open", "{")
                        .replace("close", "}"));

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

            string lResult = compiler.ApplyLexicalPass("my_ext(int i) { code(); }");
            Assert.IsTrue(lResult == "my_ext_replaced (int i)  = { code(); }");

            lexical
                .extension("my_ext_s", ExtensionKind.Member, myExtSyntactical);

            SyntaxNode sResult = compiler.ApplyLexicalPass("my_ext_s(int i) { code(); }", out lResult);

            Assert.IsTrue(lResult == "void __extension() {}");

            var method = sResult
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();

            Assert.IsNotNull(method);
            Assert.IsTrue(method
                .ParameterList
                .Parameters
                .Count == 1);

            Assert.IsTrue(method
                .Body
                .Statements
                .Count == 1);
        }

        private IEnumerable<SyntaxToken> myExtLexical(IEnumerable<SyntaxToken> tokens, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            string testResult = "my_ext_replaced "
                              + RoslynCompiler.TokensToString(extension.Arguments)
                              + " = "
                              + RoslynCompiler.TokensToString(extension.Body);

            return RoslynCompiler.ParseTokens(testResult);
        }

        private SyntaxNode myExtSyntactical(SyntaxNode node, Scope scope, LexicalExtension<SyntaxToken> extension)
        {
            Assert.IsTrue(node is MethodDeclarationSyntax);
            var method = node as MethodDeclarationSyntax;

            Assert.IsTrue(method.Identifier.ToString() == "__extension");

            var argString = RoslynCompiler.TokensToString(extension.Arguments);

            Assert.IsTrue(argString == "(int i) ");
            var arguments = CSharp.ParseParameterList(argString);

            var codeString = RoslynCompiler.TokensToString(extension.Body);
            var codeNode = CSharp.ParseStatement(codeString);

            Assert.IsTrue(codeNode is BlockSyntax);
            var code = codeNode as BlockSyntax;

            return method
                .WithIdentifier(CSharp.ParseToken("my_ext_s"))
                .WithParameterList(arguments)
                .WithBody(code);
        }

        [TestMethod]
        public void SyntacticalMatching()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            var syntax = compiler.Syntax();

            //simple match
            syntax
                .match<ClassDeclarationSyntax>(c => !c.Members.OfType<ConstructorDeclarationSyntax>().Any())
                    .then(addConstructor);

            var tree = compiler.ApplySyntacticalPass("class foo { } class bar { bar() {} }");

            Assert.IsTrue(tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Count() == 2); //must have added a constructor to "foo"

            //scope match & transform
            syntax
                .match<ClassDeclarationSyntax>(c => c.Identifier.ToString() == "foo")
                    .descendants<MethodDeclarationSyntax>(named: "methods")
                    .descendants<PropertyDeclarationSyntax>(prop => prop.Identifier.ToString().StartsWith("my"), named: "myProps")
                .then(syntax.transform()
                    .replace("methods", method => ((MethodDeclarationSyntax)method)
                        .WithIdentifier(CSharp.ParseToken("my" + ((MethodDeclarationSyntax)method).Identifier.ToString())))
                    .remove("myProps"));


            var scopeTree = compiler.ApplySyntacticalPass("class foo { public void Method() {} int myProp {get; set;} }");
            Assert.IsTrue(scopeTree.ToString() == "class foo { public void myMethod() {} foo (){}}");

            Assert.IsTrue(scopeTree
                .GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Count() == 1); //must have added a constructor to "foo", since the syntax is the same
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
            var syntax = compiler.Syntax();

            SyntaxTree tree;

            //code extension
            syntax
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
            syntax
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
            syntax
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

        private SyntaxNode codeExtension(SyntaxNode node, SyntacticalExtension<SyntaxNode> extension)
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

        private SyntaxNode memberExtension(SyntaxNode node, SyntacticalExtension<SyntaxNode> extension)
        {
            var memberDecl = node as MethodDeclarationSyntax;
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

        private SyntaxNode typeExtension(SyntaxNode node, SyntacticalExtension<SyntaxNode> extension)
        {
            return CSharp.ClassDeclaration(extension.Identifier)
                .WithMembers(CSharp.List<MemberDeclarationSyntax>(new[] {
                        CSharp.MethodDeclaration(CSharp.ParseTypeName("int"), "myMethod")
                            .WithBody((BlockSyntax)extension.Body)
                }));
        }
    }
}

