using System;
using Owin;
using Excess.Concurrent.Runtime;

namespace Middleware
{
    public static class BuilderExtensions
    {
        public static void UseExcess(this IAppBuilder app, Action<IDistributedApp> initializer = null)
        {
            var server = new DistributedConcurrentApp();
            if (initializer != null)
                initializer(server);

            app.Use<ExcessOwinMiddleware>(server);
            server.Start();
        }
    }
}
