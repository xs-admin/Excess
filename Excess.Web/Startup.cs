using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Excess.Web.Startup))]
namespace Excess.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
