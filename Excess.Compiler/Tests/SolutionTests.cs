using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Excess.Compiler;
using Excess.Compiler.Roslyn;
using Excess.Compiler.Mock;

namespace Tests
{
    using Microsoft.CodeAnalysis.CSharp;
    using CSharp = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

    [TestClass]
    public class SolutionTests
    {
        [TestMethod]
        public void ShouldBuildSolution()
        {
            var solution = ExcessMock.SolutionForCode(@"
                fn greetings()
                {
                    Console.WriteLn(""Hello"");
                }", null);

            Assert.IsNotNull(solution);
        }
    }
}
