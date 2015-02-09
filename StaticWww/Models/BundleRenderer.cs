using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using System.Web;

namespace StaticWww
{
	public class BundleRenderer
	{
		public BundleRenderer()
		{
			this.MapPath = HostingEnvironment.MapPath;
		}

		public void Render(IEnumerable<string> virtualPaths, Stream stream)
		{
			foreach (string virtualPath in virtualPaths)
			{
				string physicalPath = this.MapPath(virtualPath);
				using (var fs = File.OpenRead(physicalPath))
				using (var writer = new StreamWriter(stream))
				{
					fs.CopyTo(stream);
					writer.WriteLine();
				}
			}
		}

		public void SetResponseHeaders(HttpContextBase context, string contentType, bool cacheable)
		{
			context.Response.ContentType = contentType;
			context.Response.Cache.SetCacheability(cacheable ? HttpCacheability.Public : HttpCacheability.NoCache);

			if (cacheable)
			{
				context.Response.Cache.SetMaxAge(new TimeSpan(364, 0, 0, 0));
				context.Response.Cache.SetLastModified(DateTime.Now.AddDays(-364));
			}
		}

		public Func<string, string> MapPath { private get; set; }
	}
}

