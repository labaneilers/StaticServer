using System.Globalization;
using System;

namespace StaticWwwHelpers
{
	/// <summary>
	/// The result of a lookup in a LocalizedManifestFile.
	/// Contains information about the virtualPath/language combination requested.
	/// </summary>
	public class LocalizedManifestLookupResult
	{
		/// <summary>
		/// If true, a manifest entry was found for the specified virtualPath.
		/// </summary>
		public bool Found { get; private set; }

		/// <summary>
		/// The virtual path (i.e. /merch/foo.png)
		/// </summary>
		public string VirtualPath { get; private set; }

		/// <summary>
		/// The translated virtual path (i.e. /merch/www/foo.png).
		/// This is the URL from the perspective of the IIS virtual directory structure.
		/// </summary>
		public string TranslatedVirtualPath { get; private set; }

		/// <summary>
		/// The language ID for which the manifest entry was found.
		/// This may be a parent of the language which was requested, if there was no match for the specified language.
		/// </summary>
		public CultureInfo CultureInfo { get; private set; }

		/// <summary>
		/// The VersionId of the manifest entry that was found.
		/// </summary>
		public Guid VersionId { get; private set; }

		public LocalizedManifestLookupResult(bool found, string virtualPath, string translatedVirtualPath, CultureInfo cultureInfo, Guid versionId)
		{
			this.Found = found;
			this.VirtualPath = virtualPath;
			this.TranslatedVirtualPath = translatedVirtualPath;
			this.CultureInfo = cultureInfo;
			this.VersionId = versionId;
		}

		public static LocalizedManifestLookupResult Empty(string virtualPath)
		{
			return new LocalizedManifestLookupResult(false, virtualPath, null, null, Guid.Empty);
		}
	}
}