using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using StaticWww.Models;

namespace StaticWww.Controllers
{
    public class ImageController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

		public FileResult Image()
		{
			var qs = new ResponsiveImageQueryString(this.HttpContext.Request.QueryString);

			var stream = new MemoryStream();
			WriteImage(qs, stream);
			var result = new FileStreamResult(stream, "image/jpeg");

			return result;
		}

		private void SetResponseHeaders(bool isPng, bool cacheable)
		{
			this.HttpContext.Response.ContentType = isPng ? "image/png" : "image/jpeg";
			this.HttpContext.Response.Cache.SetCacheability(cacheable ? HttpCacheability.Public : HttpCacheability.NoCache);

			if (cacheable)
			{
				this.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(364, 0, 0, 0));
				this.HttpContext.Response.Cache.SetLastModified(DateTime.Now.AddDays(-364));
			}
		}

		private void WriteImage(ResponsiveImageQueryString qs, Stream stream)
		{
			string physicalPath = Server.MapPath(qs.Src);

			bool isPng = qs.PngColors >= 2 && qs.PngColors <= 255;
			bool isJpeg = qs.JpegQuality > 0 && qs.JpegQuality <= 100;
			var destRect = new Rectangle(0, 0, qs.Width, qs.Height);

			// Determine the natural size of the image
			using (var sourceImage = System.Drawing.Image.FromFile(physicalPath))
			{
				var naturalSize = sourceImage.Size;

				using (var outputBitmap = new Bitmap(qs.Width, qs.Height, PixelFormat.Format32bppArgb))
				using (var graphics = Graphics.FromImage(outputBitmap))
				{
					// Use bicubic resampling because it looks silky smooth for text and other high-contrast details.
					// We are willing to use more expensive operations here because this image will be rendered once and
					// then cached in the CDN.
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
					graphics.SmoothingMode = SmoothingMode.HighQuality;

					// If we're rendering as a JPEG, we can't support alpha transparency.
					// Set the background to white.
					if (!isPng)
					{
						graphics.Clear(Color.White);
					}

					using (var imageAttributes = new ImageAttributes())
					{
						// WrapMode.TileFlipXY prevents edges from resampling against white neighbor pixels.
						// This prevents a halo from appearing around edges.
						// http://stackoverflow.com/questions/1890605/ghost-borders-ringing-when-resizing-in-gdi
						imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

						graphics.DrawImage(
							sourceImage,
							destRect, 
							0, 
							0, 
							naturalSize.Width, 
							naturalSize.Height, 
							GraphicsUnit.Pixel, 
							imageAttributes);
					}

					outputBitmap.Save(stream, ImageFormat.Jpeg);
				}
			}
		}
    }


}
