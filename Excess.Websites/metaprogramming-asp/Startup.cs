using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Middleware;

[assembly: OwinStartup(typeof(metaprogramming_asp.Startup))]

namespace metaprogramming_asp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseExcess<Startup>();
        }
    }
}
