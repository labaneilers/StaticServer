using System;
using NUnit.Framework;
using StaticWww.Helpers;

namespace StaticWww.Tests.Helpers
{
	[TestFixture]
	public class ShortGuidTests
	{
		[Test]
		public void Encode_RoundTripsWithDecode()
		{
			var original = new Guid("3a2235f433dd3f09b20af8e3f773ee6c");
			string encoded = ShortGuid.Encode(original);

			//9DUiOt0zCT-yCvjj93PubA
			Guid decoded = ShortGuid.Decode(encoded);

			Assert.AreEqual(original, decoded);
		}
	}
}

