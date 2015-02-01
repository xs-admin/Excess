using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Excess.Compiler.XS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class XSUsage
    {
        [TestMethod]
        public void Functions()
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
            string text = null;

            //as untyped method
            tree = compiler.ApplySyntacticalPass("class foo { public function bar() {}}", out text);
            Assert.IsTrue(text == "class foo\r\n{\r\n    public void bar()\r\n    {\r\n    }\r\n}");

            //as code function
            tree = compiler.ApplySyntacticalPass("class foo { public function bar() { function foobar(int x) {return 3;}}}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Any()); //code functions replaced by a lambda declaration
        }

        [TestMethod]
        public void Methods()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            XSModule.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //untyped method
            tree = compiler.ApplySyntacticalPass("class foo { method bar() {}}", out text);
            Assert.IsTrue(text == "class foo\r\n{\r\n    public void bar()\r\n    {\r\n    }\r\n}");

            //typed
            tree = compiler.ApplySyntacticalPass("class foo { int method bar() {}}", out text);
            Assert.IsTrue(text == "class foo\r\n{\r\n    public int bar()\r\n    {\r\n    }\r\n}");
        }

        [TestMethod]
        public void Events()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            XSModule.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //event handler usage
            var handlerTest = @"
                class foo
                {
                    public delegate void bar_delegate(int x, int y);
                    public event bar_delegate bar;

                    on bar()
                    {
                    }
                }";

            tree = compiler.ApplySemanticalPass(handlerTest, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Any()); //must have added a constructor

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .Identifier.ToString() == "on_bar"); //must have added a method and renamed it

            //event declaration usage
            var declarationTest = @"
                class foo
                {
                    public event bar(int x, int y);

                    on bar()
                    {
                    }
                }";

            tree = compiler.ApplySemanticalPass(declarationTest, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<EventFieldDeclarationSyntax>()
                .Any()); //must have added the event declaration

            var eventMethod = tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First();

            Assert.IsTrue(eventMethod
                .ParameterList
                .Parameters
                .Count == 2); //must have added the event's parameters to the handler
        }

        [TestMethod]
        public void TypeDef()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            XSModule.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            //event handler usage
            var cStyle = @"
                class foo
                {
                    typedef List<int> bar;
                    bar foobar;
                }";

            tree = compiler.ApplySemanticalPass(cStyle, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .First()
                .Declaration
                .Type
                .ToString() == "List<int>"); //must have replaced the type

            var csharpStyle = @"
                class foo
                {
                    typedef bar = List<int>;
                    bar foobar;
                }";

            tree = compiler.ApplySemanticalPass(csharpStyle, out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .First()
                .Declaration
                .Type
                .ToString() == "List<int>"); //must be equivalent to the last test
        }
    }
}