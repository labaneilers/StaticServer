using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using MVC5.Models;
using StaticWww.Models;

namespace MVC5.Controllers
{
    public class ImageWriterResult : ActionResult
    {
        private readonly Action<Stream> _write;

        public ImageWriterResult(Action<Stream> write)
        {
            _write = write;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            _write(context.HttpContext.Response.OutputStream);
        }
    }

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Image()
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