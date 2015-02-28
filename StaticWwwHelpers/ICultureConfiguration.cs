using System;
using System.Globalization;
using System.Collections.Generic;

namespace StaticWwwHelpers
{
	public interface ICultureConfiguration
	{
		CultureInfo GetCultureForDirectoryName(string directoryName);
		string GetDirectoryNameForCulture(CultureInfo cultureInfo);
		CultureInfo GetParent(CultureInfo cultureInfo);
		IEnumerable<CultureInfo> GetSupportedCultures();
	}
}

