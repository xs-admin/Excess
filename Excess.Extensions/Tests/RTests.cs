using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using Excess.Extensions.R;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class RTests
    {
        [TestMethod]
        public void R_Usage()
        {
            var tree = ExcessMock.Compile(@"
                void main()
                {
                    R()
                    {
                        x <- c(10.4, 5.6, 3.1, 6.4, 21.7)
                        y <- c(x, 0, x)
                        z <- 2*x + y + 1

                        a <- x > 13
                        b <- x[!(is.na(x))]
                        c <- x[-(1:5)]
                    }
                }", builder: (compiler) => RExtension.Apply(compiler));

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<VariableDeclarationSyntax>()
                .Count() == 6); //must have created 5 variables (x, y, z, a, b)

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<BinaryExpressionSyntax>()
                .Count() == 0); //must have replaced all operators
        }

        [TestMethod]
        public void R_Sequence_Usage()
        {
            var tree = ExcessMock.Compile(@"
                void main()
                {
                    R()
                    {
                        x <- 1:30
                        y <- 2*1:15
                        seq(-5, 5, by=.2) -> s3
                        s4 <- seq(length=51, from=-5, by=.2)
                        s5 <- rep(x, times=5)
                    }
                }", builder: (compiler) => RExtension.Apply(compiler));

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(invocation => invocation.Expression.ToString().Contains("RR"))
                .Count() == 6); //must have replaced all operators
         }

        [TestMethod]
        public void R_Statement_Usage()
        {
            var tree = ExcessMock.Compile(@"
                void main()
                {
                    R()
                    {
                        x <- 1
                        y <- 2
                        z <- NA

                        if (x == 1) 
                            3 -> z
                                                    
                        if (y == 1) 
                        {
                            3 -> z
                        }
                        else
                        {
                            z1 <- 4
                            z <- z1 
                        }

                        while(z < 10)  c(a, z) -> a

                        for(i in z)
                        {
                            a <- c(a, i);
                        }

                        repeat
                        {
                            b <- a   
                            a <- c(b, b);
                            if (length(a) > 10) break;
                        }
                    }
                }", builder: (compiler) => RExtension.Apply(compiler));

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<WhileStatementSyntax>()
                .Count() == 2); //must have replaced a while and a repeat

            Assert.IsTrue(tree.GetRoot()
                .DescendantNodes()
                .OfType<StatementSyntax>()
                .Where(ss => !(ss is ExpressionStatementSyntax || ss is LocalDeclarationStatementSyntax || ss is BlockSyntax))
                .Count() == 7); //3 if, 2 whiles, a foreach, a break
        }
    }
}
