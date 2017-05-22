using Excess.Compiler.Roslyn;
using Excess.Concurrent.Compiler;
using Excess.Concurrent.Runtime;
using xslang;
using System;
//using System.Net.Http;
//using System.Net.Http.Headers;
using System.Configuration;
using Newtonsoft.Json.Linq;
using excess_ui.Interfaces;
using System.Web;

namespace excess_ui.server.WebTranspilers
{
    public class CodeTranspiler : ICodeTranspiler
    {
        RoslynCompiler _compiler = new RoslynCompiler();
        public CodeTranspiler()
        {
            XSLanguage.Apply(_compiler);
            ConcurrentExtension.Apply(_compiler);
        }

        public string Transpile(string source)
        {
            string result;
            _compiler.ApplySemanticalPass(source, out result);
            return result;

/*            var configKey = "WebTranspiler";
#if DEBUG
            configKey += "-debug";
#endif

            return "123 " + code + " 456";*/
            /*using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var uri = $"{ConfigurationManager.AppSettings[configKey]}/transpile/code";
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
            }*/
        }
    }
}
