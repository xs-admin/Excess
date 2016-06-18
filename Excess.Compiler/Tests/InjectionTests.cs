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
        public void Injection_Usage()
        {
            var tree = ExcessMock.Compile(@"
                class TestClass
                {
                    inject
                    {
                        IInterface1 _1;
                        IInterface2 _2;
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

            //a private field ought to be added per injection
            Assert.AreEqual(2, tree
                .GetRoot()
                .DescendantNodes()
                .Count(node => node is FieldDeclarationSyntax));
        }

        [TestMethod]
        public void Debug()
        {
            var tree = ExcessMock.Link(@"
                using xs.server;
                using xs.concurrent;

                using demo_transpiler;

                namespace Home
                { 
	                public function Transpile(string text)
	                {
		                inject 
		                {
			                ITranspiler	_transpiler;
		                }      

		                return _transpiler.Process(text);       
	                }

	                public function TranspileGraph(string text)
	                {
		                inject 
		                {
			                IGraphTranspiler _graphTranspiler;
		                }      

		                return _graphTranspiler.Process(text);      
	                } 
                }", 
                (compiler) => 
                {
                    DependencyInjection.Apply(compiler);
                    Functions.Apply(compiler);
                });

            //the result must contain one class (testing the temp class is removed)
            Assert.AreNotEqual(null, tree);
        }
    }
}