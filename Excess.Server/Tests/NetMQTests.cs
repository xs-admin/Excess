using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class NetMQTests
    {
        [TestMethod]
        public void Usage()
        {
            //setup
            const string SourceCode = @"
            struct HelloModel
            {
                public string Greeting;                
                public int Times;
                public GoodbyeService Goodbye;
            }

            concurrent object HelloService
            {
                int _times = 0;
                public HelloModel Hello(string who)
                {
                    return new HelloModel
                    {
                        Greeting = ""greetings, "" + who,
                        Times = _times++,
                        Goodbye = spawn<GoodbyeService>()
                    };
                }
            }

            concurrent object GoodbyeService
            {
                public string Name = ""GoodbyeService""; 

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
                        Url = ""tcp://localhost:1081"",
                        Hosts = new []
                        {
                            HelloService
                        }
                    };

                    Node node2 = new NetMQ.RequestResponse
                    {
                        Url = ""tcp://localhost:1082"",
                        Hosts = new []
                        {
                            GoodbyeService
                        }
                    };
                }
            }";

            var Services = new Dictionary<string, Guid>();
            using (var server = Mock.CreateServer(SourceCode, "Default", Services))
            {
                //make sure it compiles and such
                Assert.IsNotNull(server);

                HttpResponseMessage response;

                //the server should delegate to the NetMQ services
                response = server
                    .HttpClient
                    .GetAsync("/" + Services["HelloService"] + "/Hello")
                    .Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                var json = JObject.Parse(response
                    .Content
                    .ReadAsStringAsync()
                    .Result);

                Assert.IsTrue(json
                    .Descendants()
                    .OfType<JProperty>()
                    .Any(prop => 
                        prop.Name == "Times"));
            }
        }
    }
}