using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Web;
using CC.Data;
using Rhino.Mocks;

namespace CC.Web.Tests
{
    /// <summary>
    /// Summary description for AuthorizeAttributeTest
    /// </summary>
    [TestClass]
    public class AuthorizeAttributeTest
    {
        public AuthorizeAttributeTest()
        {
            //

            // TODO: Add constructor logic here
            //
        }

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
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        //[TestMethod()]
        //public void AuthorizeCoreTest()
        //{
        //    CcAuthorizeAttribute_Accessor target = new CcAuthorizeAttribute_Accessor(FixedRoles.Admin); // TODO: Initialize to an appropriate value
        //    target.CurrentUser = new User();
        //    var mocks = new ContextMocks(target, true, false);
        //    HttpContextBase httpContext = null; // TODO: Initialize to an appropriate value

        //    //should deny an empty user
        //    Assert.IsFalse(target.AuthorizeCore(httpContext));

        //    //should deny user from different role
        //    target.CurrentUser.RoleId = (int)FixedRoles.AgencyOfficer;
        //    Assert.IsFalse(target.AuthorizeCore(httpContext));

        //    //should allow the same role
        //    target.CurrentUser.RoleId = (int)FixedRoles.Admin;
        //    Assert.IsTrue(target.AuthorizeCore(httpContext));

        //    //should deny undefined role
        //    target.CurrentUser.RoleId = (int)(FixedRoles.AgencyOfficer | FixedRoles.Admin);
        //    Assert.IsFalse(target.AuthorizeCore(httpContext));
        //}
    }
}
