using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Middleware.NetMQ
{
    public class IdentityServer : BaseIdentityServer
    {
        ConcurrentDictionary<Guid, Action<string>> _response = new ConcurrentDictionary<Guid, Action<string>>();
        public void Start(string url, int clients, Action<Exception> started)
        {
            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var socket = context.CreateRouterSocket())
                {
                    try
                    {
                        socket.Bind(url);
                    }
                    catch (Exception ex)
                    {
                        started(ex);
                        return;
                    }

                    if (clients == 0)
                        started(null);
                    var message = new NetMQMessage(expectedFrameCount: 8);
                    for (;;)
                    {
                        message = socket.ReceiveMultipartMessage(expectedFrameCount: 8);
                        message.Pop(); //address

                        try
                        {
                            var guid = message.Pop().ConvertToString();
                            var id = Guid.Parse(guid);
                            message.Pop(); //empty frame
                            var method = message.Pop().ConvertToString();
                            message.Pop(); //empty frame
                            var data = message.Pop().ConvertToString();
                            message.Pop(); //empty frame
                            var responseId = Guid.Parse(message.Pop().ConvertToString());

                            switch (method)
                            {
                                case "__register":
                                    Debug.WriteLine($"Server.Register: {id} ==> {data}");
                                    registerRemote(id, data);
                                    break;

                                case "__registerServer":
                                    Debug.WriteLine($"Server.RegisterServer: {data}");
                                    registerServer(data, context);
                                    if (--clients == 0)
                                        started(null);
                                    break;
                                default:
                                    if (id != Guid.Empty)
                                    {
                                        Debug.WriteLine($"Server.Dispatch: {id}, {method}, {data}");

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
                                    }
                                    else
                                    {
                                        Debug.Assert(responseId != Guid.Empty);
                                        Debug.WriteLine($"Server.Response: {responseId} ==> {data}");


                                        var action = null as Action<string>;
                                        if (_response.TryRemove(responseId, out action))
                                            action(data);
                                        else
                                            throw new InvalidOperationException("response not found");
                                    }
                                    break;
                            }
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine(ex.Message); //td:
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

                        Debug.Assert(message.Id != Guid.Empty || message.ResponseId != Guid.Empty);
                        responseSocket.SendFrame(message.Id.ToString(), more: true);
                        responseSocket.SendFrameEmpty(more: true);
                        responseSocket.SendFrame(message.Method, more: true);
                        responseSocket.SendFrameEmpty(more: true);
                        responseSocket.SendFrame(message.Data, more: true);
                        responseSocket.SendFrameEmpty(more: true);
                        responseSocket.SendFrame(message.ResponseId.ToString());

                        Debug.WriteLine($"Server.Send: {message.Id} ==>  {message.Method}, {message.Data}");
                    }
                    catch(Exception ex)
                    {
                        //td: handle
                        Debug.WriteLine(ex.Message);
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
            public Guid Id;
            public string Method;
            public string Data;
        }

        BlockingCollection<WriteRequest> _writeQueue = new BlockingCollection<WriteRequest>();
        protected override void remoteDispatch(Guid id, string method, string data, Action<string> response)
        {
            var socket = null as DealerSocket;
            if (_remotes.TryGetValue(id, out socket))
            {
                var responseId = Guid.NewGuid();
                _response[responseId] = responseData => response(responseData);

                _writeQueue.Add(new WriteRequest
                {
                    Socket = socket,
                    Id = id,
                    Method = method,
                    Data = data,
                    ResponseId = responseId

                });
            }
            else
                response($"{{\"__ex\": \"not found: {id}\"}}");
        }
    }
}
