using System;
using NUnit.Framework;

namespace StaticWww.Tests.Helpers
{
	[TestFixture]
	public class FileGuidMapTests
	{
		[Test]
		public void ExtractGuid_ReturnsGuid_ForFileWithGuid()
		{
			var expected = new Guid("e9275fb14066bd3d78277fc2dc68220c");
			var actual = FileGuidMap.ExtractGuid("/somepath/somefile-hce9275fb14066bd3d78277fc2dc68220c.css");
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void UpdateFiles_MapsFiles()
		{
			var guidA = new Guid("a9275fb14066bd3d78277fc2dc68220c");
			string fileA = "/someroot/StaticFiles/fileA-hca9275fb14066bd3d78277fc2dc68220c.css";

			var guidB = new Guid("b9275fb14066bd3d78277fc2dc68220c");
			string fileB = "/someroot/StaticFiles/fileB-hcb9275fb14066bd3d78277fc2dc68220c.css";


			var f = new FileGuidMap("/someroot", "/StaticFiles");
			f.EnumerateFiles = p => new string[] {
				fileA, 
				fileB
			};

			f.UpdateFiles();

			Assert.AreEqual(fileA.Replace("/someroot", ""), f.Get(guidA));
			Assert.AreEqual(fileB.Replace("/someroot", ""), f.Get(guidB));
		}

		[Test]
		public void GetPhysicalPath_JoinsPhysicalRoot()
		{
			var f = new FileGuidMap("/foo/bar/root", "/StaticFiles");
			string physicalPath = f.GetPhysicalPath("/something");

			Assert.AreEqual("/foo/bar/root/something", physicalPath);
		}
	}
}

