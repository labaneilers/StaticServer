using System;

namespace StaticWwwHelpers
{
	public class StaticUrlHelpers
	{
		public string StaticRootUrl { private get; set; }

		public StaticUrlHelpers()
		{
		}

		public string GetUrl(string virtualPath)
		{
			return this.StaticRootUrl + virtualPath;
		}
	}
}

/*

* manifest parser
* short guids need to go in a library, or just copy the code to reduce dependencies?

 */

