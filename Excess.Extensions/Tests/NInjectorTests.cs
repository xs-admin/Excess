using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using NInjector;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests
{
    [TestClass]
    public class NInjectorTests
    {
        [TestMethod]
        public void NInjector_Usage()
        {
            var tree = ExcessMock.Compile(@"
            namespace SomeNS
            {
                injector
                {
                    ISomeType = ConcreteSomeType;
                }
            }", builder: (compiler) => NinjectExtension.Apply(compiler));

            Assert.IsNotNull(tree);

            //must find the overriden Load call from NInject
            var loadCall = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(method => method.Identifier.ToString() == "Load")
                .Single();

            //must find one bind call
            var bindCall = loadCall
                .DescendantNodes()
                .OfType<ExpressionStatementSyntax>()
                .Single();

            //must have 2 generic parameter list (ISomeType and ConcreteSomeType)
            Assert.AreEqual(2, bindCall
                .DescendantNodes()
                .OfType<TypeArgumentListSyntax>()
                .Count());
        }
    }
}
