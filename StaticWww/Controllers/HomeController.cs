using System.Web.Mvc;

namespace StaticWww.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}