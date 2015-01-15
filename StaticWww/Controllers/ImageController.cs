using System.Web.Mvc;
using StaticWww.Helpers;
using StaticWww.Models;

namespace StaticWww.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Index()
        {
            var renderer = new ImageRenderer
		    {
		        MapPath = Server.MapPath
		    };

		    var qs = new ResponsiveImageQueryString(this.HttpContext.Request.QueryString);

            renderer.SetResponseHeaders(this.HttpContext, false, true);

            return new ImageWriterResult(stream => renderer.WriteImage(qs, stream));
        }
    }
}