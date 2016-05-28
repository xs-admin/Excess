using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using xslang;
using Excess.Compiler.Mock;

namespace Tests
{
    [TestClass]
    public class InjectionTests
    {
        [TestMethod]
        public void InjectionUsage()
        {
            var tree = ExcessMock.Compile(@"
                class TestClass
                {
                    inject
                    {
                        IInterface1 _1;
                        public IInterface2 _2 {get; private set;}
                    }
                }", (compiler) => DependencyInjection.Apply(compiler));

            //the result must contain one class (testing the temp class is removed)
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .Count(node => node is ClassDeclarationSyntax));

            //a constructor must be added
            var ctor = tree
                .GetRoot()
                .DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Single();

            //one parameter per injection
            Assert.AreEqual(2, ctor.ParameterList.Parameters.Count);

            //a private field ought to be added
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .Count(node => node is FieldDeclarationSyntax));

            //and a public property
            Assert.AreEqual(1, tree
                .GetRoot()
                .DescendantNodes()
                .Count(node => node is PropertyDeclarationSyntax));
        }
    }
}