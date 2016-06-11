using Excess.Compiler.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xslang;

namespace Tests
{
    [TestClass]
    public class FunctionTests
    {
        [TestMethod]
        public void NamespaceFunctions()
        {
            var tree = ExcessMock.Compile(@"
                namespace SomeNamespace
                {
                    function SomeFunction()
                    {
                        return 10;
                    }
                }", (compiler) => Functions.Apply(compiler));

            Assert.IsNotNull(tree);
        }
    }
}
