using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;

namespace StaticWww.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			var mvcName = typeof(Controller).Assembly.GetName();
			//var isMono = Type.GetType("Mono.Runtime") != null;

			ViewData["Version"] = mvcName.Version.Major + "." + mvcName.Version.Minor;
			ViewData["Runtime"] = typeof(System.Drawing.Bitmap).Assembly.Location;

			string physicalPath = Server.MapPath("/StaticFiles/lips.png");
			using (var sourceImage = System.Drawing.Image.FromFile(physicalPath))
			{
			}

			return View();
		}
	}
}

