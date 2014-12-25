using System.Web.Mvc;

namespace Excess.Web.Controllers
{
    public class HomeController : ExcessControllerBase
    {
        public ActionResult Index()
        { 
            return View("~/App/Main/views/layout/layout.cshtml"); //Layout of the angular application.
        }
	}
}