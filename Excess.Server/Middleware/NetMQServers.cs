using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using System.Collections.Concurrent;
using System.Diagnostics;
using NetMQ.Sockets;
using System.Linq;

namespace Middleware
{
    public static class NetMQFunctions
    {
        public static void StartServer(IDistributedApp app, string url, int expectedClients = 0, Action<Exception> connected = null)
        {
            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var input = context.CreateDealerSocket())
                {
                    Dictionary<string, DealerSocket> clients = null;
                    try
                    {
                        input.Bind(url);

                        if (expectedClients > 0)
                        {
                            clients = new Dictionary<string, DealerSocket>();
                            awaitClients(input, expectedClients, clients, context);
                        }

                        connected(null);
                    }
                    catch (Exception ex)
                    {
                        connected(ex);
                        return;
                    }

                    //loop
                    var message = new NetMQMessage(expectedFrameCount: 7);
                    var appMessage = new DistributedAppMessage();
                    for (;;)
                    {
                        message = input.ReceiveMultipartMessage(expectedFrameCount: 7);

                        try
                        {
                            if (!readMessage(message, appMessage))
                                continue;

                            app.Receive(appMessage);
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
        }

        public static void StartClient(IDistributedApp app, string url, Action<Exception> connected)
        {
            var writeQueue = new BlockingCollection<DistributedAppMessage>();
            app.Send = msg => writeQueue.Add(msg);

            Task.Run(() =>
            {
                using (var context = NetMQContext.Create())
                using (var output = context.CreateDealerSocket())
                {
                    try
                    {
                        output.Connect(url);
                    }
                    catch (Exception ex)
                    {
                        connected(ex);
                        return;
                    }

                    //loop
                    for (;;)
                    {
                        var toSend = writeQueue.Take();
                        writeMessage(output, toSend);

                        Debug.WriteLine(
                            $"Client.Send: {toSend.Id} ==> {toSend.Method}, {toSend.Data} => {toSend.RequestId}");
                    }
                }
            });
        }

        private static void awaitClients(DealerSocket input, int clients, Dictionary<string, DealerSocket> result, NetMQContext context)
        {
            while (result.Count <  clients)
            {
                var clientCmd = input.ReceiveFrameString();
                switch (clientCmd)
                {
                    case "hi":
                        var clientUrl = input.ReceiveFrameString();
                        var dealer = context.CreateDealerSocket();
                        dealer.Connect(clientUrl);

                        result[clientUrl] = dealer;
                        break;
                    default:
                        throw new ArgumentException(clientCmd);
                }
            }
        }

        private static bool readMessage(NetMQMessage message, DistributedAppMessage appMessage)
        {
            try
            {
                appMessage.Id = Guid.Parse(message.Pop().ConvertToString());
                message.Pop();
                appMessage.Method = message.Pop().ConvertToString();
                message.Pop();
                appMessage.Data = message.Pop().ConvertToString();
                message.Pop();
                appMessage.RequestId = Guid.Parse(message.Pop().ConvertToString());
            }
            catch
            {
                //td: what?
                return false;
            }

            return true;
        }

        private static void writeMessage(DealerSocket output, DistributedAppMessage appMessage)
        {
            output.SendFrame(appMessage.Id.ToString(), more: true);
            output.SendFrameEmpty(more: true);
            output.SendFrame(appMessage.Method ?? string.Empty, more: true);
            output.SendFrameEmpty(more: true);
            output.SendFrame(appMessage.Data, more: true);
            output.SendFrameEmpty(more: true);
            output.SendFrame(appMessage.RequestId.ToString());
        }
    }

    public static class NetMQNode
    {
        public static void Start(
            string localServer,
            string remoteServer,
            int threads = 2,
            IEnumerable<Type> classes = null,
            IDictionary<Guid, IConcurrentObject> managedInstances = null)
        {
            var concurrentApp = new ThreadedConcurrentApp(new Dictionary<string, Func<IConcurrentApp, object[], IConcurrentObject>>());
            var app = new DistributedApp(concurrentApp);
            if (classes != null)
            {
                foreach (var @class in classes)
                {
                    Guid id;
                    IConcurrentObject @object;

                    app.RegisterClass(@class);
                    if (isConcurrentSingleton(@class, out id, out @object))
                    {
                        if (managedInstances != null)
                            managedInstances[id] = @object;

                        app.RegisterInstance(id, @object);
                    }
                }
            }

            app.Connect = _ =>
            {
                var waiter = new ManualResetEvent(false);
                var errors = new List<Exception>();
                int waitFor = 2;

                NetMQFunctions.StartServer(app, localServer, 
                    connected: ex =>
                    {
                        if (ex != null) errors.Add(ex);
                        if (--waitFor == 0) waiter.Set();
                    });

                NetMQFunctions.StartClient(app, remoteServer, ex =>
                {
                    if (ex != null) errors.Add(ex);
                    if (--waitFor == 0) waiter.Set();
                });

                waiter.WaitOne();
                return errors.Any()
                    ? new AggregateException(errors)
                    : null;
            };

            app.Start();
        }

        private static bool isConcurrentSingleton(Type type, out Guid id, out IConcurrentObject @object)
        {
            var attribute = type
                .CustomAttributes
                .Where(attr => attr.AttributeType.Name == "ConcurrentSingleton")
                .SingleOrDefault();

            if (attribute != null && attribute.ConstructorArguments.Count == 1)
            {
                id = Guid.Parse((string)attribute.ConstructorArguments[0].Value);
                @object = (IConcurrentObject)Activator.CreateInstance(type);
                return true;
            }

            id = Guid.Empty;
            @object = null;
            return false;
        }
    }
}
