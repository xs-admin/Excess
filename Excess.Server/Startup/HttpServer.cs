using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owin;
using Microsoft.Owin.Hosting;
using Excess.Concurrent.Runtime;
using Middleware;
using Middleware.NetMQ;
using System.Threading;

namespace Startup
{
    public class HttpServer
    {
        public static void Start<T>(string baseUrl)
        {
            using (WebApp.Start<T>(baseUrl))
            {
                Console.WriteLine("Press Enter to quit.");
                Console.ReadKey();
            }
        }

        public static void Start(
            string url,
            string identityUrl = null,
            IInstantiator instantiator = null,
            int threads = 8,
            Action<IAppBuilder> onInit = null,
            bool useStaticFiles = true,
            int waitForClients = 0)
        {
            using (WebApp.Start(url, (app) =>
            {
                if (useStaticFiles)
                    app.UseStaticFiles();

                var concurrentServer = null as IConcurrentServer;
                app.UseConcurrent(server =>
                {
                    var failure = null as Exception;
                    var waiter = new ManualResetEvent(false);

                    var identityServer = new IdentityServer();
                    if (identityUrl != null)
                        identityServer.Start(identityUrl, waitForClients, exception =>
                        {
                            failure = exception;
                            waiter.Set();
                        });

                    server.Identity = identityServer;
                    concurrentServer = server;

                    var classes = instantiator.GetConcurrentClasses();
                    var instances = instantiator.GetConcurrentInstances();
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

                    waiter.WaitOne();
                    if (failure != null)
                        throw failure;
                });

                Debug.Assert(concurrentServer != null);
                if (onInit != null)
                    onInit(app);
            }))
            {
                Console.WriteLine("Press Enter to quit."); //td: lol
                Console.ReadKey();
            }
        }
    }
}
