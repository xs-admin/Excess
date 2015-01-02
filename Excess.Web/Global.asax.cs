using System;
using Abp.Dependency;
using Abp.Web;
using Castle.Facilities.Logging;
using Excess.Core;
using Castle.MicroKernel.Registration;
using System.Web;

namespace Excess.Web
{
    public class MvcApplication : AbpWebApplication
    {
        protected override void Application_Start(object sender, EventArgs e)
        {
            IocManager.Instance.IocContainer.AddFacility<LoggingFacility>(f => f.UseLog4Net().WithConfig("log4net.config"));
            IocManager.Instance.IocContainer.Register(
                Component.For<IDSLFactory>()
                    .UsingFactoryMethod(() => {
                        var ctxFactory = HttpContext.Current.Session["dsl"] as IDSLFactory;
                        if (ctxFactory == null)
                            return new SimpleFactory();

                        return ctxFactory;
                    }).LifeStyle.Singleton);

            base.Application_Start(sender, e);
        }
    }
}
