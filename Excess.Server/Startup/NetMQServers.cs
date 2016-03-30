using Middleware;
using Excess.Concurrent.Runtime;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using System.Collections.Concurrent;
using System.Diagnostics;
using NetMQ.Sockets;

namespace Startup
{
    public static class NetMQNode
    {
        public static void Start(
            string localServer,
            string remoteServer,
            int threads = 2,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, IConcurrentObject>> instances = null)
        {
            var app = new DistributedConcurrentApp();
            if (classes != null)
            {
                foreach (var @class in classes)
                    app.RegisterClass(@class);
            }

            if (instances != null)
            {
                foreach (var instance in instances)
                    app.RegisterInstance(instance.Key, instance.Value);
            }


            app.Connect = ConnectMQ(app, localServer, remoteServer);
            app.Start();
        }

        private static Func<IDistributedApp, Exception> ConnectMQ(DistributedConcurrentApp app, string localServer, string remoteServer)
        {
            return _ =>
            {
                var waiter = new ManualResetEvent(false);
                var errors = new List<Exception>();
                int waitFor = 2;

                Task.Run(() =>
                {
                    using (var context = NetMQContext.Create())
                    using (var input = context.CreateDealerSocket())
                    {
                        try
                        {
                            input.Bind(localServer);
                        }
                        catch (Exception ex)
                        {
                            lock(errors)
                            {
                                errors.Add(ex);
                            }
                        }

                        if (--waitFor == 0)
                            waiter.Set();

                        var message = new NetMQMessage(expectedFrameCount: 7);
                        var appMessage = new DistributedAppMessage();
                        
                        //loop
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

                Task.Run(() =>
                {
                    using (var context = NetMQContext.Create())
                    using (var output = context.CreateDealerSocket())
                    {
                        try
                        {
                            output.Connect(remoteServer);
                        }
                        catch (Exception ex)
                        {
                            lock (errors)
                            {
                                errors.Add(ex);
                            }
                        }

                        var writeQueue = new BlockingCollection<DistributedAppMessage>();
                        app.Send = msg => writeQueue.Add(msg);

                        if (--waitFor == 0)
                            waiter.Set();

                        //loop
                        for (;;)
                        {
                            writeMessage(output, writeQueue.Take());
                            var toSend = writeQueue.Take();


                            Debug.WriteLine(
                                $"Client.Send: {toSend.Id} ==> {toSend.Method}, {toSend.Data} => {toSend.RequestId}");
                        }
                    }
                });

                waiter.WaitOne();
                return null;
            };
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
}
