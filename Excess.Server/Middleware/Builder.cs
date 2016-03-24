using System;
using Owin;

namespace Middleware
{
    public static class BuilderExtensions
    {
        public static void UseConcurrent(this IAppBuilder app, Action<IConcurrentServer> initialize = null)
        {
            var server = new ConcurrentServer();
            if (initialize != null)
                initialize(server);

            if (server.Identity == null)
                server.Identity = new BaseIdentityServer();

            app.Use<ConcurrentOwinMiddleware>(server);
            server.StartListening();
        }
    }
}
