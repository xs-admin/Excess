using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owin;
using Microsoft.Owin.Hosting;
using Excess.Concurrent.Runtime;
using Middleware;
using Middleware.NetMQ;

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
            bool useStaticFiles = true)
        {
            using (WebApp.Start(url, (app) =>
            {
                if (useStaticFiles)
                    app.UseStaticFiles();

                var concurrentServer = null as IConcurrentServer;
                app.UseConcurrent(server =>
                {
                    var identityServer = new IdentityServer();
                    if (identityUrl != null)
                        identityServer.Start(identityUrl);

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
                });

                Debug.Assert(concurrentServer != null);
                if (onInit != null)
                    onInit(app);

                concurrentServer.Start();
            }))
            {
                Console.WriteLine("Press Enter to quit."); //td: lol
                Console.ReadKey();
            }
        }
    }
}
