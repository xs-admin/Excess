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
                        someExtension()
                        {
                            [Header1]
                                Value1 = 10
                                Value2 = ""SomeValue""
                            [Header2]
                                Value3 = ""Hello "" + TestVar
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
        }

        [TestMethod]
        public void Indented_Razor_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class TestClass
                {
                    void TestMethod()
                    {
                        someExtension()
                        {
                            [Header1]
                                Call Someone at 1-800-WAT-EVER
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
        }
    }
}
