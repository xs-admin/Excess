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
            Assert.IsTrue(text == "class foo\r\n{\r\n    public void bar()\r\n    {\r\n    }\r\n}");
        }
    }
}
