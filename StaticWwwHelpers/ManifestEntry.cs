using System;

namespace StaticWwwHelpers
{
	public class ManifestEntry
	{
		public ManifestEntry(Guid versionId, string path)
		{
			this.VersionId = versionId;
			this.Path = path;
		}

		public Guid VersionId { get; private set }
		public string Path { get; private set; }

	}
}

