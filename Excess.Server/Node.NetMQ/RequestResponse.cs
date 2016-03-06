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
    using Runtime = Excess.Concurrent.Runtime;

    public class RequestResponseClient : IConcurrentNode
    {
        Runtime.Node _node;
        NetMQContext _context;
        string _url;
        RequestSocket _socket;
        public void Connect(IConcurrentServer server)
        {
            _socket = _context.CreateRequestSocket();
            _socket.Connect(_url);

            //connection protocol
            List<Guid> publicObjects = new List<Guid>(); 
            _socket.SendFrame(string.Empty);
            handshake(_socket.ReceiveFrameString(), publicObjects);

            //register the objects in the main server
            foreach (var @object in publicObjects)
            {
                server.Register(@object, (_, method, args, success, failure) =>
                {
                    var request = buildCallRequest(@object, method, args);
                    Task.Run(() =>
                    {
                        try
                        {
                            _socket.SendFrame(request);

                            var response = _socket.ReceiveFrameString();
                            success(JObject.Parse(response));
                        }
                        catch (Exception ex)
                        {
                            failure(ex);
                        }
                    });
                });
            }
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
        public override void Start()
        {
            using (var context = NetMQContext.Create())
            using (var server = context.CreateResponseSocket())
            {
                server.Bind(_url);
                var message = server.ReceiveFrameString();
                if (message.Any())
                    throw new InvalidOperationException();

                //handshake
                server.SendFrame(serverObjects());

                //loop
                for(;;)
                {
                    message = server.ReceiveFrameString();

                    try
                    {
                        JObject call = JObject.Parse(message);

                        var @object = Guid.Parse(call.GetValue("id").ToString());
                        var method = call.GetValue("method").ToString();
                        var args = JObject.FromObject(call.GetValue("args"));

                        Invoke(@object, method, args,
                            result => server.SendFrame(result.ToString()),
                            ex => server.SendFrame(exceptionJson(ex).ToString()));
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
