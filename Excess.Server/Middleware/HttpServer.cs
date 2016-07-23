using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin.FileSystems;
using Excess.Runtime;

namespace Excess.Server.Middleware
{
    using FilterFunction = Func<
        Func<string, IOwinRequest, __Scope, object>,  //prev
        Func<string, IOwinRequest, __Scope, object>>; //next

    public class HttpServer
    {
        public static void Start(
            string url,
            string identityUrl = null,
            int threads = 4,
            string staticFiles = null,
            int nodes = 0,
            IEnumerable<Assembly> assemblies = null,
            IEnumerable<string> except = null,
            IEnumerable<FilterFunction> filters = null)
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

                builder.UseExcess(
                    initializeApp: server =>
                    {
                        Loader.FromAssemblies(server, assemblies, except);

                        if (nodes > 0)
                        {
                            throw new NotImplementedException();

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
                    }, 
                    filters: filters);
            }))
            {
                Console.ReadKey();
            }
        }
    }
}
