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
                        }
                    }
                }", builder: (compiler) => ContractExtension.Apply(compiler));

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Count() == 2); //must have added an if for each contract condition
        }
    }
}
