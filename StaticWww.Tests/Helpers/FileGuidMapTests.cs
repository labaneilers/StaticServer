﻿using System;
using NUnit.Framework;

namespace StaticWww.Tests
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
			string fileA = "/someroot/fileA-hca9275fb14066bd3d78277fc2dc68220c.css";

			var guidB = new Guid("b9275fb14066bd3d78277fc2dc68220c");
			string fileB = "/someroot/fileB-hcb9275fb14066bd3d78277fc2dc68220c.css";


			var f = new FileGuidMap("/someroot");
			f.EnumerateFiles = p => new string[] {
				fileA, 
				fileB
			};

			f.UpdateFiles();

			Assert.AreEqual(fileA.Replace("/someroot", ""), f.Get(guidA));
			Assert.AreEqual(fileB.Replace("/someroot", ""), f.Get(guidB));


		}
	}
}
