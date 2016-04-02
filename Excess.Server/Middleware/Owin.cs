using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using Excess.Concurrent.Runtime;

namespace Middleware
{
    public class ExcessOwinMiddleware : OwinMiddleware
    {
        IDistributedApp _server;
        public ExcessOwinMiddleware(OwinMiddleware next, IDistributedApp server) : base(next)
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
                        var body = new StreamReader(requestBody);
                        var data = await body.ReadToEndAsync();
                        var completion = new TaskCompletionSource<bool>();
                        _server.Receive(new DistributedAppMessage
                        {
                            Id = id,
                            Method = method,
                            Data = data,
                            Success = responseData => SendResponse(response, responseData, completion),
                            Failure = ex => SendError(response, ex, completion),
                        });

                        await completion.Task;
                    }
                    catch (Exception ex)
                    {
                        SendError(response, ex, null);
                    }

                    return;
                }
            }

            await Next.Invoke(context);
        }

        private static void SendResponse(IOwinResponse response, string data, TaskCompletionSource<bool> waiter)
        {
            response.Write(data);

            if (waiter != null)
                waiter.SetResult(true);
        }

        private void SendError(IOwinResponse response, Exception ex, TaskCompletionSource<bool> waiter)
        {
            response.StatusCode = 500;
            response.ReasonPhrase = ex.Message;
            response.Write(string.Empty);

            if (waiter != null)
                waiter.SetResult(false);
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
