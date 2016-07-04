using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Mock;
using Tests.Mocks;
using Microsoft.CodeAnalysis.CSharp;

namespace Tests
{
    [TestClass]
    public class IndentedTests
    {
        [TestMethod]
        public void Indented_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class TestClass
                {
                    void TestMethod()
                    {
                        var TestVar = ""World"";        
                        var TestArray = new [] {""Hello"", ""World""};
                        someExtension()
                        {
                            [Header1]
                                Value1 = 10
                                Value2 = ""SomeValue""
                            [Header2]
                                Value3 = ""Hello "" + TestVar
                            [ContactHeader]
                                Call Someone at 1-800-WAT-EVER
                                    Or else at 1-800-456-7890
                                    Less likely at (877) 789-1234

                            //some code
                            for i in TestArray
                                Console.Write(i);
                        }
                    }
                }", (compiler) => MockIndentGrammar.Apply(compiler));

            Assert.IsNotNull(tree);

            //must have add 3 calls in the form SetHeaderValue("[Header]", "Value", Value);
            Assert.AreEqual(3, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation
                    .Expression
                    .ToString()
                    .StartsWith("SetHeaderValue"))
                .Count());

            //must have added a call in the form SetContact("[Header]", "Name", "Telephone");
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation
                    .Expression
                    .ToString()
                    .StartsWith("SetContact"))
                .Count());

            //must refer to the supplied number
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(literal => literal
                    .ToString()
                    .Equals("\"1-800-WAT-EVER\""))
                .Count());

            //must have added extra numbers in the form AddContactNumber("Header", "Name", AreaCode, First 3 numbers, Last 4 numbers)
            Assert.AreEqual(2, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation
                    .Expression
                    .ToString()
                    .Equals("AddContactNumber"))
                .Count());

            //must have parsed correctly and include all secondary telephones
            var numbers = new[] { 800, 456, 7890, 877, 789, 1234 };
            Assert.AreEqual(numbers.Length, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<LiteralExpressionSyntax>()
                .Where(number => number.IsKind(SyntaxKind.NumericLiteralExpression)
                              && numbers.Contains(int.Parse(number.ToString())))
                .Count());

            //must have added a foreach statement
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ForEachStatementSyntax>()
                .Count());
        }
    }
}
