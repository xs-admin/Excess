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
            tree = compiler.ApplySemanticalPass("class foo { public function bar() {}}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .ReturnType
                .ToString() == "void"); //must have added a return type


            //as code function
            tree = compiler.ApplySemanticalPass("class foo { public function bar() { function foobar(int x) {return 3;}}}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ParenthesizedLambdaExpressionSyntax>()
                .Any()); //code functions replaced by a lambda declaration

            //as type, without return type
            tree = compiler.ApplySemanticalPass("class foo { void bar() { function<void, string> foobar; }}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .First()
                .Declaration
                .Type
                .ToString() == "Action<string>"); //must have changed the function type into an action (because of the void)

            //as type, with return type
            tree = compiler.ApplySemanticalPass("class foo { void bar() { function<int, string> foobar; }}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .First()
                .Declaration
                .Type
                .ToString() == "Func<string,int>"); //must have changed the function type, moving the return type to the end 
        }

        [TestMethod]
        public void Members()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            XSModule.Apply(compiler);

            SyntaxTree tree = null;
            string     text = null;

            //typed method
            tree = compiler.ApplySyntacticalPass("class foo { int method bar() {}}", out text);
            Assert.IsTrue(text == "class foo\r\n{\r\n    public int bar()\r\n    {\r\n    }\r\n}");

            //untyped method
            tree = compiler.ApplySemanticalPass("class foo { method bar() { return 5; }}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First()
                .ReturnType
                .ToString() == "Int32"); //must have added a return type

            //constructors
            tree = compiler.ApplySyntacticalPass("class foo { constructor(int x, int y) {}}", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Any()); //must have added a constructor

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First()
                .Members
                .Count == 1); //must have nothing else

            //typed properties
            tree = compiler.ApplySyntacticalPass("class foo { int property bar; }", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Any()); //must have added a property

            //untyped properties, initialization
            tree = compiler.ApplySyntacticalPass("class foo { property bar = 3; }", out text);
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Any()); //must have added a property

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Any()); //must have added a constructor for initialization
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