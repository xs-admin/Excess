using Abp.Application.Navigation;
using Abp.Localization;

namespace Excess.Web
{
    /// <summary>
    /// This class defines menus for the application.
    /// It uses ABP's menu system.
    /// When you add menu items here, they are automatically appear in angular application.
    /// See .cshtml and .js files under App/Main/views/layout/header to know how to render menu.
    /// </summary>
    public class ExcessNavigationProvider : NavigationProvider
    {
        public override void SetNavigation(INavigationProviderContext context)
        {
            context.Manager.MainMenu
                .AddItem(
                    new MenuItemDefinition(
                        "Home",
                        new LocalizableString("HomePage", "Home"),
                        url: "#/",
                        icon: "fa fa-home"
                        )
                ).AddItem(
                    new MenuItemDefinition(
                        "Project",
                        new LocalizableString("Project", "Project"),
                        url: "#/project",
                        icon: "fa fa-info"
                        )
                );

            //context.Manager.Menus["Project"] = new MenuDefinition("Project", new LocalizableString("Project", ExcessConsts.LocalizationProject))
            //    .AddItem(
            //        new MenuItemDefinition(
            //            "Home",
            //            new LocalizableString("HomePage", ExcessConsts.LocalizationSourceName),
            //            url: "#/",
            //            icon: "fa fa-home"
            //            )
            //    );
        }
    }
}
