using Abp.Web.Mvc.Views;

namespace Excess.Web.Views
{
    public abstract class ExcessWebViewPageBase : ExcessWebViewPageBase<dynamic>
    {

    }

    public abstract class ExcessWebViewPageBase<TModel> : AbpWebViewPage<TModel>
    {
        protected ExcessWebViewPageBase()
        {
            LocalizationSourceName = ExcessConsts.LocalizationSourceName;
        }
    }
}