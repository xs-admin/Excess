using System.Reflection;
using Abp.Application.Services;
using Abp.Modules;
using Abp.WebApi;
using Abp.WebApi.Controllers.Dynamic.Builders;

namespace Excess
{
    [DependsOn(typeof(AbpWebApiModule), typeof(ExcessApplicationModule))]
    public class ExcessWebApiModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            DynamicApiControllerBuilder
                .ForAll<IApplicationService>(typeof(ExcessApplicationModule).Assembly, "app")
                .Build();
        }
    }
}
