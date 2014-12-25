using System.Reflection;
using Abp.Modules;

namespace Excess
{
    [DependsOn(typeof(ExcessCoreModule))]
    public class ExcessApplicationModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
