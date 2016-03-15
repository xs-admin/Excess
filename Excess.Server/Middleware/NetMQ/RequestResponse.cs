using Excess.Concurrent.Runtime;
using Middleware;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetMQNode
{
    using System.Collections.Concurrent;
    using Runtime = Excess.Concurrent.Runtime;

    public class RequestResponseClient : IConcurrentNode
    {
        string _url;
        public RequestResponseClient(string url)
        {
            _url = url;
        }

        private class Request
        {
            public string Payload { get; set; }
            public Action<JObject> Continuation { get; set; }
        }

        public void Connect(IConcurrentServer server, Action connected, Action<Exception> failure)
        {
            Task.Run(() =>
            {
                try
                {
                    using (var context = NetMQContext.Create())
                    using (var client = context.CreateRequestSocket())
                    {
                        client.Connect(_url);

                        //connection protocol
                        List<Guid> publicObjects = new List<Guid>();
                        client.SendFrame(string.Empty);
                        handshake(client.ReceiveFrameString(), publicObjects);

                        using (var queue = new BlockingCollection<Request>())
                        {
                            //register the objects in the main server
                            foreach (var @object in publicObjects)
                            {
                                server.Register(@object, (_, method, args, success) =>
                                {
                                    var payload = buildCallRequest(@object, method, args);
                                    queue.Add(new Request
                                    {
                                        Payload = payload,
                                        Continuation = success
                                    });
                                });
                            }

                            connected();

                            //main loop
                            for (;;)
                            {
                                var request = queue.Take();
                                client.SendFrame(request.Payload);

                                var response = client.ReceiveFrameString();
                                var json = JObject.Parse(response);
                                request.Continuation(json);

                                //any concurrent object returned must registered in the server
                                foreach (var objId in json
                                    .Descendants()
                                    .OfType<JProperty>()
                                    .Where(prop => prop.Name == "__ID")
                                    .Select(prop => (Guid)prop.Value))
                                {
                                    if (!server.Has(objId))
                                        server.Register(objId, (_, method, args, success) =>
                                            queue.Add(new Request
                                            {
                                                Payload = buildCallRequest(objId, method, args),
                                                Continuation = success,
                                            }));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    failure(ex);
                }
            });
        }

        public ConcurrentObject Bind(Guid id)
        {
            throw new NotImplementedException();
        }

        private void handshake(string value, List<Guid> result)
        {
            var delimitedObjects = value.Split('|'); //why? why not?
            foreach (var @object in delimitedObjects)
                result.Add(Guid.Parse(@object));
        }

        private string buildCallRequest(Guid @object, string method, JObject args)
        {
            var newJson = new JObject(new
            {
                id = @object,
                method = method,
                args = args
            });

            return newJson.ToString();
        }
    }

    public class RequestResponseServer : ConcurrentServer
    {
        string _url;
        public RequestResponseServer(string url)
        {
            _url = url;
        }

        public override void Start()
        {
            using (var context = NetMQContext.Create())
            using (var server = context.CreateResponseSocket())
            {
                server.Bind(_url);

                //connect concurrent, blocking
                base.Start();

                //loop
                for (;;)
                {
                    var message = server.ReceiveFrameString();
                    if (message.Any())
                    {
                        //parent server requesting public objects
                        server.SendFrame(serverObjects());
                        continue;
                    }

                    try
                    {
                        JObject call = JObject.Parse(message);

                        var @object = Guid.Parse(call.GetValue("id").ToString());
                        var method = call.GetValue("method").ToString();
                        var args = JObject.FromObject(call.GetValue("args"));

                        Invoke(@object, method, args,
                            result => server.SendFrame(result.ToString()));
                    }
                    catch (Exception ex)
                    {
                        server.SendFrame(exceptionJson(ex).ToString());
                    }
                }
            }
        }

        private string serverObjects()
        {
            return string.Join("|", _funcs
                .Keys
                .Select(key => key.ToString()
                .ToArray()));
        }

        private JObject exceptionJson(Exception ex)
        {
            return new JObject(new
            {
                ExceptionType = ex.GetType().AssemblyQualifiedName,
                Message = ex.Message
            });
        }
    }
}
