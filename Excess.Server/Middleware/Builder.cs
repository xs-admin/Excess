using System;
using Owin;
using Excess.Concurrent.Runtime;

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
    }

    public static class BuilderExtensions
    {
        public static void UseExcess(this IAppBuilder app, 
            Action<AppSettings> initializeSettings = null,
            Action<IDistributedApp> initializeApp = null)
        {
            var settings = new AppSettings();
            if (initializeSettings != null)
                initializeSettings(settings);

            var server = new DistributedApp(new ThreadedConcurrentApp(
                types: null,
                threadCount: settings.Threads,
                blockUntilNextEvent: settings.BlockUntilNextEvent));

            if (initializeApp != null)
                initializeApp(server);

            app.Use<ExcessOwinMiddleware>(server);
            server.Start();
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
