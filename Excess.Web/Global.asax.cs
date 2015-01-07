using System;
using Castle.Facilities.Logging;
using Excess.Core;
using Castle.MicroKernel.Registration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;
using System.Data.Entity.Migrations;

namespace Excess.Web
{
    public class MvcApplication : HttpApplication
    {
        protected virtual void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected virtual void Application_End(object sender, EventArgs e)
        {
        }

        protected virtual void Session_Start(object sender, EventArgs e)
        {
        }

        protected virtual void Session_End(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// This method is called by ASP.NET system when a request starts.
        /// </summary>
        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// This method is called by ASP.NET system when a request ends.
        /// </summary>
        protected virtual void Application_EndRequest(object sender, EventArgs e)
        {
        }

        protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected virtual void Application_Error(object sender, EventArgs e)
        {

        }
        
        //public override void Init()
        //{
        //    //IocManager.Instance.IocContainer.AddFacility<LoggingFacility>(f => f.UseLog4Net().WithConfig("log4net.config"));
        //    //IocManager.Instance.IocContainer.Register(
        //    //    Component.For<IDSLFactory>()
        //    //        .UsingFactoryMethod(() => {
        //    //            var ctxFactory = HttpContext.Current.Session["dsl"] as IDSLFactory;
        //    //            if (ctxFactory == null)
        //    //                return new SimpleFactory();

        //    //            return ctxFactory;
        //    //        }).LifeStyle.Singleton);

        //    //base.Application_Start(sender, e);
        //}
    }
}
