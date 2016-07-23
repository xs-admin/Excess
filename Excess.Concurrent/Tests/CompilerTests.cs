using Excess.Compiler.Roslyn;
using Excess.Concurrent.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concurrent.Tests
{
    [TestClass]
    public class CompilerTests
    {
        [TestMethod]
        public void ShouldSupport_GenericReturnType()
        {
            RoslynCompiler compiler = new RoslynCompiler();
            ConcurrentExtension.Apply(compiler);

            SyntaxTree tree = null;
            string text = null;

            tree = compiler.ApplySemanticalPass(@"
                concurrent class SomeClass 
                { 
                    public IEnumerable<int> SomeMethod()
                    {
                        throw new NotImplementedException();
                    }
                }", out text);

            Assert.IsNotNull(tree);

            //must have passed the generic type along
            Assert.IsTrue(tree
                .GetRoot()
                .DescendantNodes()
                .OfType<TypeSyntax>()
                .Where(type => type.ToString() == "IEnumerable<int>")
                .Any());
        }
    }
}
