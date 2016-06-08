using System;
using Owin;
using Excess.Concurrent.Runtime;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace Middleware
{
    public class AppSettings
    {
        public AppSettings()
        {
            Threads = 4;
            BlockUntilNextEvent = true;
        }

        public int Threads { get; set; }
        public bool BlockUntilNextEvent { get; set; }

        public  void From(AppSettings settings)
        {
            Threads = settings.Threads;
            BlockUntilNextEvent = settings.BlockUntilNextEvent;
        }
    }

    public static class BuilderExtensions
    {
        public static void UseExcess(this IAppBuilder app, 
            Action<AppSettings> initializeSettings = null,
            Action<IDistributedApp> initializeApp = null)
        {
            var settings = new AppSettings();
            initializeSettings?.Invoke(settings);

            var server = new DistributedApp(new ThreadedConcurrentApp(
                types: null,
                threadCount: settings.Threads,
                blockUntilNextEvent: settings.BlockUntilNextEvent));

            initializeApp?.Invoke(server);

            app.Use<ExcessOwinMiddleware>(server);
            server.Start();
        }

        public static void UseExcess(this IAppBuilder builder,
            IEnumerable<Assembly> assemblies,
            AppSettings settings = null)
        {
            UseExcess(builder,
                initializeSettings: _settings =>
                {
                    if (settings != null)
                        _settings.From(settings);
                },
                initializeApp: app => Loader.FromAssemblies(app, assemblies));
        }

        public static void UseExcess<T>(this IAppBuilder builder, AppSettings settings = null)
        {
            UseExcess(builder, new[] { typeof(T).Assembly });
        }

        public static void UseExcess(this IAppBuilder builder, IConcurrentApp app)
        {
            UseExcess(builder, new DistributedApp(app));
        }

        public static void UseExcess(this IAppBuilder builder, IDistributedApp server)
        {
            builder.Use<ExcessOwinMiddleware>(server);
            server.Start();
        }
    }
}
