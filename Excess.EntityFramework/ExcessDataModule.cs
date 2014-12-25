using System.Data.Entity;
using System.Reflection;
using Abp.EntityFramework;
using Abp.Modules;
using Excess.EntityFramework;

namespace Excess
{
    [DependsOn(typeof(AbpEntityFrameworkModule), typeof(ExcessCoreModule))]
    public class ExcessDataModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = "Default";
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            Database.SetInitializer<ExcessDbContext>(null);
        }
    }
}
