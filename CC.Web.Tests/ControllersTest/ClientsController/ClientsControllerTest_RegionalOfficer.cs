
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using CC.Data;
using CC.Data.Tests;
using CC.Web.Tests;
using CC.Web;
using CC.Web.Controllers;
using CC.Web.Models;

namespace CC.Web.Tests
{


    //folder for controller
    //file for every role - copy - but different result
    //property get target that create target for current user
    //type of test: sequrity - allowed/denied
    //sequrity - filtered data
    //update - verify result


    /// <summary>
    ///This is a test class for ClientsControllerTest and is intended
    ///to contain all ClientsControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ClientsControllerTest_RegionalOfficer
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        ///

        public ClientsController GetTarget_ForUser(FixedRoles role, string AgencyName = "")
        {

            ClientsController target = new ClientsController();
            ccEntities context = new ccEntities();

            target.CcUser = Helper.GetUser(role, AgencyName);
            return target;

        }

        ClientsController target;
        ContextMocks mocks;
        public ClientsController Target
        {
            get
            {
                if (target == null)
                {
                    target = GetTarget_ForUser(FixedRoles.RegionOfficer, "Agency1_FirstTest");
                    mocks = new ContextMocks(Target, true, true);
                }
                return target;
            }
            set
            {
                target = value;
            }
        }


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
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            CC.Data.Tests.Helper.PrepareTestData();
        }
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
        ///A test for Index
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.

        [TestMethod()]
        public void IndexTest()
        {

            //show clients list for user

            target = GetTarget_ForUser(FixedRoles.Admin);
            ViewResult actual = Target.Index();

            Assert.IsNotNull(actual, "Did not render a view");

            Assert.IsFalse(actual.ViewBag.Permission == CC.Data.Services.PermissionsFactory.GetPermissionsFor(target.CcUser), "Permissions is wrong");
            Assert.IsFalse(actual.ViewName != "", "name of the view must be empty");

        }





        [TestMethod()]
        public void IndexTest_TryUpdateStatus_NotAdmin()
        {
            //try to change approval status without select clients

            ClientsListModel model = new ClientsListModel();
            List<int> ClientIds = null;
            ClientIds = CC.Data.Tests.Helper.GetTestClientIdsList();
            model.UpdateModel.SelectedClientIds = ClientIds;

            model.UpdateModel.NewApprovalStatusId = (int)ApprovalStatusEnum.Approved;



            ActionResult actual = Target.Index(model);

            Assert.IsNotNull(actual, "action result can not be null");
            string content = ((System.Web.Mvc.ContentResult)actual).Content;
            Assert.IsTrue(content == "You are not allowed to change the approval status", "Only admin can update approval status");

        }













        [TestMethod()]
        public void IndexDataTable_Test()
        {
            string sort = "asc";
            string col = "1";
            int paramStart = 0;



            Target.Request.QueryString.Add("sSortDir_0", sort);
            Target.Request.QueryString.Add("iSortCol_0", col);




            ClientsListDataTableModel param = new ClientsListDataTableModel();

            int n = new ccEntities().Clients.Count();


            ActionResult actual;

            param.iDisplayLength = n;
            param.iDisplayStart = paramStart;
            actual = target.IndexDataTables(param);

            Assert.IsNotNull(param.aaData, "result data can not be empty");

            int m = param.aaData.ToList<ClientsListEntry>().Count;




            //verify info      
            var c1 = param.aaData.Where(x => x.FirstName == "Client1");
            Assert.IsTrue(c1.Count() > 0, "Regional Officer for First Region must get Client1");

            //var c2 = param.aaData.Where(x => x.FirstName == "Client3");
            //Assert.IsTrue(c2.Count() == 0, "Regional Officer for First Region can not clients from other region");






        }






        [TestMethod()]
        public void Details_Test()
        {
            CC.Data.Tests.Helper.PrepareTestData();
            ClientsController target = new ClientsController(); // TODO: Initialize to an appropriate value
            User ccUser = CC.Data.Tests.Helper.GetAdminUser();
            var mocks = new ContextMocks(target, true, false);
            target.CcUser = ccUser;
            Client cc = CC.Data.Tests.Helper.GetClient("Client1");
            Nullable<int> id = cc.Id;
            Nullable<bool> newClient = false;

            ViewResult actual = target.Details(id, newClient);

            Assert.IsNotNull(actual, " view result can not be null");

            System.Web.Mvc.ViewResultBase viewResult = (System.Web.Mvc.ViewResultBase)actual;

            ViewDataDictionary data = viewResult.ViewData;

            Assert.IsNotNull(data.Keys.Count() == 3, "must be 3 dataitems: client data, agencies, permissions");

            Client cl = (Client)data.Model;

            Assert.IsTrue(cl.Id == cc.Id && cl.FirstName == "Client1", " must open detail of client with name " + cc.FirstName);

            //now write the same test for user, that not have permissions for this client"

        }







        [TestMethod()]
        public void Create_Test()
        {

            ActionResult actual = Target.Create();



            Assert.IsNotNull(actual, "action result can not be null");
            Assert.IsNotNull(actual, "action result can not be null");
            string actionName = (((System.Web.Mvc.RedirectToRouteResult)(actual))).RouteValues["action"].ToString();
            Assert.IsTrue(actionName == "Index", "Regional Officer can not create new clients");



        }


        [TestMethod()]
        public void CreatePost_Test()
        {
            ClientCreateModel cm = new ClientCreateModel();
            cm.Data = new Client();
            string randomName = System.IO.Path.GetRandomFileName();
            cm.Data.FirstName = randomName;
            cm.Data.LastName = "last";
            cm.Data.City = "city";
            cm.Data.Address = "address";
            cm.Data.AgencyId = new ccEntities().Agencies.First().Id;
            cm.Data.CountryId = new ccEntities().Countries.First().Id;
            cm.Data.JoinDate = DateTime.Now;


            ActionResult actual = Target.Create(cm);


            Assert.IsNotNull(actual, "action result can not be null");
            string actionName = (((System.Web.Mvc.RedirectToRouteResult)(actual))).RouteValues["action"].ToString();
            Assert.IsTrue(actionName == "Index", "Regional Officer can not create new clients");



        }


        [TestMethod()]
        public void Edit_Test()
        {
            Client cl = new ccEntities().Clients.Single(x => x.FirstName == "Client1");


            ActionResult actual = Target.Edit(cl.Id);
            ActionResult actualView = (ActionResult)actual;

            Assert.IsNotNull(actual, "action result can not be null");

            string actionName = (((System.Web.Mvc.RedirectToRouteResult)(actual))).RouteValues["action"].ToString();
            Assert.IsTrue(actionName == "Index", "Regional Officer can not edit client data");


        }



        [TestMethod()]
        public void EditPost_Test()
        {
            Client cl = new ccEntities().Clients.Single(x => x.FirstName == "Client1");
            ClientEditModel cm = new ClientEditModel();
            cm.Data = cl;
            cl.City = "changed city";



            ActionResult actual = Target.Edit(cl.Id);


            Assert.IsNotNull(actual, "action result can not be null");

            string actionName = (((System.Web.Mvc.RedirectToRouteResult)(actual))).RouteValues["action"].ToString();
            Assert.IsTrue(actionName == "Index", "Regional Officer can not edit client data");

        }

    }
}