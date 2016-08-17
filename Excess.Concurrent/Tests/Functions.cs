using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Concurrent.Tests;

namespace Concurrent.Tests
{
    [TestClass]
    public class Functions
    {
        [TestMethod]
        public void Usage()
        {
            var text = null as string;
            var tree = Mock.Compile(@"
                function SomeFunction()
                {
                    (SomeOtherFunction1() && SomeOtherFunction2())
                        >> Console.WriteLine(""All finished"");        
                }

                function SomeOtherFunction1()
                {
                    await SomeOtherFunction3();
                }

                function SomeOtherFunction2()
                {
                    await SomeOtherFunction3();
                }", out text);

            Assert.IsNotNull(tree);
        }
    }
}
