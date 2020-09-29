using CC.Data.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using CC.Data;
using System.Linq;
using System.Linq.Expressions;

namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for PermissionsFactory_AgencyUserPermissionsTest and is intended
    ///to contain all PermissionsFactory_AgencyUserPermissionsTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PermissionsFactory_AgencyUserPermissionsTest
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
        ///A test for AgencyUserPermissions Constructor
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void PermissionsFactory_AgencyUserPermissionsConstructorTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
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
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Agency, bool> AgencyFilter = target.AgencyFilter.Compile();
            Agency param = new Agency() {  Users = new List<User>() { new User() { Id = 0 } }};
            Assert.IsFalse(AgencyFilter(param));
            param.Users.Add( user );
            Assert.IsTrue(AgencyFilter(param));
        }

        /// <summary>
        ///A test for AgencyGroupsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AgencyGroupsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AgencyGroup, bool> AgencyGroupsFilter = target.AgencyGroupsFilter.Compile();
            AgencyGroup param = new AgencyGroup() { Agencies = new List<Agency>() { new Agency(){ Users = new List<User>(){ new User(){ Id = 0}}}} };
            Assert.IsFalse(AgencyGroupsFilter(param));
            param.Agencies.Add(new Agency() { Users = new List<User>() { user} });
            Assert.IsTrue(AgencyGroupsFilter(param));
        }

        /// <summary>
        ///A test for AppBudgetServicesFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetServicesFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudgetService, bool> AppBudgetServicesFilter = target.AppBudgetServicesFilter.Compile();
            AppBudgetService s = new AppBudgetService() { AgencyId = 0 };
            Assert.IsFalse(AppBudgetServicesFilter(s));
            s.AgencyId = 1;
            Assert.IsTrue(AppBudgetServicesFilter(s));
        }

        /// <summary>
        ///A test for AppBudgetsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppBudgetsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<AppBudget, bool> AppBudgetsFilter = target.AppBudgetsFilter.Compile();
            AppBudget ab = new AppBudget() { App = new App() { AgencyGroup = new AgencyGroup() { Agencies = new List<Agency>() { new Agency() { Users = new List<User>(){ new User(){ Id = 0}} } } } } };
            Assert.IsFalse(AppBudgetsFilter(ab));
            ab.App.AgencyGroup.Agencies.Add(new Agency() { Users = new List<User>() { user} });
            Assert.IsTrue(AppBudgetsFilter(ab));
        }

        /// <summary>
        ///A test for AppsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AppsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<App, bool> AppsFilter = target.AppsFilter.Compile();
            App a = new App() { AgencyGroup = new AgencyGroup() { Agencies = new List<Agency>() { new Agency(){ Id = 0 } } } };
            Assert.IsFalse(AppsFilter(a));
            a.AgencyGroup.Agencies.Add(new Agency() { Id = 1 });
            Assert.IsTrue(AppsFilter(a));
        }

        /// <summary>
        ///A test for CanCreateNewClient
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanCreateNewClientTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanCreateNewClient);
        }

        /// <summary>
        ///A test for CanUpdateExistingClient
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void CanUpdateExistingClientTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsTrue(target.CanUpdateExistingClient);
        }

        /// <summary>
        ///A test for ClientReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void ClientReportsFilterTest()
        {
            User user = new User() { Id = 1, AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<ClientReport, bool> ClientReportsFilter = target.ClientReportsFilter.Compile();
            ClientReport cr = new ClientReport() { Client = new Client() { AgencyId = 0 }, SubReport = new SubReport() { AppBudgetService = new AppBudgetService() { AgencyId = 1 } } };
            Assert.IsFalse(ClientReportsFilter(cr));
            cr.Client.AgencyId = 1;
            cr.SubReport.AppBudgetService.AgencyId = 0;
            Assert.IsFalse(ClientReportsFilter(cr));
            cr.SubReport.AppBudgetService.AgencyId = 1;
            Assert.IsTrue(ClientReportsFilter(cr));
        }

        /// <summary>
        ///A test for ClientsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void ClientsFilterTest() /// fixed
        {
            User user = new User(){Id=1, AgencyId = 1 ,RoleId=(int)FixedRoles.AgencyUser};
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<Client, bool> ClientsFilter = target.ClientsFilter.Compile();
            Client c = new Client() { Agency = new Agency() { Users = new List<User>() { new User() { Id = 0 } } } };
            Assert.IsFalse(ClientsFilter(c));
           
            c.Agency.Users.Add(user);
            Assert.IsTrue(ClientsFilter(c));
        }

        /// <summary>
        ///A test for MainReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void MainReportsFilterTest()
        {
            User user = new User() { AgencyId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Func<MainReport, bool> MainReportsFilter = target.MainReportsFilter.Compile();
            MainReport mr = new MainReport() { AppBudget = new AppBudget() { App = new App() { AgencyGroup = new AgencyGroup() { Agencies = new List<Agency>() { new Agency() { Id = 1 } } } } } };
            Assert.IsTrue(MainReportsFilter(mr));
            mr.AppBudget.App.AgencyGroup.Agencies.Clear(); //  disallow another agencies mainreports
            Assert.IsFalse(MainReportsFilter(mr));
        }

        /// <summary>
        ///A test for SubReportsFilter
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void SubReportsFilterTest()
        {
            User user = new User(){ AgencyId = 1 ,RoleId=(int)FixedRoles.AgencyUser};
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);// new PermissionsFactory.AgencyUserPermissions(user); // TODO: Initialize to an appropriate value
            Func<SubReport, bool> SubReportsFilter = target.SubReportsFilter.Compile();
            SubReport sr = new SubReport() { AppBudgetService = new AppBudgetService() { AgencyId = 1 } };
            Assert.IsTrue(SubReportsFilter(sr));
            sr.AppBudgetService.AgencyId = 0; //  disallow another agencies subreports
            Assert.IsFalse(SubReportsFilter(sr));
        }
        /// <summary>
        ///access denied asserts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("CC.Data.dll")]
        public void AccessDeniedTest()
        {
            User user = new User() { AgencyId = 1, AgencyGroupId = 1, RoleId = (int)FixedRoles.AgencyUser };
            IPermissionsBase target = PermissionsFactory.GetPermissionsFor(user);
            Assert.IsFalse(target.CanEditCeefFields);
            Assert.IsFalse(target.CanChangeApprovalStatus);
            Assert.IsFalse(target.CanChangeDeceasedDate);
            Assert.IsFalse(target.CanChangeGfHours);
            Assert.IsFalse(target.CanChangeExceptionalHours);
            Assert.IsFalse(target.CanDeleteHcePeriod);
            Assert.IsFalse(target.CanDeleteFuncScore);
            Assert.IsFalse(target.CanSeeProgramField);
            Assert.IsTrue(target.CanCreateNewClient);
            Assert.IsTrue(target.CanUpdateExistingClient);
            // Assert.IsTrue(target.CanCreateMainReport); everyone can create mainreports ?
        }
    }
}
