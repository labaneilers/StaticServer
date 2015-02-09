using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using StaticWww.Helpers;

namespace StaticWww.Controllers
{
	public class BundleController : Controller
	{
		private IFileGuidMap _fileGuidMap;

		public BundleController(IFileGuidMap fileGuidMap)
		{
			_fileGuidMap = fileGuidMap;
		}

		// Example:
		//sV8n6WZAPb14J3_C3GgiDA

		public ActionResult Css([ModelBinder(typeof(BundleModelBinder))] IEnumerable<Guid> model)
		{
			var renderer = new BundleRenderer();

			renderer.SetResponseHeaders(this.HttpContext, "text/css", true);

			IEnumerable<string> virtualPaths = model.Select(x => _fileGuidMap.Get(x));

			return new StreamResult(stream => renderer.Render(virtualPaths, stream));
		}
	}
}

