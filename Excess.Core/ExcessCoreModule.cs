using System.Reflection;
using Abp.Modules;

namespace Excess
{
    public class ExcessCoreModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
