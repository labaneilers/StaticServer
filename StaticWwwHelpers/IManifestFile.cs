using System;
using System.Collections.Generic;

namespace StaticWwwHelpers
{
	/// <summary>
	/// An API to lookup individual manifest entries by path.
	/// </summary>
	public interface IManifestFile
	{
		/// <summary>
		/// If specified, this root directory is prepended to all entries in the manifest.
		/// </summary>
		string RootVirtualDirectory { get; }

		/// <summary>
		/// Gets the manifest entry for the specified virtual path.
		/// Returns false if no entry exists.
		/// </summary>
		bool TryGetEntry(string virtualPath, out ManifestEntry manifestEntry);

		/// <summary>
		/// List all the files in the specified directory.
		/// </summary>
		IEnumerable<ManifestEntry> ListDirectory(string virtualPath, bool recursive);

		/// <summary>
		/// Returns true if the directory exists
		/// </summary>
		bool DirectoryExists(string virtualPath);

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