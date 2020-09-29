using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace p4b.Web.Export.Tests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			Assert.AreEqual(GeneratedCode.XlsxExtensions_Accessor.Column(0), "A");
			Assert.AreEqual(GeneratedCode.XlsxExtensions_Accessor.Column(27), "AA");
		}
	}
}
