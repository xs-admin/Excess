using Excess.Concurrent.Runtime;
using Middleware;
using NetMQ;
using NetMQ.Core;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin;
using System.IO;

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
        public override void Start()
        {
            base.Start();
            //var node = new Runtime.Node(5); //td: config
            //using(var context = NetMQContext.Create())
            //using (var server = context.CreateResponseSocket())
            //{
            //    server.Bind(url);
            //    var message = server.ReceiveFrameString();
            //    if (message.Any())
            //        throw new InvalidOperationException();

            //    server.SendFrame(serverObjects());
            //    while (true)
            //    {
            //        message = server.ReceiveFrameString();

            //        try
            //        {
            //            JObject call = JObject.Parse(message);

            //            var objectId = Guid.Parse(call.GetValue("id").ToString());
            //            var method = call.GetValue("method").ToString();
            //            var args = JObject.FromObject(call.GetValue("args"));

            //            var @object = _instances[objectId];
            //        }
            //        catch (Exception ex)
            //        {
            //        }


            //        // processing the request
            //        //hread.Sleep(100);

            //        Console.WriteLine("Sending World");
            //        server.SendFrame("World");
            //    }
            //}
        }
    }
}
