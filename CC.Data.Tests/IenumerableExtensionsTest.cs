using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CC.Data.Tests
{


    /// <summary>
    ///This is a test class for IenumerableExtensionsTest and is intended
    ///to contain all IenumerableExtensionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class IenumerableExtensionsTest
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




        [TestMethod()]
        public void ToChunnksTest()
        {

            int dataCount = 99;
            int chunkSize = 10;
            var data = Enumerable.Range(0, dataCount);
            var chunks = data.Split(chunkSize);
            var chunkCount = 0;
            foreach (var chunk in chunks)
            {
                Assert.IsTrue(chunk.Count() <= chunkSize);
                chunkCount++;
            }

            Assert.IsTrue(chunkCount == Math.Ceiling((double)dataCount / chunkSize));
        }
    }
}
