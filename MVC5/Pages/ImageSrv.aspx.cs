using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using MVC5.Models;
using StaticWww.Models;

namespace StaticWww.Pages
{
	public class ImageSrv : System.Web.UI.Page
	{
	    protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

		    var renderer = new ImageRenderer
		    {
		        MapPath = Server.MapPath
		    };

		    var qs = new ResponsiveImageQueryString(this.Context.Request.QueryString);

            renderer.SetResponseHeaders(new HttpContextWrapper(this.Context), false, true);

			renderer.WriteImage(qs, this.Response.OutputStream);
		}
	}
}

