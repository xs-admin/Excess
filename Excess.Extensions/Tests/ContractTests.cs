using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using Contract;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class ContractTests
    {
        [TestMethod]
        public void Contract_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        contract
                        {
                            x > 3;
                            y != null;
                            z.Validate()
                                >> ArgumentException(""z""); 
                        }
                    }
                }", builder: (compiler) => ContractExtension.Apply(compiler));

            //must have added an if for each contract condition
            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Count() == 3); 

            //must have added a throw statement for an ArgumentException
            var newExpr = tree.GetRoot()
                .DescendantNodes()
                .OfType<ObjectCreationExpressionSyntax>()
                .Where(creation => creation.Type.ToString() == "ArgumentException")
                .Single();

            Assert.AreEqual(1, newExpr.ArgumentList.Arguments.Count);
            Assert.AreEqual("\"z\"", newExpr.ArgumentList.Arguments.Single().ToString());
        }

        [TestMethod]
        public void Contract_WhenUsingInvocationThenException_ShouldSucceed()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    void SomeMethod()
                    {
                        contract
                        {
                            SomeObject.SomeOtherMethod()
                                >> SomeException();
                        }
                    }
                }", builder: (compiler) => ContractExtension.Apply(compiler));

            Assert.IsNotNull(tree);
            Assert.IsFalse(tree.GetDiagnostics().Any());
        }
    }
}
