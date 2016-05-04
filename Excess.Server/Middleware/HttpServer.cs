using System;
using System.Threading;
using System.Collections.Generic;
using Owin;
using Microsoft.Owin.Hosting;
using Excess.Concurrent.Runtime;
using System.IO;

namespace Middleware
{
    public class HttpServer
    {
        public static void Start(
            string url,
            string identityUrl = null,
            int threads = 4,
            string staticFiles = null,
            int nodes = 0,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, Type>> instances = null)
        {
            using (WebApp.Start(url, (builder) =>
            {
                if (staticFiles != null)
                {
                    if (!Directory.Exists(staticFiles))
                        throw new ArgumentException(staticFiles);

                    builder.UseStaticFiles(staticFiles);
                }

                var distributed = null as IDistributedApp;
                builder.UseExcess(initializeApp: server =>
                {
                    distributed = server;

                    //setup
                    if (classes != null)
                    {
                        foreach (var @class in classes)
                            server.ConcurrentApp.RegisterClass(@class);
                    }

                    if (instances != null)
                    {
                        foreach (var instance in instances)
                        {
                            server.RegisterInstance(instance.Key, (IConcurrentObject)Activator.CreateInstance(instance.Value));
                        }
                    }

                    if (nodes > 0)
                    {
                        //start the identity server if we have any nodes
                        var error = null as Exception;
                        var waiter = new ManualResetEvent(false);

                        NetMQFunctions.StartServer(server, identityUrl, 
                            expectedClients: nodes,
                            connected: ex => 
                            {
                                error = ex;
                                waiter.Set();
                            });

                        waiter.WaitOne(); //td: timeout
                        if (error != null)
                            throw error;
                    }
                });
            }))
            {
                Console.ReadKey();
            }
        }
    }
}
