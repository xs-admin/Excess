using Abp.Application.Services;

namespace Excess
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class ExcessAppServiceBase : ApplicationService
    {
        protected ExcessAppServiceBase()
        {
            LocalizationSourceName = ExcessConsts.LocalizationSourceName;
        }
    }
}