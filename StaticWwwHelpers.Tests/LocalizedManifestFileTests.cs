using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StaticWwwHelpers;
using System.Globalization;

namespace StaticWwwHelpers.Tests
{
	[Category("Integration")]
	[TestFixture]
	public class LocalizedManifestFileTests
	{
		private class MockCultureConfiguration : ICultureConfiguration
		{
			private class CultureData
			{
				public CultureInfo CultureInfo { get; private set; }
				public string DirectoryName { get; private set; } 
				public CultureData Parent { get; private set; }

				public CultureData(string cultureCode, string directoryName, CultureData parent)
				{
					this.CultureInfo = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
					if (this.CultureInfo == null)
					{
						throw new Exception("Culture not found for " + cultureCode);
					}
					this.DirectoryName = directoryName;
					this.Parent = parent;
				}
			}

			public void Add(string cultureCode, string directoryName, string parentCode)
			{
				CultureData parent = null;
				if (parentCode != null)
				{
					if (!_cultures.TryGetValue(parentCode, out parent))
					{
						throw new Exception("Parent culture not found: " + parentCode);
					}
				}
				var cultureData = new CultureData(cultureCode, directoryName, parent);
				_cultures.Add(cultureCode, cultureData);
				_culturesByDirectoryName.Add(directoryName, cultureData);
			}

			private IDictionary<string, CultureData> _cultures = new Dictionary<string, CultureData>(StringComparer.OrdinalIgnoreCase);
			private IDictionary<string, CultureData> _culturesByDirectoryName = new Dictionary<string, CultureData>(StringComparer.OrdinalIgnoreCase);

			private static IDictionary<string, CultureData> GetCulturesByDirectoryName(IDictionary<string, CultureData> cultures)
			{
				var culturesByDir = new Dictionary<string, CultureData>(StringComparer.OrdinalIgnoreCase);
				foreach (CultureData cultureData in cultures.Values)
				{
					culturesByDir.Add(cultureData.DirectoryName, cultureData);
				}
				return culturesByDir;
			}

			public CultureInfo GetCultureForDirectoryName(string directoryName)
			{
				CultureData data;
				if (_culturesByDirectoryName.TryGetValue(directoryName, out data))
				{
					return data.CultureInfo;
				}
				return null;
			}

			public string GetDirectoryNameForCulture(CultureInfo cultureInfo)
			{
				CultureData data;
				if (_cultures.TryGetValue(cultureInfo.Name, out data))
				{
					return data.DirectoryName;
				}
				return null;
			}

			public CultureInfo GetParent(CultureInfo cultureInfo)
			{
				CultureData data;
				if (_cultures.TryGetValue(cultureInfo.Name, out data))
				{
					if (data.Parent == null)
					{
						return null;
					}
					return data.Parent.CultureInfo;
				}
				return null;
			}

			public IEnumerable<CultureInfo> GetSupportedCultures()
			{
				return _cultures.Select(x => x.Value.CultureInfo);
			}
		}

		private LocalizedManifestFile GetMapper(IEnumerable<ManifestEntry> entries = null)
		{
			if (entries == null)
			{
				entries = new []
				{
					new ManifestEntry("/www/abc/foo.png", "/www/abc/foo-hca6836c539dd87129026ae2a85e4e43f4.png"),
					new ManifestEntry("/www.fr/abc/foo.png", "/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
					new ManifestEntry("/www.ca.fr/abc/foo.png", "/www.ca.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f6.png")
				};
			}

			var mapper = new LocalizedManifestFile(new ManifestFile(entries), "/merch");

			return mapper;
		}

		[SetUp]
		public void SetUp()
		{
			var config = new MockCultureConfiguration();
			config.Add("en-US", "www", null);
			config.Add("fr-FR", "www.fr", "en-US");
			config.Add("fr-BE", "www.be", "fr-FR");
			config.Add("fr-CA", "www.ca.fr", "fr-FR");
			config.Add("de-DE", "www.de", "en-US");

			Configuration.CultureConfiguration = config;
		}

		[Test]
		public void ResolvePath_ReturnsLocalizedImage_IfExists()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/merch/abc/foo.png", CultureInfo.GetCultureInfo("fr-FR"), false);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		public void ResolvePath_ReturnsParentLocalizedImage_IfNotExists_ButParentExists()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/merch/abc/foo.png", CultureInfo.GetCultureInfo("fr-CA"), false);

			// TODO: I think this was wrong in the original test. Its fixed, but it should be retested in VP.Common.Web
			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www.ca.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f6.png", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual(Guid.Parse("a6836c539dd87129026ae2a85e4e43f6"), result.VersionId);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-CA"), result.CultureInfo);
		}

		[Test]
		public void ResolvePath_ReturnsUsLocalizedImage_IfNotExists()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/merch/abc/foo.png", CultureInfo.GetCultureInfo("de-DE"), false);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www/abc/foo-hca6836c539dd87129026ae2a85e4e43f4.png", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual(Guid.Parse("a6836c539dd87129026ae2a85e4e43f4"), result.VersionId);
			Assert.AreEqual(CultureInfo.GetCultureInfo("en-US"), result.CultureInfo);
		}

		[Test]
		public void ResolvePath_ReturnsResultNotFound_IfNotExists_IfUsVersionDoesntExist()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/merch/abc/idontexist.png", CultureInfo.GetCultureInfo("de-DE"), false);

			Assert.IsFalse(result.Found);
			Assert.AreEqual(null, result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/idontexist.png", result.VirtualPath);
			Assert.AreEqual(null, result.CultureInfo);
		}

		[Test]
		public void ResolvePath_ReturnsNull_IfNotCorrectDirectoryPrefix()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/notmerch/abc/foo_hashed.png", CultureInfo.GetCultureInfo("de-DE"), false);

			Assert.IsNull(result);
		}

		[Test]
		public void ResolvePath_ReturnsResult_IfAllowNonExistentPaths_IsTrue()
		{
			LocalizedManifestLookupResult result = GetMapper().ResolvePath("/merch/abc/foo.png", CultureInfo.GetCultureInfo("de-DE"), true);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www.de/abc/foo.png", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("de-DE"), result.CultureInfo);
		}

		[Test]
		public void GetFromTranslatedPath_ReturnsCorrectVirtualPath()
		{
			LocalizedManifestLookupResult result = GetMapper().GetFromTranslatedPath("/merch/www.fr/abc/foo.png");

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", result.TranslatedVirtualPath);
			Assert.AreEqual(Guid.Parse("a6836c539dd87129026ae2a85e4e43f5"), result.VersionId);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		public void GetFromTranslatedPath_ReturnsCorrectVirtualPath_UsingVersionedPath()
		{
			LocalizedManifestLookupResult result = GetMapper().GetFromTranslatedPath("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png");

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/abc/foo.png", result.VirtualPath);
			Assert.AreEqual("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", result.TranslatedVirtualPath);
			Assert.AreEqual(Guid.Parse("a6836c539dd87129026ae2a85e4e43f5"), result.VersionId);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		public void GetFromTranslatedPath_ReturnsFalse_IfNotInRootPath()
		{
			LocalizedManifestLookupResult result = GetMapper().GetFromTranslatedPath("/notmerch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png");

			Assert.IsFalse(result.Found);
			Assert.IsNull(result.VirtualPath);
		}

		private static readonly ManifestEntry[] FIND_ALL_FILES_DATA = 
		{
			new ManifestEntry("/www/abc/foo.png", "/www/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www.fr/abc/foo.png", "/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www.ca.fr/abc/foo.png", "/www.ca.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www/x/foo.png", "/www/x/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www.fr/x/foo.png", "/www.fr/x/foo-hca6836c539dd87129026ae2a85e4e43f5.png"),
			new ManifestEntry("/www.ca.fr/x/foo.png", "/www.ca.fr/x/foo-hca6836c539dd87129026ae2a85e4e43f5.png")
		};

		[Test]
		public void ListDirectory_FindsAllFiles_IfLanguageIsNull()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			IList<LocalizedManifestLookupResult> entries = mapper.ListDirectory("/merch/abc", null, true).OrderBy(x => x.TranslatedVirtualPath).ToList();

			Assert.AreEqual(3, entries.Count);

			Assert.AreEqual("/merch/www.ca.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", entries[0].TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", entries[0].VirtualPath);

			Assert.AreEqual("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", entries[1].TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", entries[1].VirtualPath);

			Assert.AreEqual("/merch/www/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", entries[2].TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", entries[2].VirtualPath);
		}

		[Test]
		public void ListDirectory_FindsLanguageFiles_IfLanguageIsNotNull()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			IList<LocalizedManifestLookupResult> entries = mapper.ListDirectory("/merch/abc", CultureInfo.GetCultureInfo("fr-FR"), true).ToList();

			Assert.AreEqual(1, entries.Count);

			Assert.AreEqual("/merch/www.fr/abc/foo-hca6836c539dd87129026ae2a85e4e43f5.png", entries[0].TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc/foo.png", entries[0].VirtualPath);
		}

		[Test]
		public void ListDirectory_ReturnsEmptyList_IfNonExistentPathPassed()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			IList<LocalizedManifestLookupResult> entries = mapper.ListDirectory("/merch/xyz", null, true).ToList();

			Assert.AreEqual(0, entries.Count);
		}

		[Test]
		public void ListDirectory_ReturnsEmptyList_IfIncorrectPrefix()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			IList<LocalizedManifestLookupResult> entries = mapper.ListDirectory("/nerch/abc", null, true).ToList();

			Assert.AreEqual(0, entries.Count);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnValidResult_ForUsDirectory()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/abc", CultureInfo.GetCultureInfo("en-US"), false);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www/abc", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("en-US"), result.CultureInfo);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnValidResult_ForNonUsDirectory()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/abc", CultureInfo.GetCultureInfo("fr-FR"), false);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www.fr/abc", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnValidResult_ForInheritedLanguageDirectory()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/abc", CultureInfo.GetCultureInfo("fr-BE"), false);

			Assert.IsTrue(result.Found);
			Assert.AreEqual("/merch/www.fr/abc", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/abc", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnNotFound_ForNonExistentUsDirectory()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/xyz", CultureInfo.GetCultureInfo("en-US"), false);

			Assert.IsFalse(result.Found);
			Assert.AreEqual("/merch/www/xyz", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/xyz", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("en-US"), result.CultureInfo);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnNotFound_ForNonExistentNonUsDirectory()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/xyz", CultureInfo.GetCultureInfo("fr-FR"), false);

			Assert.IsFalse(result.Found);
			Assert.AreEqual("/merch/www/xyz", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/xyz", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("en-US"), result.CultureInfo);
		}

		[Test]
		public void ResolveDirectoryPath_ShouldReturnNotFound_ForNonExistentNonUsDirectory_WithAllowNonExistentTrue()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			LocalizedManifestLookupResult result = mapper.ResolveDirectoryPath("/merch/xyz", CultureInfo.GetCultureInfo("fr-FR"), true);

			Assert.IsFalse(result.Found);
			Assert.AreEqual("/merch/www.fr/xyz", result.TranslatedVirtualPath);
			Assert.AreEqual("/merch/xyz", result.VirtualPath);
			Assert.AreEqual(CultureInfo.GetCultureInfo("fr-FR"), result.CultureInfo);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ResolveDirectoryPath_ShouldThrowNullArgumentException_WhenLanguageIsNull()
		{
			var mapper = GetMapper(FIND_ALL_FILES_DATA);

			mapper.ResolveDirectoryPath("/merch/abc", null, false);
		}
	}
}