using CC.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CC.Data;
using System.Linq.Expressions;

namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for PermissionsFactory_RegionOfficerPremissionsTest and is intended
    ///to contain all PermissionsFactory_RegionOfficerPremissionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PermissionsFactory_RegionOfficerPremissionsTest
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
        ///A test for RegionOfficerPremissions Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void PermissionsFactory_RegionOfficerPremissionsConstructorTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(true);
        }

        /// <summary>
        ///A test for AgencyFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AgencyFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Agency, bool> AgencyFilter = target.AgencyFilter.Compile();
            Agency param = new Agency() { AgencyGroup = new AgencyGroup() { Country = new Country() { Region = new Region() { Users = new List<User>() { new User() { Id = 0 } } } } } };
            Assert.IsFalse(AgencyFilter(param));
            param.AgencyGroup.Country.Region.Users.Add(user);
            Assert.IsTrue(AgencyFilter(param));
        }

        /// <summary>
        ///A test for AgencyGroupsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AgencyGroupsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AgencyGroup, bool> AgencyGroupsFilter = target.AgencyGroupsFilter.Compile();
            AgencyGroup param = new AgencyGroup() { Country = new Country() { Region = new Region() { Users = new List<User>() { new User() { Id = 0 } } } } };
            Assert.IsFalse(AgencyGroupsFilter(param));
            param.Country.Region.Users.Add( user );
            Assert.IsTrue(AgencyGroupsFilter(param));
        }

        /// <summary>
        ///A test for AppBudgetServicesFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetServicesFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RegionId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudgetService, bool> AppBudgetServicesFilter = target.AppBudgetServicesFilter.Compile();
            AppBudgetService param = new AppBudgetService() { Agency = new Agency() { AgencyGroup = new AgencyGroup() { Country = new Country() { RegionId = 0 } } } };
            Assert.IsFalse(AppBudgetServicesFilter(param));
            param.Agency.AgencyGroup.Country.RegionId = 1;
            Assert.IsTrue(AppBudgetServicesFilter(param));
        }

        /// <summary>
        ///A test for AppBudgetsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RegionId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudget, bool> AppBudgetsFilter = target.AppBudgetsFilter.Compile();
            AppBudget param = new AppBudget() { App = new App() { AgencyGroup = new AgencyGroup() { Country = new Country() { RegionId = 0 } } } };
            Assert.IsFalse(AppBudgetsFilter(param));
            param.App.AgencyGroup.Country.RegionId = 1;
            Assert.IsTrue(AppBudgetsFilter(param));
        }

        /// <summary>
        ///A test for AppsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RegionId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<App, bool> AppsFilter = target.AppsFilter.Compile();
            App param = new App() { AgencyGroup = new AgencyGroup() { Country = new Country() { RegionId = 0 } } };
            Assert.IsFalse(AppsFilter(param));
            param.AgencyGroup.Country.RegionId = 1;
            Assert.IsTrue(AppsFilter(param));
        }

        /// <summary>
        ///A test for CanSeeProgramField
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanSeeProgramFieldTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
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
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanEditCeefFields);
        }

        /// <summary>
        ///A test for ClientsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void ClientsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Client, bool> ClientsFilter = target.ClientsFilter.Compile();
            Client param = new Client() { Agency = new Agency() { AgencyGroup = new AgencyGroup() { Country = new Country() { Region = new Region() { Users = new List<User>() { new User() { Id = 0 } } } } } } };
            Assert.IsFalse(ClientsFilter(param));

            param.Agency.AgencyGroup.Country.Region.Users.Add(user);
            Assert.IsTrue(ClientsFilter(param));
        }

        /// <summary>
        ///A test for MainReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void MainReportsFilterTest()
        {
            User user = new User() { AgencyId = 1, RegionId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<MainReport, bool> MainReportsFilter = target.MainReportsFilter.Compile();
            MainReport param = new MainReport() { AppBudget = new AppBudget() { App = new App() { AgencyGroup = new AgencyGroup() { Country = new Country() { RegionId = 0 } } } } };
            Assert.IsFalse(MainReportsFilter(param));
            param.AppBudget.App.AgencyGroup.Country.RegionId = 1; 
            Assert.IsTrue(MainReportsFilter(param));
        }

        /// <summary>
        ///A test for SubReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void SubReportsFilterTest()
        {
            User user = new User() { AgencyId = 1, RegionId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<SubReport, bool> SubReportsFilter = target.SubReportsFilter.Compile();
            SubReport param = new SubReport() { AppBudgetService = new AppBudgetService() { Agency = new Agency() { AgencyGroup = new AgencyGroup() { Country = new Country() { RegionId = 0 } } } } };
            Assert.IsFalse(SubReportsFilter(param));
            param.AppBudgetService.Agency.AgencyGroup.Country.RegionId = 1;
            Assert.IsTrue(SubReportsFilter(param));
        }
        /// <summary>
        ///access denied asserts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AccessDeniedTest()
        {
            User user = new User() { AgencyId = 1, AgencyGroupId = 1, RoleId = (int)FixedRoles.RegionOfficer };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanEditCeefFields);
            Assert.IsFalse(target.CanChangeApprovalStatus);
            Assert.IsFalse(target.CanChangeDeceasedDate);
            Assert.IsFalse(target.CanChangeGfHours);
            Assert.IsFalse(target.CanChangeExceptionalHours);
            Assert.IsFalse(target.CanDeleteHcePeriod);
            Assert.IsFalse(target.CanDeleteFuncScore);
            Assert.IsTrue(target.CanSeeProgramField);
            Assert.IsFalse(target.CanCreateNewClient);
            Assert.IsFalse(target.CanUpdateExistingClient);
            // Assert.IsTrue(target.CanCreateMainReport); everyone can create mainreports ?
        }

    }
}
