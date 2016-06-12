using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
    public class MatchTests
    {
        [TestMethod]
        public void Match_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class SomeClass
                {
                    method SomeMethod()
                    {
                        match(x)
                        {
                            case > 10:
                                break;
                            case < 5:
                                Console.WriteLine(""no breaks needed"");
                            default:
                                Console.WriteLine(""like a switch"");
                        }
                    }
                }", (compiler) => Match.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //2 ifs (and one else) should have been created
            Assert.AreEqual(root
                .DescendantNodes()
                .OfType<IfStatementSyntax>()
                .Count(), 2);

            //no breaks
            Assert.IsFalse(root
                .DescendantNodes()
                .OfType<BreakStatementSyntax>()
                .Any());
        }
    }
}