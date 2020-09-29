using CC.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for ClientTest and is intended
    ///to contain all ClientTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClientTest
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
        ///should check the following:
        ///1.when adding deceased date leave date is updated(only if needed)
        ///2.validate fails if there exists reported service later than leave date
        ///3.validate fails if marked with HCentitled   and BirthDate doesnt exist
        ///</summary>
        [TestMethod()]
        public void DatesTest()// test fails no automatic updating of leave date ?
        {
            Client target = new Client(); // TODO: Initialize to an appropriate value
            target.DeceasedDate = DateTime.Now.AddDays(20);
            Assert.IsTrue(target.LeaveDate.Equals(target.DeceasedDate)); // assert fails!!
            DateTime k = DateTime.Now.AddDays(-5);
            target.LeaveDate = k;
            target.DeceasedDate = DateTime.Now.AddDays(20); // leave must remain unchanged here
            Assert.IsTrue(target.LeaveDate.Equals(k));
        }
        /// <summary>
        ///should check the following:
        ///2.validate fails if there exists reported service later than leave date
        ///</summary>
        [TestMethod()]
        public void LeaveDateTest() // the check isnt implemented yet i guess
        {
            Client target = new Client();
            target.LeaveDate = DateTime.Now;
            target.CountryId = 3;
            var ValidContext = new ValidationContext(target, null, null);
            var res = target.Validate(ValidContext);
            Assert.IsTrue(res.Any());  // test fails
            foreach (var t in res)
            {
                //Assert.IsTrue(t.ErrorMessage.Equals("Leave date illegal message"));
            }
            target.LeaveDate = DateTime.Now.AddDays(20);
            res = target.Validate(ValidContext);
            Assert.IsFalse(res.Any());  
        }
        /// <summary>
        ///should check the following:
        ///2.validate fails if there exists reported service later than leave date that was autoset due to deceased date
        ///</summary>
        [TestMethod()]
        public void AutoLeaveDateTest() // the check isnt implemented yet i guess
        {
            Client target = new Client(); // TODO: Initialize to an appropriate value
            target.DeceasedDate = DateTime.Now; // should autoset leave date to illegal value
            target.CountryId = 3;
            var ValidContext = new ValidationContext(target, null, null);
            var res = target.Validate(ValidContext);
            Assert.IsTrue(res.Any()); // test fails
            target.DeceasedDate = DateTime.Now.AddDays(20);
            res = target.Validate(ValidContext);
            Assert.IsFalse(res.Any());
        }
        /// <summary>
        ///should check the following:
        ///3.validate fails if marked with HCentitled   and BirthDate doesnt exist
        ///</summary>
        [TestMethod()]
        public void BirthDateTest()
        {
            Client target = new Client();
            target.HomeCareEntitled = true;
            var ValidContext = new ValidationContext(target,null,null);
            target.CountryId = 3;
            var res = target.Validate(ValidContext);
            int i = 0; // return 1 fail BirthDate required
            foreach (var t in res)
            {
                Assert.IsTrue(t.ErrorMessage.Equals("Birth Date is required if the client is marked as Home Care Entitled"));
                i++;
            }
            Assert.IsTrue(i == 1);
            target.HomeCareEntitled = false;
            target.BirthDate = null;
            res = target.Validate(ValidContext); // everything is fine no results returned
            Assert.IsFalse(res.Any());
        }
        /// <summary>
        ///should check the following:
        ///3.validate fails if the country is USA or Canada (with states)
        ///but the state field is empty
        ///</summary>
        [TestMethod()]
        public void StatedCountryTest()
        {
            Client target = new Client();
            var ValidContext = new ValidationContext(target, null, null);
            target.CountryId = 1; // canada
            var res = target.Validate(ValidContext);
            foreach (var t in res)
            {
                Assert.IsTrue(t.ErrorMessage.Equals("State is required")); // no state
            }
            
            target.StateId = 0;
            res = target.Validate(ValidContext);
            Assert.IsFalse(res.Any());
            
            target.CountryId = 4;
            target.StateId = null;
            res = target.Validate(ValidContext);
            Assert.IsFalse(res.Any());

            target.CountryId = 0;
            target.StateId = null;
            res = target.Validate(ValidContext);
            foreach (var t in res)
            {
                Assert.IsTrue(t.ErrorMessage.Equals("State is required")); // no state
            }
        }

    }
}
