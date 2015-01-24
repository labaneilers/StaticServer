using System;
using System.IO;
using System.Web.Mvc;

namespace StaticWww.Helpers
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
}