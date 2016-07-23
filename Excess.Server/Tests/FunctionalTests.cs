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
    public class FunctionalTests
    {
        [TestMethod]
        public void Functional_Usage()
        {
            //setup
            const string SourceCode = @"
            namespace Some.Namespace
            {
                function SomeFunction(string what)
                {
                    return ""Hello "" + what;
                }
            }";

            //server
            using (var server = Mock.CreateFunctionalServer(SourceCode))
            {
                HttpResponseMessage response;

                //the server should not respond to regular requests
                response = server.HttpClient.GetAsync("/").Result;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);

                var uri = $"/Some/Namespace/SomeFunction";

                //the server should not respond to get requests
                response = server.HttpClient.GetAsync($"{uri}?what=world").Result;
                Assert.AreEqual(response.StatusCode, HttpStatusCode.NotFound);

                //the server should not respond to post where the body is not json
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
