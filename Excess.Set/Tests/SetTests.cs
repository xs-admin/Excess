using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Mock;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Linq;
using Compiler;
using Compiler.SetParser;
using Microsoft.CodeAnalysis.CSharp;

namespace Tests
{
    [TestClass]
    public class SetTests
    {
        [TestMethod]
        public void Set_Predicate_Usage()
        {
            var set = SetMock.Parse("x | x > 5");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 1);
            Assert.IsInstanceOfType(set.Constructor, typeof(PredicateConstructorSyntax));

            //class Someclass
            //{
            //    void somefn()
            //    {
            //        set<int> ps = {x | x > 5}
            //        //set ps = {x e int, y in Someset | Vx, Ey. y.someProperty}
            //        //set ps = {x in int, y in Someset | Vx. Ey. y.someProperty}
            //        //set ps = {x, y | when x = 3 => y = 7, otherwise y = 8 }
            //    }
            //}", (compiler) => SetExtension.Apply(compiler));
        }

        [TestMethod]
        public void Set_Predicate_With_Type()
        {
            var set = SetMock.Parse("x in N | x > 5");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 1);
            Assert.IsInstanceOfType(set.Constructor, typeof(PredicateConstructorSyntax));
        }

        [TestMethod]
        public void Set_Predicate_With_Multiple_Variables()
        {
            var set = SetMock.Parse("x in N, y : R | x > y");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 2);
            Assert.IsInstanceOfType(set.Constructor, typeof(PredicateConstructorSyntax));
        }

        [TestMethod]
        public void Set_Predicate_With_Complex_Expressions()
        {
            var set = SetMock.Parse("x, y | x > y && y > 5");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 2);
            Assert.IsInstanceOfType(set.Constructor, typeof(PredicateConstructorSyntax));

            var ctor = set.Constructor as PredicateConstructorSyntax;
            Assert.AreEqual(ctor.Expressions.Count(), 1);

            var expr = ctor.Expressions.First();
            Assert.IsInstanceOfType(expr, typeof(BinaryExpressionSyntax));
            Assert.IsTrue((expr as BinaryExpressionSyntax)
                .OperatorToken
                .IsKind(SyntaxKind.AmpersandAmpersandToken));
        }

        [TestMethod]
        public void Set_With_Indices()
        {
            var set = SetMock.Parse("xi | i in N, x > 5");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 1);
            Assert.IsTrue(set.Variables[0].IsIndexed);
            Assert.AreEqual(set.Variables[0].IndexName, "i");
            Assert.AreEqual(set.Variables[0].IndexType.ToString(), "N");

            Assert.IsInstanceOfType(set.Constructor, typeof(PredicateConstructorSyntax));

            var ctor = set.Constructor as PredicateConstructorSyntax;
            Assert.AreEqual(ctor.Expressions.Count(), 1);
        }

        [TestMethod]
        public void Set_With_Alias()
        {
            var set = SetMock.Parse("x as name, y as age | y > 40");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 2);
            Assert.AreEqual(set.Variables[0].Alias, "name");
            Assert.AreEqual(set.Variables[1].Alias, "age");
        }

        [TestMethod]
        public void Set_Match_Constructor()
        {
            var set = SetMock.Parse("x, y | when y > 40 : x = y + 1, otherwise 42");
            Assert.IsNotNull(set);
            Assert.AreEqual(set.Variables.Length, 2);

            Assert.IsNotNull(set.Constructor);
            Assert.IsInstanceOfType(set.Constructor, typeof(MatchConstructorSyntax));

            var ctor = set.Constructor as MatchConstructorSyntax;
            Assert.AreEqual(ctor.MatchPairs.Count(), 1);
            Assert.AreEqual(ctor.Otherwise.ToString(), "42");
        }

        [TestMethod]
        public void Set_NumericalInduction_Constructor()
        {
            var set = SetMock.Parse("x | x[0] = 0, x[1] = 1,..., x[n] = x[n-1] + x[n-2]");
            Assert.IsNotNull(set);

            Assert.IsNotNull(set.Constructor);
            Assert.IsInstanceOfType(set.Constructor, typeof(NumericalInductionConstructorSyntax));

            var ctor = set.Constructor as NumericalInductionConstructorSyntax;
            Assert.AreEqual(ctor.Values.Count(), 4);
            Assert.AreEqual(ctor.Values.ElementAt(2).ToString(), "ellipsis");
        }

        [TestMethod]
        public void Set_GeneralInduction_Constructor()
        {
            var set = SetMock.Parse("x | / 1, n /(n + 2)");
            Assert.IsNotNull(set);

            Assert.IsNotNull(set.Constructor);
            Assert.IsInstanceOfType(set.Constructor, typeof(InductionConstructorSyntax));

            var ctor = set.Constructor as InductionConstructorSyntax;
            Assert.AreEqual(ctor.Rules.Count(), 2);
        }
    }
}
