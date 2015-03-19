using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Excess.Extensions.R;

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
                @"static void test()
                {
                    R()
                    {
                        x <- c(10.4, 5.6, 3.1, 6.4, 21.7)
                        y <- c(1, 2, 3)
                        z <- c(x, -1, y)
                    }

                    result[""z""] = z;
                }", compiler => Excess.Extensions.R.Extension.Apply(compiler));

            //must have executed
            Assert.IsNotNull(result);

            //must have return doubles
            Assert.IsInstanceOfType(result.z, typeof(Vector<Double>));

            IEnumerable<Double> values = result.z.getEnumerable<Double>();

            //must have returned 9 values, with -1 in between x and y
            Assert.IsNotNull(values);
            Assert.AreEqual(values.Count(), 9);
            Assert.AreEqual(values.ElementAt(5), -1);
        }
    }
}
