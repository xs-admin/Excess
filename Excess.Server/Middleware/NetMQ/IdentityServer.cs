using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Middleware.NetMQ
{
    public class IdentityServer : BaseIdentityServer
    {
        public void Start(string url)
        {
            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var socket = context.CreateRouterSocket())
                {
                    socket.Bind(url);

                    var message = new NetMQMessage(expectedFrameCount: 7);
                    for (;;)
                    {
                        message = socket.ReceiveMultipartMessage();

                        try
                        {
                            var id = Guid.Parse(message.Pop().ConvertToString());
                            message.Pop(); //empty frame
                            var method = message.Pop().ConvertToString();
                            message.Pop(); //empty frame
                            var data = message.Pop().ConvertToString();
                            message.Pop(); //empty frame
                            var responseId = Guid.Parse(message.Pop().ConvertToString());

                            switch (method)
                            {
                                case "__register":
                                    registerRemote(id, data);
                                    break;

                                case "__registerServer":
                                    registerServer(data, context);
                                    break;
                                default:
                                    var responseSocket = _remotes[id];
                                    dispatch(id, method, data, response =>
                                    {
                                        //queue to send
                                        _writeQueue.Add(new WriteRequest
                                        {
                                            Socket = responseSocket,
                                            ResponseId = responseId,
                                            Data = response
                                        });
                                    });
                                    break;
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            message.Clear();
                        }
                    }
                }
            });

            //writes
            Task.Run(() =>
            {
                for (;;)
                {
                    var message = _writeQueue.Take();
                    try
                    {
                        var responseSocket = message.Socket;
                        responseSocket.SendFrame(message.ResponseId.ToString(), more: true);
                        responseSocket.SendFrameEmpty(more: true);
                        responseSocket.SendFrame(message.Data);
                    }
                    catch
                    {
                        //td: handle
                    }
                }
            });
        }

        ConcurrentDictionary<string, DealerSocket> _servers = new ConcurrentDictionary<string, DealerSocket>();
        private void registerServer(string url, NetMQContext context)
        {
            if (!_servers.ContainsKey(url))
            {
                var socket = context.CreateDealerSocket();
                socket.Connect(url);
                _servers.TryAdd(url, socket);
            }
        }

        ConcurrentDictionary<Guid, DealerSocket> _remotes = new ConcurrentDictionary<Guid, DealerSocket>();
        private void registerRemote(Guid id, string data)
        {
            _remotes[id] = _servers[data];
        }

        class WriteRequest
        {
            public DealerSocket Socket;
            public Guid ResponseId;
            public string Data;
        }

        BlockingCollection<WriteRequest> _writeQueue = new BlockingCollection<WriteRequest>();
    }
}
