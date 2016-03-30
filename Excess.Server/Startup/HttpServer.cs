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
            IEnumerable<Action<Action<Exception>>> connections = null,
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
                    if (connections != null && connections.Any())
                    {
                        var errors = new List<Exception>();
                        var waiter = new ManualResetEvent(false);
                        var count = connections.Count();
                        foreach (var connection in connections)
                        {
                            connection(ex =>
                            {
                                if (ex != null)
                                    errors.Add(ex);

                                count--;
                                if (count <= 0)
                                    waiter.Set();
                            });
                        }

                        waiter.WaitOne(); //td: timeout
                        if (errors.Any())
                            throw new AggregateException("cannot connect", errors);
                    }

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
            }))
            {
                Console.ReadKey();
            }
        }
    }
}
