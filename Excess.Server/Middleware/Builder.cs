using System;
using Owin;
using Excess.Concurrent.Runtime;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Excess.Server.Middleware
{
    public class ConcurrentAppSettings
    {
        public ConcurrentAppSettings()
        {
            Threads = 4;
            BlockUntilNextEvent = true;
        }

        public int Threads { get; set; }
        public bool BlockUntilNextEvent { get; set; }

        public  void From(ConcurrentAppSettings settings)
        {
            Threads = settings.Threads;
            BlockUntilNextEvent = settings.BlockUntilNextEvent;
        }
    }

    public static class BuilderExtensions
    {
        public static void UseExcess(this IAppBuilder app, 
            Action<ConcurrentAppSettings> initializeSettings = null,
            Action<IDistributedApp> initializeApp = null,
            IEnumerable<Type> functional = null)
        {
            var settings = new ConcurrentAppSettings();
            initializeSettings?.Invoke(settings);

            var server = new DistributedApp(new ThreadedConcurrentApp(
                types: null,
                threadCount: settings.Threads,
                blockUntilNextEvent: settings.BlockUntilNextEvent));

            initializeApp?.Invoke(server);

            app.Use<ExcessOwinMiddleware>(server, functional);
            server.Start();
        }

        public static void UseExcess(this IAppBuilder builder,
            IEnumerable<Assembly> assemblies,
            ConcurrentAppSettings settings = null)
        {
            UseExcess(builder,
                initializeSettings: _settings =>
                {
                    if (settings != null)
                        _settings.From(settings);
                },
                initializeApp: app => Loader.FromAssemblies(app, assemblies),
                functional: assemblies
                    .SelectMany(assembly => assembly
                        .GetTypes()
                        .Where(type => type.Name == "Functions")));
        }

        public static void UseExcess<T>(this IAppBuilder builder, ConcurrentAppSettings settings = null)
        {
            UseExcess(builder, new[] { typeof(T).Assembly });
        }

        public static void UseExcess(this IAppBuilder builder, IConcurrentApp app)
        {
            UseExcess(builder, new DistributedApp(app));
        }

        public static void UseExcess(this IAppBuilder builder, IDistributedApp server)
        {
            builder.Use<ExcessOwinMiddleware>(server, null);
            server.Start();
        }


    }
}
