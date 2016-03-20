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
            public struct HelloModel
            {
                public string Greeting;                
                public int Times;
                public GoodbyeService Goodbye;
            }

            public concurrent object HelloService
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

            public concurrent class GoodbyeService
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
                    Identity = ""tcp://localhost:1079"";

                    Node node1 = new NetMQ.Node
                    {
                        Url = ""tcp://localhost:1081"",
                        Hosts = new []
                        {
                            HelloService
                        }
                    };

                    Node node2 = new NetMQ.Node
                    {
                        Url = ""tcp://localhost:1082"",
                        Hosts = new []
                        {
                            GoodbyeService
                        }
                    };
                }
            }";

            var services = new Dictionary<string, Guid>();
            using (var server = Mock.CreateServer(SourceCode, "Default", services))
            {
                HttpResponseMessage response;

                //make sure it compiles and such
                Assert.IsNotNull(server);

                //the server should delegate to the NetMQ services
                response = server
                    .HttpClient
                    .PostAsync(
                        "/" + services["HelloService"] + "/Hello",
                        new StringContent(JObject
                            .FromObject(new
                            {
                                who = "world"
                            }).ToString()))
                    .Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                var greeting = JObject.Parse(response
                    .Content
                    .ReadAsStringAsync()
                    .Result)
                    .Descendants()
                    .OfType<JProperty>()
                    .SingleOrDefault(prop =>
                        prop.Name == "Greeting");

                Assert.IsNotNull(greeting);
                Assert.IsNotNull(greeting.Value.ToString() == "greetings, world");

                //call again, still should be processed correctly
                response = server
                    .HttpClient
                    .PostAsync(
                        "/" + services["HelloService"] + "/Hello",
                        new StringContent(JObject
                            .FromObject(new
                            {
                                who = "ma"
                            }).ToString()))
                    .Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                //must have incremented the times
                var times = JObject.Parse(response
                    .Content
                    .ReadAsStringAsync()
                    .Result)
                    .Descendants()
                    .OfType<JProperty>()
                    .SingleOrDefault(prop =>
                        prop.Name == "Times");

                Assert.IsNotNull(times);
                Assert.IsNotNull(times.Value.ToString() == "1");

            }
        }
    }
}