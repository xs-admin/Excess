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
        ConcurrentServer _server;
        public ConcurrentOwinMiddleware(OwinMiddleware next, ConcurrentServer server) : base(next)
        {
            _server = server;
        }

        public async override Task Invoke(IOwinContext context)
        {
            if (context.Request.Method == "POST")
            {
                Guid id;
                string method;
                if (TryParsePath(context.Request.Path.Value, out id, out method))
                {
                    var requestBody = context.Request.Body;
                    var response = context.Response;
                    try
                    {
                        StreamReader reader = new StreamReader(requestBody);
                        JsonTextReader jsonReader = new JsonTextReader(reader);
                        JsonSerializer serializer = new JsonSerializer();

                        var args = serializer.Deserialize<JObject>(jsonReader);

                        await _server.Invoke(id, method, args,
                            json => response.Write(json.ToString()));
                    }
                    catch (Exception ex)
                    {
                        errorResponse(response, ex);
                    }

                    return;
                }
            }

            await Next.Invoke(context);
        }

        private void errorResponse(IOwinResponse response, Exception ex)
        {
            response.StatusCode = 500;
            response.ReasonPhrase = ex.Message;
            response.Write(string.Empty);
        }

        private static bool TryParsePath(string value, out Guid id, out string method)
        {
            id = Guid.Empty;
            method = null;

            var storage = new StringBuilder();
            var first = true;
            var awaitingId = false;
            foreach (var ch in value)
            {
                if (ch == '/')
                {
                    if (first)
                    {
                        first = false;
                        awaitingId = true;
                    }
                    else if (awaitingId)
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
