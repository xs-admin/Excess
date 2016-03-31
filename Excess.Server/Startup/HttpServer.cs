using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using Owin;
using Microsoft.Owin.Hosting;
using Middleware;
using Excess.Concurrent.Runtime;

namespace Startup
{
    public class HttpServer
    {
        public static void Start(
            string url,
            string identityUrl = null,
            int threads = 4,
            bool useStaticFiles = true,
            int nodes = 0,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, IConcurrentObject>> instances = null)
        {
            using (WebApp.Start(url, (app) =>
            {
                if (useStaticFiles)
                    app.UseStaticFiles();

                var distributed = null as IDistributedApp;
                app.UseExcess(server =>
                {
                    distributed = server;

                    //setup
                    if (classes != null)
                    {
                        foreach (var @class in classes)
                            server.RegisterClass(@class);
                    }

                    if (instances != null)
                    {
                        foreach (var instance in instances)
                            server.RegisterInstance(instance.Key, instance.Value);
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
