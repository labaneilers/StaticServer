using System;
using System.Web.Mvc;
using System.Linq;
using System.Collections.Generic;
using StaticWww.Helpers;

namespace StaticWww
{
	public class BundleModelBinder : IModelBinder
	{
		public BundleModelBinder()
		{
		}

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			var shortGuidData = controllerContext.RequestContext.HttpContext.Request.QueryString.Get("d");
			IEnumerable<Guid> guids = shortGuidData.Split(',')
				.Select(x => ShortGuid.Decode(x));

			return guids;
		}
	}
}

