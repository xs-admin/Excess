using System;
using System.Threading.Tasks;
using NetMQ;
using System.Collections.Concurrent;
using Excess.Concurrent.Runtime;

namespace Middleware.NetMQ
{
    public class IdentityClient : BaseIdentityServer
    {
        public bool Connected { get { return _connected == 2; } }

        int _connected = 0;
        ConcurrentDictionary<Guid, Action<string>> _awaiting = new ConcurrentDictionary<Guid, Action<string>>();
        public void Start(string inputUrl, string outputUrl, ReferenceInstantiator instantiator)
        {
            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var input = context.CreateDealerSocket())
                {
                    input.Bind(inputUrl);
                    _connected++;

                    var message = new NetMQMessage(expectedFrameCount: 5);
                    for (;;)
                    {
                        message = input.ReceiveMultipartMessage();

                        try
                        {
                            var id = Guid.Parse(message
                                .Pop()
                                .ConvertToString());

                            message.Pop(); //empty frame

                            var action = null as Action<string>;
                            if (_awaiting.TryRemove(id, out action))
                            {
                                var response = message.Pop().ConvertToString();
                                action(response);
                            }
                        }
                        catch
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
                    _connected++;

                    var message = new NetMQMessage(expectedFrameCount: 7);
                    for (;;)
                    {
                        var toSend = _writeQueue.Take();

                        message.Clear(); //td: best way to do this?
                        message.Append(toSend.Id.ToString());
                        message.AppendEmptyFrame();
                        message.Append(toSend.Method);
                        message.AppendEmptyFrame();
                        message.Append(toSend.Data);
                        message.AppendEmptyFrame();
                        message.Append(toSend.ResponseId.ToString());

                        output.SendMultipartMessage(message);
                    }
                }
            });

            instantiator.Dispatch = remoteDispatch;
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

            if (!_awaiting.TryAdd(responseId, response))
                throw new InvalidOperationException();
        }

        protected override void remoteRegister(Guid id)
        {
            _writeQueue.Add(new WriteRequest
            {
                Id = Guid.Empty,
                Method = "__register",
                Data = id.ToString()
            });
        }
    }
}
