using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace StaticWwwHelpers
{
	/// <summary>
	/// Implements a file localization strategy using a sparse directory tree
	/// which has been processed by Hashly, and has an associated manifest file.
	/// This is governed by the Language domain data class's inheritance rules,
	/// and the directory names are defined in the domain data as well.
	/// </summary>
	public class LocalizedManifestFile : ILocalizedManifestFile
	{
		// Important: Do not use Sadie to inject request context properties (i.e. LanguageId)
		// The FileMapper may be used across sessions

		public string RootVirtualDirectory { get; private set; }

		private readonly IManifestFile _manifestFile;

		/// <summary>
		/// Wraps a manifest file with localization logic.
		/// </summary>
		/// <param name="manifestFile">The manifest file to wrap</param>
		/// <param name="rootVirtualDirectory">The root virtual directory that the manifest file manages</param>
		public LocalizedManifestFile(IManifestFile manifestFile, string rootVirtualDirectory)
		{
			_manifestFile = manifestFile;
			RootVirtualDirectory = rootVirtualDirectory;

			if (_manifestFile.RootVirtualDirectory != null)
			{
				throw new Exception("The specified manifest has a root directory specified (" + _manifestFile.RootVirtualDirectory + "). LocalizedManifestFile only works with unrooted IManifestFiles.");
			}

			_manifestFile.Changed += (sender, e) =>
			{
				if (this.Changed != null)
				{
					this.Changed.Invoke(this, e);
				}
			};
		}

		/// <summary>
		/// Takes a canonical root-relative path and returns a structure which contains the "versioned" virtual path, 
		/// the physical path, and information about the language.
		/// </summary>
		/// <param name="virtualPath">A canonical root-relative path for a static file.</param>
		/// <param name="language">The language for which to get a result.</param>
		/// <param name="allowNonExistentPaths">If true, ResolvePath() will return a result for the specified language, even if the file is not localized for that language.</param>
		public LocalizedManifestLookupResult ResolvePath(string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths)
		{
			return ResolvePath(VirtualPathHelper.GetRootDirectory(virtualPath), virtualPath, cultureInfo, allowNonExistentPaths);
		}

		/// <summary>
		/// This overload only exists as an optimization: If the caller already has a parsed root directory,
		/// there's no need to parse it again.
		/// </summary>
		public LocalizedManifestLookupResult ResolvePath(string rootDir, string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths)
		{
			if (VirtualPathHelper.Matches(rootDir, this.RootVirtualDirectory))
			{
				foreach (PathFromWwwDir pathFromWwwDir in this.EnumeratePathsWithLanguageHierarchy(virtualPath, cultureInfo))
				{
					ManifestEntry entry;
					if (this._manifestFile.TryGetEntry(pathFromWwwDir.Path, out entry))
					{
						// The manifest contained an entry for this file, and this language.
						return new LocalizedManifestLookupResult(true, virtualPath, this.RootVirtualDirectory + entry.VersionedPath, pathFromWwwDir.CultureInfo, entry.VersionId);
					}

					if (allowNonExistentPaths)
					{
						// NOTE: This is here for compatibility with FileMapper operations that write to the file system in developer mode. 
						// It makes very little sense for a manifest-driven directory to allow these operations in production.

						// Even though no entry was found, return the virtual path for the specified language.
						return new LocalizedManifestLookupResult(true, virtualPath, this.RootVirtualDirectory + pathFromWwwDir.Path, pathFromWwwDir.CultureInfo, Guid.Empty);
					}
				}

				return LocalizedManifestLookupResult.Empty(virtualPath);
			}

			return null;
		}

		/// <summary>
		/// Does a lookup for the specified translatedPath (includes the www* directory and versionId)
		/// </summary>
		public LocalizedManifestLookupResult GetFromTranslatedPath(string translatedPath)
		{
			if (!string.IsNullOrEmpty(translatedPath))
			{
				// Get the path from the /www* directory. 
				// The internal manifest file entries start at the localized directory root (they no nothing of localization).
				string translatedPathNoRoot = VirtualPathHelper.StripRoot(translatedPath, this.RootVirtualDirectory);
				if (translatedPathNoRoot != null)
				{
					// This works because ManifestFile will do a lookup by the translated path if the parameter contains a version ID.
					ManifestEntry entry;
					if (_manifestFile.TryGetEntry(translatedPathNoRoot, out entry))
					{
						LocalizedManifestLookupResult result = GetFromManifestEntry(entry);
						if (result != null)
						{
							return result;
						}
					}
				}
			}

			return new LocalizedManifestLookupResult(false, null, translatedPath, null, Guid.Empty);
		}

		/// <summary>
		/// Gets a lookup result from the specified ManifestEntry
		/// </summary>
		private LocalizedManifestLookupResult GetFromManifestEntry(ManifestEntry entry)
		{
			// Now parse the language directory name out of the next directory name.
			int seperatorAfterLangDir = entry.Path.IndexOf("/", 1, StringComparison.Ordinal);
			if (seperatorAfterLangDir > 1)
			{
				// We know the first char is /, or it wouldn't have been found in the manifest.
				string languageDir = entry.Path.Substring(1, seperatorAfterLangDir - 1);
				CultureInfo cultureInfo = Configuration.CultureConfiguration.GetCultureForDirectoryName(languageDir);

				// If no language was found, something very fishy is going on. The path was probably invalid to begin with.
				if (cultureInfo != null)
				{
					// Now, put the directory back together minus the language directory: /root/path 
					string virtualPath = this.RootVirtualDirectory + entry.Path.Substring(seperatorAfterLangDir);

					// translatedPath (the input argument) may or may not be the versioned path.
					// Ensure the result has the versioned path for its "TranslatedVirtualPath" property.
					string versionedPath = this.RootVirtualDirectory + entry.VersionedPath;

					return new LocalizedManifestLookupResult(true, virtualPath, versionedPath, cultureInfo, entry.VersionId);
				}
			}

			return null;
		}

		/// <summary>
		/// List all manifest entries in the underlying manifest
		/// </summary>
		public IEnumerable<ManifestEntry> ListAll()
		{
			return _manifestFile.ListDirectory("/", true);
		}

		/// <summary>
		/// Gets all manifest entries from the specified virtual directory.
		/// If Language is null, files from all language directories are returned.
		/// </summary>
		public IEnumerable<LocalizedManifestLookupResult> ListDirectory(string directoryVirtualPath, CultureInfo cultureInfo, bool recursive)
		{
			IEnumerable<CultureInfo> cultures = cultureInfo == null ? (IEnumerable<CultureInfo>)Configuration.CultureConfiguration.GetSupportedCultures() : new[] { cultureInfo };

			if (!VirtualPathHelper.IsInRootDirectory(directoryVirtualPath, this.RootVirtualDirectory))
			{
				yield break;
			}

			string pathWithoutPrefix = directoryVirtualPath.Substring(this.RootVirtualDirectory.Length);

			foreach (CultureInfo culture in cultures)
			{
				string directoryPath = "/" + Configuration.CultureConfiguration.GetDirectoryNameForCulture(culture) + pathWithoutPrefix;
				foreach (ManifestEntry entry in _manifestFile.ListDirectory(directoryPath, recursive))
				{
					yield return GetFromManifestEntry(entry);
				}
			}
		}

		/// <summary>
		/// Returns a lookup result for a directory path.
		/// </summary>
		/// <param name="virtualPath">A canonical root-relative path for a directory in the manifest.</param>
		/// <param name="language">The language for which to get a result.</param>
		/// <param name="allowNonExistentPaths">If true, ResolvePath() will return a result for the specified language, even if the directory does not exist for that language.</param>
		public LocalizedManifestLookupResult ResolveDirectoryPath(string virtualPath, CultureInfo cultureInfo, bool allowNonExistentPaths)
		{
			if (VirtualPathHelper.IsInRootDirectory(virtualPath, this.RootVirtualDirectory))
			{
				PathFromWwwDir lastPathFromWwwDir = null;
				foreach (PathFromWwwDir pathFromWwwDir in EnumeratePathsWithLanguageHierarchy(virtualPath, cultureInfo))
				{
					lastPathFromWwwDir = pathFromWwwDir;

					bool found = _manifestFile.DirectoryExists(pathFromWwwDir.Path);

					if (found || allowNonExistentPaths)
					{
						return new LocalizedManifestLookupResult(found, virtualPath, this.RootVirtualDirectory + pathFromWwwDir.Path, pathFromWwwDir.CultureInfo, Guid.Empty);
					}
				}

				if (lastPathFromWwwDir == null)
				{
					throw new Exception("No US directory found for path: " + virtualPath);
				}

				return new LocalizedManifestLookupResult(false, virtualPath, this.RootVirtualDirectory + lastPathFromWwwDir.Path, CultureInfo.GetCultureInfo("en-US"), Guid.Empty);
			}

			return null;
		}

		private class PathFromWwwDir
		{
			public string Path;
			public CultureInfo CultureInfo;
		}

		/// <summary>
		/// Given the specified language, enumerates the translated paths for the specified virtual path,  
		/// walking up the language hierarchy.
		/// </summary>
		private IEnumerable<PathFromWwwDir> EnumeratePathsWithLanguageHierarchy(string virtualPath, CultureInfo cultureInfo)
		{
			if (cultureInfo == null)
			{
				throw new ArgumentNullException("language");
			}

			CultureInfo currentCultureInfo = cultureInfo;

			string pathWithoutPrefix = VirtualPathHelper.StripRoot(virtualPath, this.RootVirtualDirectory, false);

			// Walk up through the language heirarchy until a manifest entry is found for the specified path.
			while (currentCultureInfo != null)
			{
				string currentLanguageDir = Configuration.CultureConfiguration.GetDirectoryNameForCulture(currentCultureInfo);

				string virtualPathFromLangDir = "/" + currentLanguageDir + pathWithoutPrefix;

				yield return new PathFromWwwDir { Path = virtualPathFromLangDir, CultureInfo = currentCultureInfo };

				currentCultureInfo = Configuration.CultureConfiguration.GetParent(currentCultureInfo);
			}
		}

		/// <summary>
		/// Gets the date that the manifest file was last modified.
		/// </summary>
		public DateTime LastModified
		{
			get { return _manifestFile.LastModified; }
		}

		/// <summary>
		/// Event which fires when the underlying manifest file is updated
		/// </summary>
		public event EventHandler<EventArgs> Changed;
	}
}