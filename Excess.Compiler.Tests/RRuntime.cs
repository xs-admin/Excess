using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Excess.Compiler.Tests
{
    [TestClass]
    public class RRuntime
    {
        [TestMethod]
        public void RConcatenation()
        {
            dynamic result;
            result = RuntimeHelper.ExecuteTest(
                @"void test()
                {
                    R()
                    {
                        x <- c(10.4, 5.6, 3.1, 6.4, 21.7)
                        y <- c(1, 2, 3)
                        z <- c(x, -1, y)
                    }

                    result[""z""] = z;
                    result[""xlen""] = x.Count();
                }", compiler => Excess.Extensions.R.Extension.Apply(compiler));

            Assert.IsNotNull(result);
        }
    }
}
