using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Excess.Compiler.Roslyn;

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
        }
    }
}
