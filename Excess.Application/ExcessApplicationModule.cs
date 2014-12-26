using System.Reflection;
using Abp.Modules;

namespace Excess
{
    public class ExcessApplicationModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
        }
    }
}
