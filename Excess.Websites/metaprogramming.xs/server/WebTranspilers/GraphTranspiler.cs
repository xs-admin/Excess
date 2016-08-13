using System;
using System.Configuration;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using metaprogramming.interfaces;

namespace metaprogramming.server.WebTranspilers
{
    public class GraphTranspiler : IGraphTranspiler
    {
        public string Transpile(string code)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebTranspiler"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var contents = $"{{\"text\" : \"{HttpUtility.JavaScriptStringEncode(code)}\"}}";
                HttpResponseMessage response = client
                    .PostAsync("/transpile/graph", new StringContent(contents))
                    .Result;

                var result = "An error occured";
                if (response.IsSuccessStatusCode)
                {
                    var responseContents = response.Content.ReadAsStringAsync().Result;
                    result = JObject.Parse(responseContents)
                        .Property("__res")
                        .Value
                        .ToString();
                }

                return result;
            }
        }
    }
}
