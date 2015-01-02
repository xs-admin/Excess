
using System.Web.Mvc;

namespace Excess.Web.Views
{
    public abstract class ExcessWebViewPageBase : ExcessWebViewPageBase<dynamic>
    {

    }

    public abstract class ExcessWebViewPageBase<TModel> : WebViewPage<TModel>
    {
        protected ExcessWebViewPageBase()
        {
            //LocalizationSourceName = ExcessConsts.LocalizationSourceName;
        }
    }
}