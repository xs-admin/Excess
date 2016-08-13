using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json.Linq;
using metaprogramming.interfaces;

namespace metaprogramming.server.WebTranspilers
{
    public class CodeTranspiler : ICodeTranspiler
    {
        public string Transpile(string code)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["WebTranspiler"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var contents = $"{{\"text\" : \"{code}\"}}";
                HttpResponseMessage response = client
                    .PostAsync("/transpile/code", new StringContent(contents))
                    .Result;

                var result = string.Empty;
                if (response.IsSuccessStatusCode)
                {
                    var responseContents = response.Content.ReadAsStringAsync().Result;
                    result = JObject.Parse(responseContents)
                        .Property("__res")
                        .Value
                        .ToString();
                }

                return "An error occured";
            }
        }
    }
}
