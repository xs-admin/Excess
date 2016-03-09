using Excess.Compiler.Roslyn;
using LanguageExtension;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class LanguageExtensionTests
    {
        [TestMethod]
        public void Usage()
        {
            //setup
            const string Code = @"
            using XS.Extensions.Server;

            namespace SomeNS            
            {
                server SomeServer()
                {
                    Url = ""http://*.1080"";
                
                    Node someNode = new NetMQ_RequestResponse
                    {
                        Url = ""http://*.2080"",
                        Threads = 25,
                        Hosts = new []
                        {
                            SomeService
                        }
                    };
                }
            }";

            RoslynCompiler compiler = new RoslynCompiler();
            Extension.Apply(compiler);

            string output = null;
            SyntaxTree tree = compiler.ApplySemanticalPass(Code, out output);

            Assert.IsNotNull(tree);
        }
    }
}
