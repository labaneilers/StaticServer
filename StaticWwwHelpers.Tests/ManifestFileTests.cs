using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StaticWwwHelpers;

namespace StaticWwwHelpers.Tests
{
	[TestFixture]
	public class ManifestFileTests
	{
		private ManifestFile GetManifest(IEnumerable<ManifestEntry> entries = null, string virtualPath = null)
		{
			if (entries == null)
			{
				entries = new[]
				{
					new ManifestEntry("/www/abc/foo.png", "/www/abc/foo-hca6836c539dd87129026ae2a85e4e43f4.png"),
					new ManifestEntry("/www.fr/abc/foo.png", "/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
					new ManifestEntry("/www.ca.fr/abc/foo.png", "/www.ca.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f6.png")
				};
			}

			return new ManifestFile(entries, virtualPath);
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfDirectoryExists()
		{
			Assert.IsTrue(GetManifest().DirectoryExists("/www/abc"));
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfDirectoryExists_WithVirtualRoot()
		{
			Assert.IsTrue(GetManifest(null, "/virtualRoot").DirectoryExists("/virtualRoot/www/abc"));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfDirectoryDoesntExist()
		{
			Assert.IsFalse(GetManifest().DirectoryExists("/www/xyz"));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfDirectoryDoesntExist_WithVirtualRoot()
		{
			Assert.IsFalse(GetManifest(null, "/virtualRoot").DirectoryExists("/virtualRoot/www/xyz"));
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfDirectoryExists_WithEndingSlash()
		{
			Assert.IsTrue(GetManifest().DirectoryExists("/www/abc/"));
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfDirectoryExists_WithEndingSlash_WithVirtualRoot()
		{
			Assert.IsTrue(GetManifest(null, "/virtualRoot").DirectoryExists("/virtualRoot/www/abc/"));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfDirectoryDoesntExist_WithEndingSlash()
		{
			Assert.IsFalse(GetManifest().DirectoryExists("/www/xyz/"));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfDirectoryDoesntExist_WithEndingSlash_WithVirtualRoot()
		{
			Assert.IsFalse(GetManifest(null, "/virtualRoot").DirectoryExists("/virtualRoot/www/xyz/"));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfNullPassed()
		{
			Assert.IsFalse(GetManifest().DirectoryExists(null));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfNullPassed_WithVirtualRoot()
		{
			Assert.IsFalse(GetManifest(null, "/virtualRoot").DirectoryExists(null));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfEmptyStringPassed()
		{
			Assert.IsFalse(GetManifest().DirectoryExists(""));
		}

		[Test]
		public void DirectoryExists_ReturnsFalse_IfEmptyStringPassed_WithVirtualRoot()
		{
			Assert.IsFalse(GetManifest(null, "/virtualRoot").DirectoryExists(""));
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfEmptyStringPassed()
		{
			// An empty path is the root
			Assert.IsFalse(GetManifest().DirectoryExists(""));
		}

		[Test]
		public void DirectoryExists_ReturnsTrue_IfEmptyStringPassed_WithVirtualRoot()
		{
			// An empty path is the root
			Assert.IsFalse(GetManifest(null, "/virtualRoot").DirectoryExists(""));
		}

		private static readonly ManifestEntry[] LIST_DIR_FILES = 
		{
			new ManifestEntry("/www/abc/aaa.png", "/www/abc/aaa-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/abc/bbb.png", "/www/abc/bbb-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/abc/ccc.png", "/www/abc/ccc-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/abc/xyz/ddd.png", "/www/abc/xyz/ddd-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/abc/xyz/eee.png", "/www/abc/xyz/eee-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/abc/xyz/fff.png", "/www/abc/xyz/fff-hca6836c539dd87129026ae2a85e4e43f5.png"),
		};

		[Test]
		public void ListDirectory_ListsAllFiles_NonRecursive()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES).ListDirectory("/www/abc", false).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(3, entries.Count);
			Assert.AreEqual("/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/www/abc/ccc.png", entries[2].Path);
		}

		[Test]
		public void ListDirectory_ListsAllFiles_NonRecursive_WithVirtualRoot()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES, "/virtualRoot").ListDirectory("/virtualRoot/www/abc", false).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(3, entries.Count);
			Assert.AreEqual("/virtualRoot/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/virtualRoot/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/virtualRoot/www/abc/ccc.png", entries[2].Path);
		}

		[Test]
		public void ListDirectory_ListsAllFiles_Recursive()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES).ListDirectory("/www/abc", true).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(6, entries.Count);
			Assert.AreEqual("/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/www/abc/ccc.png", entries[2].Path);
			Assert.AreEqual("/www/abc/xyz/ddd.png", entries[3].Path);
			Assert.AreEqual("/www/abc/xyz/eee.png", entries[4].Path);
			Assert.AreEqual("/www/abc/xyz/fff.png", entries[5].Path);
		}

		[Test]
		public void ListDirectory_ListsAllFiles_Recursive_WithVirtualRoot()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES, "/virtualRoot").ListDirectory("/virtualRoot/www/abc", true).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(6, entries.Count);
			Assert.AreEqual("/virtualRoot/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/virtualRoot/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/virtualRoot/www/abc/ccc.png", entries[2].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/ddd.png", entries[3].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/eee.png", entries[4].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/fff.png", entries[5].Path);
		}

		[Test]
		public void ListDirectory_ListsAllFiles_Recursive_FromRoot()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES).ListDirectory("/", true).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(6, entries.Count);
			Assert.AreEqual("/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/www/abc/ccc.png", entries[2].Path);
			Assert.AreEqual("/www/abc/xyz/ddd.png", entries[3].Path);
			Assert.AreEqual("/www/abc/xyz/eee.png", entries[4].Path);
			Assert.AreEqual("/www/abc/xyz/fff.png", entries[5].Path);
		}

		[Test]
		public void ListDirectory_ListsAllFiles_Recursive_FromRoot_WithVirtualRoot()
		{
			IList<ManifestEntry> entries = GetManifest(LIST_DIR_FILES, "/virtualRoot").ListDirectory("/", true).OrderBy(x => x.Path).ToList();

			Assert.AreEqual(6, entries.Count);
			Assert.AreEqual("/virtualRoot/www/abc/aaa.png", entries[0].Path);
			Assert.AreEqual("/virtualRoot/www/abc/bbb.png", entries[1].Path);
			Assert.AreEqual("/virtualRoot/www/abc/ccc.png", entries[2].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/ddd.png", entries[3].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/eee.png", entries[4].Path);
			Assert.AreEqual("/virtualRoot/www/abc/xyz/fff.png", entries[5].Path);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ListDirectory_ThrowsArgumentNullException_IfPathIsNull()
		{
			GetManifest(LIST_DIR_FILES).ListDirectory(null, true);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ListDirectory_ThrowsArgumentNullException_IfPathIsNull_WithVirtualRoot()
		{
			GetManifest(LIST_DIR_FILES, "/virtualRoot").ListDirectory(null, true);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TryGetEntry_ShouldThrowNullArgumentException_IfPathIsNull()
		{
			ManifestEntry entry;
			GetManifest(LIST_DIR_FILES).TryGetEntry(null, out entry);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void TryGetEntry_ShouldThrowNullArgumentException_IfPathIsEmpty()
		{
			ManifestEntry entry;
			GetManifest(LIST_DIR_FILES).TryGetEntry("", out entry);
		}

		[Test]
		public void TryGetEntry_ShouldFindAnExistingPath()
		{
			ManifestEntry entry;
			bool found = GetManifest(LIST_DIR_FILES).TryGetEntry("/www/abc/aaa.png", out entry);

			Assert.IsTrue(found);
			Assert.AreEqual("/www/abc/aaa.png", entry.Path);
			Assert.AreEqual("/www/abc/aaa-hca6836c539dd87129026ae2a85e4e43f5.png", entry.VersionedPath);
		}

		[Test]
		public void TryGetEntry_ShouldFindAnExistingPath_ByVersionedPath()
		{
			ManifestEntry entry;
			bool found = GetManifest(LIST_DIR_FILES).TryGetEntry("/www/abc/aaa-hca6836c539dd87129026ae2a85e4e43f5.png", out entry);

			Assert.IsTrue(found);
			Assert.AreEqual("/www/abc/aaa.png", entry.Path);
			Assert.AreEqual("/www/abc/aaa-hca6836c539dd87129026ae2a85e4e43f5.png", entry.VersionedPath);
		}
	}
}