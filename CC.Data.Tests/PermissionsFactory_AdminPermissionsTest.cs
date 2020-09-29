using CC.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CC.Data;
using System.Linq.Expressions;

namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for PermissionsFactory_AdminPermissionsTest and is intended
    ///to contain all PermissionsFactory_AdminPermissionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PermissionsFactory_AdminPermissionsTest
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
        ///A test for AdminPermissions Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void PermissionsFactory_AdminPermissionsConstructorTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Agency, bool> AgencyFilter = target.AgencyFilter.Compile();
            Assert.IsTrue(AgencyFilter(null));
            Func<Client, bool> ClientsFilter = target.ClientsFilter.Compile();
            Assert.IsTrue(ClientsFilter(null));
            Func<AgencyGroup, bool> AgencyGroupsFilter = target.AgencyGroupsFilter.Compile();
            Assert.IsTrue(AgencyGroupsFilter(null));

        }

        /// <summary>
        ///A test for AgencyFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AgencyFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Agency, bool> AgencyFilter = target.AgencyFilter.Compile();
            Assert.IsTrue(AgencyFilter(null));
        }

        /// <summary>
        ///A test for AgencyGroupsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AgencyGroupsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AgencyGroup, bool> AgencyGroupsFilter = target.AgencyGroupsFilter.Compile();
            Assert.IsTrue(AgencyGroupsFilter(null));
        }

        /// <summary>
        ///A test for AppBudgetServicesFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetServicesFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudgetService, bool> AppBudgetServicesFilter = target.AppBudgetServicesFilter.Compile();
            Assert.IsTrue(AppBudgetServicesFilter(null));
        }

        /// <summary>
        ///A test for AppBudgetsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudget, bool> AppBudgetsFilter = target.AppBudgetsFilter.Compile();
            Assert.IsTrue(AppBudgetsFilter(null));
        }

        /// <summary>
        ///A test for AppsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<App, bool> AppsFilter = target.AppsFilter.Compile();
            Assert.IsTrue(AppsFilter(null));
        }

        /// <summary>
        ///A test for CanChangeApprovalStatus
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanChangeApprovalStatusTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanChangeApprovalStatus);
        }

        /// <summary>
        ///A test for CanChangeDeceasedDate
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanChangeDeceasedDateTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanChangeDeceasedDate);
        }

        /// <summary>
        ///A test for CanChangeExceptionalHours
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanChangeExceptionalHoursTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanChangeExceptionalHours);
        }

        /// <summary>
        ///A test for CanChangeGfHours
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanChangeGfHoursTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanChangeGfHours);
        }

        /// <summary>
        ///A test for CanDeleteFuncScore
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanDeleteFuncScoreTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanDeleteFuncScore);
        }

        /// <summary>
        ///A test for CanDeleteHcePeriod
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanDeleteHcePeriodTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanDeleteHcePeriod);
        }

        /// <summary>
        ///A test for CanSeeProgramField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanSeeProgramFieldTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanSeeProgramField);
        }

        /// <summary>
        ///A test for CanViewCeefFields
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanViewCeefFieldsTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanEditCeefFields);
        }

        /// <summary>
        ///A test for ClientReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void ClientReportsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<ClientReport, bool> ClientReportsFilter = target.ClientReportsFilter.Compile();
            Assert.IsTrue(ClientReportsFilter(null));
        }

        /// <summary>
        ///A test for ClientsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void ClientsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Client, bool> ClientsFilter = target.ClientsFilter.Compile();
            Assert.IsTrue(ClientsFilter(null));
        }

        /// <summary>
        ///A test for MainReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void MainReportsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<MainReport, bool> MainReportsFilter = target.MainReportsFilter.Compile();
            Assert.IsTrue(MainReportsFilter(null));
        }

        /// <summary>
        ///A test for SubReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void SubReportsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<SubReport, bool> SubReportsFilter = target.SubReportsFilter.Compile();
            Assert.IsTrue(SubReportsFilter(null));
        }
        /// <summary>
        ///access denied asserts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AccessDeniedTest()
        {
            User user = new User() { AgencyId = 1, AgencyGroupId = 1, RoleId = (int)FixedRoles.Admin };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanEditCeefFields);
            Assert.IsTrue(target.CanChangeApprovalStatus);
            Assert.IsTrue(target.CanChangeDeceasedDate);
            Assert.IsTrue(target.CanChangeGfHours);
            Assert.IsTrue(target.CanChangeExceptionalHours);
            Assert.IsTrue(target.CanDeleteHcePeriod);
            Assert.IsTrue(target.CanDeleteFuncScore);
            Assert.IsTrue(target.CanSeeProgramField);
            Assert.IsTrue(target.CanCreateNewClient);
            Assert.IsTrue(target.CanUpdateExistingClient);
            // Assert.IsTrue(target.CanCreateMainReport); everyone can create mainreports ?
        }

    }
}
