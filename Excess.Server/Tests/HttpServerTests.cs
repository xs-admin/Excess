using System;
using System.Text;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class HttpServerTests
    {
        [TestMethod]
        public void Usage()
        {
            //setup
            const string SourceCode = @"
            public TestService
            {
                public string Hello(string what)
                {
                    return ""Hello "" + what;
                }
            }";

            Guid serviceId = Guid.NewGuid();
            string serviceName = "TestService";

            //server
            using (var server = Mock.CreateHttpServer(SourceCode, serviceId, serviceName))
            {
                HttpResponseMessage response;

                //the server should not respond to regular requests
                response = server.HttpClient.GetAsync("/").Result;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);

                var uri = $"/{serviceId}/Hello";

                //the server should not respond to get requests
                response = server.HttpClient.GetAsync($"{uri}?what=world").Result;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);

                //the server should not respond to post where te body is not json
                response = server.HttpClient.PostAsync(uri,
                    new StringContent(
                        "what=world",
                        Encoding.UTF8,
                        "application/json")).Result;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.InternalServerError);

                //the server should accept a proper request
                response = server.HttpClient.PostAsync(uri,
                    new StringContent(
                        "{what:\"world\"}",
                        Encoding.UTF8,
                        "application/json")).Result;

                Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);

                //the result should come in json format
                var responseContents = response
                    .Content
                    .ReadAsStringAsync()
                    .Result;

                //must be valid json
                var json = JObject.Parse(responseContents);

                //should say "Hello world"
                Assert.AreEqual(json.Property("__res").Value, "Hello world");
            }
        }
    }
}
