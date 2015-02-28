using System;

namespace StaticWwwHelpers
{
	/// <summary>
	/// Represents an entry in the manifest.
	/// </summary>
	public class ManifestEntry
	{
		/// <summary>
		/// The original canonical virtual path.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// The versioned path (i.e. /vp/images/foo-hcAbCdEf1234567890.png).
		/// </summary>
		public string VersionedPath { get; private set; }

		/// <summary>
		/// The version ID for the file.
		/// </summary>
		public Guid VersionId { get; private set; }

		public ManifestEntry(string path, string versionedPath)
		{
			this.Path = path;
			this.VersionedPath = versionedPath;
			this.VersionId = ManifestFile.GetVersionFromPath(versionedPath);
		}
	}
}