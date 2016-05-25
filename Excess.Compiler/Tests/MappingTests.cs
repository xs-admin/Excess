using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void MappingComments()
        {
            var tree = Mock.CompileWithMapping(
                @"  class SomeClass
                    {
                        //one line comment                        
                        public void SomeMehod()
                        {
                            //two line comment                        
                            //two line comment
                            someCall();    
                        }    
                    }");

            Assert.IsNotNull(tree);

            var root = tree.GetRoot();
            var @class = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            //the class declaration must exist and be on the first line
            Assert.IsNotNull(@class);
            var filePos = @class.GetLocation().GetMappedLineSpan();
            Assert.AreEqual(0, filePos.StartLinePosition.Line);

            var method = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Single();

            //the method must exist and be on the right line
            Assert.IsNotNull(method);
            filePos = method.GetLocation().GetMappedLineSpan();
            Assert.AreEqual(3, filePos.StartLinePosition.Line);
        }
    }
}
