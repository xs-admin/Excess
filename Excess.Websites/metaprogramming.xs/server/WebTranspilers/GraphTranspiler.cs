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
            var configKey = "WebTranspiler";
#if DEBUG
            configKey += "-debug";
#endif

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var uri = $"{ConfigurationManager.AppSettings[configKey]}/transpile/graph";
                var contents = $"{{\"text\" : \"{HttpUtility.JavaScriptStringEncode(code)}\"}}";
                HttpResponseMessage response = client
                    .PostAsync(uri, new StringContent(contents))
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
