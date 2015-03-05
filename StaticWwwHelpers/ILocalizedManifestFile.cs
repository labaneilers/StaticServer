using System;
using System.Collections.Generic;
using System.Globalization;

namespace StaticWwwHelpers
{
	/// <summary>
	/// Interface for a localization strategy using a sparse directory tree
	/// which has been processed by Hashly, and has an associated manifest file.
	/// This is governed by the Language domain data class's inheritance rules,
	/// and the directory names are defined in the domain data as well.
	/// </summary>
	public interface ILocalizedManifestFile
	{
		string RootVirtualDirectory { get; }

		/// <summary>
		/// Takes a canonical root-relative path and returns a structure which contains the "versioned" virtual path, 
		/// the physical path, and information about the language.
		/// </summary>
		/// <param name="virtualPath">A canonical root-relative path for a static file.</param>
		/// <param name="language">The language for which to get a result.</param>
		/// <param name="allowNonExistentPaths">If true, ResolvePath() will return a result for the specified language, even if the file is not localized for that language.</param>
		LocalizedManifestLookupResult ResolvePath(string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths);

		/// <summary>
		/// This overload only exists as an optimization: If the caller already has a parsed root directory,
		/// there's no need to parse it again.
		/// </summary>
		LocalizedManifestLookupResult ResolvePath(string rootDir, string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths);

		/// <summary>
		/// Takes a "versioned" path, and looks up the corresponding manifest entry.
		/// </summary>
		LocalizedManifestLookupResult GetFromTranslatedPath(string translatedPath);

		/// <summary>
		/// Lists all manifest entries. Should only be used for diagnostics.
		/// </summary>
		IEnumerable<ManifestEntry> ListAll();

		/// <summary>
		/// Gets all manifest entries from the specified virtual directory.
		/// If Language is null, files from all language directories are returned.
		/// </summary>
		IEnumerable<LocalizedManifestLookupResult> ListDirectory(string virtualPath, CultureInfo cultureInfo, bool recursive);

		/// <summary>
		/// Resolves the directory path for the specified language
		/// </summary>
		LocalizedManifestLookupResult ResolveDirectoryPath(string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths);

		/// <summary>
		/// The datetime the manifest was last updated.
		/// </summary>
		DateTime LastModified { get; }

		/// <summary>
		/// Event which fires when the underlying manifest file is updated
		/// </summary>
		event EventHandler<EventArgs> Changed;
	}
}