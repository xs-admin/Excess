using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using System.Collections.Generic;

namespace Excess.Server.Middleware
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Runtime;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using FunctionAction = Action<string, IOwinResponse, TaskCompletionSource<bool>>;

    public class ExcessOwinMiddleware : OwinMiddleware
    {
        IDistributedApp _server;
        IDictionary<string, FunctionAction> _functions;
        public ExcessOwinMiddleware(OwinMiddleware next, IDistributedApp server, IEnumerable<Type> functions) : base(next)
        {
            _server = server;
            _functions = BuildFunctions(functions);
        }

        public async override Task Invoke(IOwinContext context)
        {
            if (context.Request.Method == "POST")
            {
                var id = Guid.Empty;
                var method = default(string);
                var function = default(FunctionAction);
                if (TryParseObjectMethod(context.Request.Path.Value, out id, out method)
                    || TryParseFunctional(context.Request.Path.Value, out function))
                {
                    var requestBody = context.Request.Body;
                    var response = context.Response;
                    var body = new StreamReader(requestBody);
                    var data = await body.ReadToEndAsync();
                    var completion = new TaskCompletionSource<bool>();

                    try
                    {
                        if (function != null)
                            function(data, context.Response, completion);
                        else //a distributed app message
                            _server.Receive(new DistributedAppMessage
                            {
                                Id = id,
                                Method = method,
                                Data = data,
                                Success = responseData => SendResponse(response, responseData, completion),
                                Failure = ex => SendError(response, ex, completion),
                            });
                    }
                    catch (Exception ex)
                    {
                        SendError(response, ex, null);
                    }

                    await completion.Task;
                    return;
                }
            }

            await Next.Invoke(context);
        }

        private IDictionary<string, FunctionAction> BuildFunctions(IEnumerable<Type> functions)
        {
            if (functions == null)
                return null;

            var result = new Dictionary<string, FunctionAction>();
            foreach (var functionObject in functions)
            {
                var @namespace = functionObject.Namespace.Split('.');
                var path = new StringBuilder();
                foreach (var ns in @namespace)
                {
                    path.Append("/");
                    path.Append(ns);
                }

                Debug.Assert(path.Length > 0);
                foreach (var method in functionObject.GetTypeInfo().DeclaredMethods)
                {
                    if (method.IsPublic)
                    {
                        var methodPath = $"{path}/{method.Name}";

                        var parameters = method
                            .GetParameters();

                        var paramCount = parameters.Length;
                        var paramNames = parameters
                            .Take(paramCount)
                            .Select(param => param.Name)
                            .ToArray();

                        var paramTypes = parameters
                            .Take(paramCount)
                            .Select(param => param.ParameterType)
                            .ToArray();

                        var instantiator = Application.GetService<IInstantiator>();
                        result[methodPath] = (data, response, continuation) =>
                        {
                            var args = JObject
                                .Parse(data);

                            var arguments = new object[paramCount]; 
                            for (int i = 0; i < paramCount - 1; i++)
                            {
                                arguments[i] = args
                                    .Property(paramNames[i])
                                    .Value
                                    .ToObject(paramTypes[i]);
                            }

                            //add the scope
                            arguments[paramCount - 1] = new __Scope(instantiator);

                            try
                            {
                                var responseValue = method.Invoke(null, arguments);
                                SendResponse(response, 
                                    $"{{\"__res\": {JsonConvert.SerializeObject(responseValue)}}}",
                                    continuation);
                            }
                            catch (Exception ex)
                            {
                                SendError(response, ex, continuation);
                            }
                        };
                    }
                }

            }

            return result;
        }

        private bool TryParseFunctional(string path, out Action<string, IOwinResponse, TaskCompletionSource<bool>> function)
        {
            function = null;
            return _functions != null
                    && _functions.TryGetValue(path, out function);
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

        private static bool TryParseObjectMethod(string value, out Guid id, out string method)
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
