using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;

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
                        Goodbye = spawn<GoodbyeService>(who)
                    };
                }
            }

            public concurrent class GoodbyeService
            {
                string _who;
                public GoodbyeService(string who)
                {
                    _who = who;
                }

                public string Goodbye(string what)
                {
                    return $""Goodbye {what}, goodbye {_who}"";
                }
            }

            public concurrent object ProcessingService
            {
                public string Process(string what, GoodbyeService unGreeter)
                {
                    string goodbyeText = await unGreeter.Goodbye(what);
                    return what + "" then "" + goodbyeText;
                }
            }

            namespace Servers
            {
                server Default()
                {
                    Url = ""http://localhost:1080"";
                    Identity = ""tcp://localhost:5000"";

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
                            ProcessingService
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
                        new StringContent(JObject.FromObject(new
                        {
                            who = "world"
                        })
                        .ToString()))
                    .Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                var json = Mock.ParseResponse(response);

                Assert.IsNotNull(json.Greeting);
                Assert.AreEqual(json.Greeting.Value.ToString(),"greetings, world");

                var firstGoodbye = json.Goodbye.__ID;

                //call again, still should be processed correctly
                response = server
                    .HttpClient
                    .PostAsync(
                        "/" + services["HelloService"] + "/Hello",
                        new StringContent(JObject.FromObject(new
                        {
                            who = "ma"
                        })
                        .ToString()))
                    .Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                json = Mock.ParseResponse(response);

                //must have incremented the times
                Assert.IsNotNull(json.Times);
                Assert.AreEqual(json.Times.Value.ToString(), "1");

                //the goodbye service should be a different each time
                var secondGoodbye = json.Goodbye.__ID;

                Assert.AreNotEqual(firstGoodbye, secondGoodbye);

                //invoke Goodbye(s)
                response = server
                    .HttpClient
                    .PostAsync(
                        "/" + firstGoodbye + "/Goodbye",
                        new StringContent(JObject.FromObject(new
                        {
                            what = "blue sky"
                        })
                        .ToString()))
                    .Result;

                json = Mock.ParseResponse(response);
                Assert.AreEqual(json.Value.ToString(), "Goodbye blue sky, goodbye world");

                //and the second
                response = server
                    .HttpClient
                    .PostAsync(
                        "/" + secondGoodbye + "/Goodbye",
                        new StringContent(JObject.FromObject(new
                        {
                            what = "max"
                        })
                        .ToString()))
                    .Result;

                json = Mock.ParseResponse(response);
                Assert.AreEqual(json.Value.ToString(), "Goodbye max, goodbye ma");
            }
        }
    }
}