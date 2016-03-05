using Microsoft.Owin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware
{
    class ConcurrentOwinMiddleware : OwinMiddleware
    {
        public ConcurrentOwinMiddleware(OwinMiddleware next) : base(next)
        {
        }

        //OwinMiddleware
        ConcurrentServer _server;
        public async override Task Invoke(IOwinContext context)
        {
            if (context.Request.Method == "POST")
            {
                Guid id;
                string method;
                if (TryParsePath(context.Request.Path.Value, out id, out method))
                {
                    var requestBody = context.Request.Body;
                    StreamReader reader = new StreamReader(requestBody);
                    JsonTextReader jsonReader = new JsonTextReader(reader);
                    JsonSerializer serializer = new JsonSerializer();

                    var args = serializer.Deserialize<JObject>(jsonReader);
                    var response = context.Response;

                    _server.Invoke(id, method, args, 
						json => response.Write(json.ToString()),
						ex =>
                        {
                            response.StatusCode = 500;
                            response.ReasonPhrase = ex.Message;
                        });
                }
            }

            await Next.Invoke(context);
        }

        private Action<JObject> sendResponse(IOwinResponse response)
        {
            return (result) => response.Write(result.ToString());
        }

        private static bool TryParsePath(string value, out Guid id, out string method)
        {
            id = Guid.Empty;
            method = null;

            var storage = new StringBuilder();
            var awaitingId = true;
            foreach (var ch in value)
            {
                if (ch == '/')
                {
                    if (awaitingId)
                    {
                        if (!Guid.TryParse(storage.ToString(), out id))
                            return false;

                        awaitingId = false;
                        storage.Clear();
                    }
                    else return false;
                }
                else if (ch == '?')
                    break;
                else
                    storage.Append(ch);
            }

            method = storage.ToString();
            return true;
        }
    }
}
