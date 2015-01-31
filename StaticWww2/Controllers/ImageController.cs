using System;
using System.Web.Mvc;
using StaticWww2.Helpers;
using StaticWww2.Models;

namespace StaticWww2.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Index([ModelBinder(typeof(ResponsiveImageModelBinder))] ResponsiveImageModel model)
        {
            var renderer = new ImageRenderer
		    {
		        MapPath = Server.MapPath,
                UseFreeImage = this.HttpContext.Request.QueryString.Get("fi", Environment.Is64BitProcess)
		    };

            renderer.SetResponseHeaders(this.HttpContext, false, true);

            return new ImageWriterResult(stream => renderer.WriteImage(model, stream));
        }
    }
}