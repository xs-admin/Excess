using Abp.Web.Mvc.Controllers;

namespace Excess.Web.Controllers
{
    /// <summary>
    /// Derive all Controllers from this class.
    /// </summary>
    public abstract class ExcessControllerBase : AbpController
    {
        protected ExcessControllerBase()
        {
            LocalizationSourceName = ExcessConsts.LocalizationSourceName;
        }
    }
}