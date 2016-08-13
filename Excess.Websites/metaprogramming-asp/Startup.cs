using Owin;
using Microsoft.Owin;
using Excess.Server.Middleware;

[assembly: OwinStartup(typeof(metaprogramming.asp.Startup))]

namespace metaprogramming.asp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseExcess<Startup>();
        }
    }
}
