using System.Web.Mvc;
using System;
using StaticWww.Helpers;

namespace StaticWww.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

		public ActionResult ShortGuid(string id)
		{
			var guid = new Guid(id);
			var shortGuid = StaticWww.Helpers.ShortGuid.Encode(guid);
			return Content(shortGuid);
		}
    }
}