using System;
using System.Collections.Generic;
using System.Diagnostics;
using Owin;
using Microsoft.Owin.Hosting;
using Excess.Concurrent.Runtime;
using Middleware;

namespace Startup
{
    public static class HttpServer
    {
        public static void Start<T>(string baseUrl)
        {
            using (WebApp.Start<T>(baseUrl))
            {
                Console.WriteLine("Press Enter to quit.");
                Console.ReadKey();
            }
        }

        public static void Start(string baseUrl, 
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, ConcurrentObject>> instances = null,
            Action<IAppBuilder> onInit = null,
            bool useStaticFiles = true)
        {
            using (WebApp.Start(baseUrl, (app) =>
            {
                if (useStaticFiles)
                    app.UseStaticFiles();

                var concurrentServer = null as IConcurrentServer;
                app.UseConcurrent(server =>
                {
                    concurrentServer = server;
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
