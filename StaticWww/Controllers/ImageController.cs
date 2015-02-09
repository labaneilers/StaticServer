using System;
using System.Web.Mvc;
using StaticWww.Helpers;
using StaticWww.Models;

namespace StaticWww.Controllers
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

            return new StreamResult(stream => renderer.WriteImage(model, stream));
        }
    }
}