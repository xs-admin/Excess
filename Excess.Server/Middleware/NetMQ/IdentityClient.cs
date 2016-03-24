using System;
using System.Threading.Tasks;
using NetMQ;
using System.Collections.Concurrent;
using Excess.Concurrent.Runtime;
using System.Diagnostics;

namespace Middleware.NetMQ
{
    public class IdentityClient : BaseIdentityServer
    {
        string _url;
        public IdentityClient(string url)
        {
            _url = url;

            //handshake
            _writeQueue.Add(new WriteRequest
            {
                Id = Guid.Empty,
                Method = "__registerServer",
                Data = url
            });
        }

        public bool Connected { get { return _connected == 2; } }

        int _connected = 0;
        ConcurrentDictionary<Guid, Action<string>> _pending = new ConcurrentDictionary<Guid, Action<string>>();
        public void Start(
            string inputUrl, 
            string outputUrl, 
            ReferenceInstantiator instantiator,
            Action<Exception> started = null)
        {
            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var input = context.CreateDealerSocket())
                {
                    input.Bind(inputUrl);
                    notifyConnected(started);

                    var message = new NetMQMessage(expectedFrameCount: 3);
                    for (;;)
                    {
                        message = input.ReceiveMultipartMessage(expectedFrameCount: 3);

                        try
                        {
                            //these messages are dual, 
                            //they could represent a request for execution
                            //or the return value of an invocation. Either way both keep 
                            //a requestID.
                            var id = Guid.Parse(message.Pop().ConvertToString());
                            message.Pop(); //empty frame
                            var method = message.Pop().ConvertToString();
                            message.Pop();
                            var data = message.Pop().ConvertToString();
                            message.Pop();
                            var requestId = Guid.Parse(message.Pop().ConvertToString());

                            var action = null as Action<string>;
                            if (_pending.TryRemove(requestId, out action))
                            {
                                Debug.Assert(id == Guid.Empty && string.IsNullOrEmpty(method)); //return value, no id or method

                                Debug.WriteLine(
                                    $"Client.ReceiveResponse: {id} ==> {data}");

                                action(data);
                            }
                            else
                            {
                                Debug.Assert(id != Guid.Empty
                                    && !string.IsNullOrWhiteSpace(method) 
                                    && !string.IsNullOrWhiteSpace(data));

                                Debug.WriteLine(
                                    $"Client.ReceiveRequest: {id} ==> {method}, {data}");

                                bool dispatched = localDispatch(id, method, data, response =>
                                {
                                    _writeQueue.Add(new WriteRequest
                                    {
                                        ResponseId = requestId,
                                        Data = response
                                    });
                                });

                                if (!dispatched)
                                    _writeQueue.Add(new WriteRequest
                                    {
                                        ResponseId = requestId,
                                        Data = $"{id} not found"
                                    }); //td: write functions
                            }
                        }
                        catch
                        {
                            //td: log?
                        }
                        finally
                        {
                            message.Clear();
                        }
                    }
                }
            });

            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var output = context.CreateDealerSocket())
                {
                    output.Connect(outputUrl);
                    notifyConnected(started);

                    //loop
                    var message = new NetMQMessage(expectedFrameCount: 7);
                    for (;;)
                    {
                        var toSend = _writeQueue.Take();

                        message.Clear(); //td: best way to do this?
                        message.Append(toSend.Id.ToString());
                        message.AppendEmptyFrame();
                        message.Append(toSend.Method ?? string.Empty);
                        message.AppendEmptyFrame();
                        message.Append(toSend.Data);
                        message.AppendEmptyFrame();
                        message.Append(toSend.ResponseId.ToString());

                        output.SendMultipartMessage(message);
                        Debug.WriteLine(
                            $"Client.Send: {toSend.Id} ==> {toSend.Method}, {toSend.Data} => {toSend.ResponseId}");
                    }
                }
            });

            instantiator.Dispatch = remoteDispatch;
        }

        private void notifyConnected(Action<Exception> started)
        {
            _connected++;
            if (_connected == 2 && started != null)
                started(null);
        }

        class WriteRequest
        {
            public Guid Id;
            public Guid ResponseId;
            public string Method;
            public string Data;
        }

        BlockingCollection<WriteRequest> _writeQueue = new BlockingCollection<WriteRequest>();
        protected override void remoteDispatch(Guid id, string method, string data, Action<string> response)
        {
            var responseId = Guid.NewGuid();
            _writeQueue.Add(new WriteRequest
            {
                Id = id,
                ResponseId = responseId,
                Method = method,
                Data = data
            });

            if (!_pending.TryAdd(responseId, response))
                throw new InvalidOperationException();
        }

        protected override void remoteRegister(Guid id)
        {
            _writeQueue.Add(new WriteRequest
            {
                Id = id,
                Method = "__register",
                Data = _url
            });
        }
    }
}
