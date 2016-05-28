using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using Owin;
using Microsoft.Owin.Hosting;
using Excess.Concurrent.Runtime;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin;
using Excess.Runtime;
using System.Reflection;

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
            IEnumerable<Assembly> assemblies = null,
            IEnumerable<Type> classes = null,
            IEnumerable<KeyValuePair<Guid, Type>> instances = null)
        {
            if (assemblies != null)
                Application.Start(assemblies);

            var instantiator = Application.GetService<IInstantiator>()
                ?? new DefaultInstantiator();

            using (WebApp.Start(url, (builder) =>
            {
                if (staticFiles != null)
                {
                    if (!Directory.Exists(staticFiles))
                        throw new ArgumentException(staticFiles);

                    staticFiles = Path.GetFullPath(staticFiles);

                    var physicalFileSystem = new PhysicalFileSystem(staticFiles);
                    var options = new FileServerOptions
                    {
                        EnableDefaultFiles = true,
                        FileSystem = physicalFileSystem
                    };
                    options.StaticFileOptions.FileSystem = physicalFileSystem;
                    options.StaticFileOptions.ServeUnknownFileTypes = true;
                    options.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };

                    builder.UseFileServer(options);
                }

                var distributed = null as IDistributedApp;
                builder.UseExcess(initializeApp: server =>
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
                        {
                            server.RegisterInstance(instance.Key, (IConcurrentObject)instantiator.Create(instance.Value)); 
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
