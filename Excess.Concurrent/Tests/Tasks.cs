using Excess.Concurrent.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrent.Tests
{
    [TestClass]
    public class Tasks
    {
        [TestMethod]
        public void Usage()
        {
            var text = null as string;
            var tree = Mock.Compile(@"
                concurrent class SomeConcurrentClass
                {
                    public void SomeMethod()
                    {
                        await Task.Delay(1000);
                    }
                }", out text);

            Assert.IsNotNull(tree);
        }
    }
}