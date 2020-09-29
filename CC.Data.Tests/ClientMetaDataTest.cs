using CC.Data.MetaData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for ClientMetaDataTest and is intended
    ///to contain all ClientMetaDataTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClientMetaDataTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for FirstName
        ///</summary>
        [TestMethod()]
        public void FirstNameTest()
        {
            ClientMetaData target = new ClientMetaData(); // TODO: Initialize to an appropriate value
            var t = typeof(ClientMetaData);
            var pi = t.GetProperty("FirstName");
            var hasRequired = Attribute.IsDefined(pi, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Assert.IsTrue(hasRequired);
            var attr = (System.ComponentModel.DataAnnotations.RequiredAttribute[])pi.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            Assert.IsTrue(attr.Length > 0);
            Assert.IsTrue(attr[0].AllowEmptyStrings == false);
        }

        /// <summary>
        ///A test for Address
        ///</summary>
        [TestMethod()]
        public void AddressTest()
        {
            ClientMetaData target = new ClientMetaData(); // TODO: Initialize to an appropriate value
            var t = typeof(ClientMetaData);
            var pi = t.GetProperty("Address");
            var hasRequired = Attribute.IsDefined(pi, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Assert.IsTrue(hasRequired);
            var attr = (System.ComponentModel.DataAnnotations.RequiredAttribute[])pi.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            Assert.IsTrue(attr.Length > 0);
            Assert.IsTrue(attr[0].AllowEmptyStrings == false);
        }

        /// <summary>
        ///A test for LastName
        ///</summary>
        [TestMethod()]
        public void LastNameTest()
        {
            ClientMetaData target = new ClientMetaData(); // TODO: Initialize to an appropriate value
            var t = typeof(ClientMetaData);
            var pi = t.GetProperty("LastName");
            var hasRequired = Attribute.IsDefined(pi, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Assert.IsTrue(hasRequired);
            var attr = (System.ComponentModel.DataAnnotations.RequiredAttribute[])pi.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            Assert.IsTrue(attr.Length > 0);
            Assert.IsTrue(attr[0].AllowEmptyStrings == false);
        }

        /// <summary>
        ///A test for City
        ///</summary>
        [TestMethod()]
        public void CityTest()
        {
            ClientMetaData target = new ClientMetaData(); // TODO: Initialize to an appropriate value
            var t = typeof(ClientMetaData);
            var pi = t.GetProperty("City");
            var hasRequired = Attribute.IsDefined(pi, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Assert.IsTrue(hasRequired);
            var attr = (System.ComponentModel.DataAnnotations.RequiredAttribute[])pi.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), false);
            Assert.IsTrue(attr.Length > 0);
            Assert.IsTrue(attr[0].AllowEmptyStrings == false);
        }

        /// <summary>
        ///A test for CountryId
        ///</summary>
        [TestMethod()]
        public void CountryIdTest()
        {
            ClientMetaData target = new ClientMetaData(); // TODO: Initialize to an appropriate value
            var t = typeof(ClientMetaData);
            var pi = t.GetProperty("CountryId");
            var hasRequired = Attribute.IsDefined(pi, typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Assert.IsTrue(hasRequired);
        }
    }
}
