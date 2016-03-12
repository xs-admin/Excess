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
                
                    Node someNode = new NetMQ.RequestResponse
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

            string output;
            var tree = Mock.Compile(Code, out output);

            Assert.IsNotNull(tree);
        }

        [TestMethod]
        public void WithNodes()
        {
            //setup
            const string Source = @"
            concurrent class HelloService
            {
                public string Hello(string what)
                {
                    return ""Hello "" + what;
                }
            }

            concurrent class GoodbyeService
            {
                public string Goodbye(string what)
                {
                    return ""Goodbye "" + what;
                }
            }

            namespace Servers
            {
                server Default()
                {
                    Url = ""http://localhost:1080"";

                    Node node1 = new NetMQ.RequestResponse
                    {
                        Url = ""http://localhost:1081"",
                        Hosts = new []
                        {
                            HelloService
                        }
                    };

                    Node node2 = new NetMQ.RequestResponse
                    {
                        Url = ""http://localhost:1082"",
                        Hosts = new []
                        {
                            GoodbyeService
                        }
                    };
                }
            }";

            var storage = new MockStorage();
            var errors = new List<string>();
            var compilation = Mock.Build(Source, storage, errors);
            
            Assert.IsFalse(errors.Any());

            var serverConfig = compilation.Scope.get<IServerConfiguration>();
            Assert.IsNotNull(serverConfig);

            var clientCode = serverConfig.GetClientInterface();

            Assert.IsTrue(
                clientCode.Contains("function HelloService()")
                && clientCode.Contains("function GoodbyeService()")
                && clientCode.Contains("Hello = function (what)")
                && clientCode.Contains("Goodbye = function (what)"));
        }
    }
}
