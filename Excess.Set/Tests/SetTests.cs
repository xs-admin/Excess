using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using Compiler;

namespace Tests
{
    [TestClass]
    public class SetTests
    {
        [TestMethod]
        public void Set_Usage()
        {
            var tree = ExcessMock.Compile(@"
            class Someclass
            {
                void somefn()
                {
                    set<int> ps = {x | x > 5}
                    //set ps = {x : int | x > 5}
                    //set ps = {xi | i : Guid, x > 5}
                    //set ps = {x | x in Y, (x > 5 || P(x))}
                    //set ps = {x as name : int, y as age in Someset | x > 5 & y.someProperty}
                    //set ps = {x e int, y in Someset | Vx, Ey. y.someProperty}
                    //set ps = {x in int, y in Someset | Vx. Ey. y.someProperty}
                    //set ps = {x, y | when x = 3 => y = 7, otherwise y = 8 }
                }
            }", (compiler) => SetExtension.Apply(compiler));

            var root = tree
                ?.GetRoot()
                ?.NormalizeWhitespace();

            //a class must have been created
            var @class = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            //named with the "Functions" convention
            Assert.AreEqual(@class.Identifier.ToString(), "SomeObject");

            //must have added properties
            Assert.AreEqual(2, root
                .DescendantNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Count());
        }
    }
}
