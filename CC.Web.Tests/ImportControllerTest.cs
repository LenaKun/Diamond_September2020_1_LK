using CC.Web.Areas.Admin.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;
using System;
using System.Linq;
using CC.Data;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using CC.Data.Repositories;


namespace CC.Web.Tests
{

    /// <summary>
    ///This is a test class for ImportControllerTest and is intended
    ///to contain all ImportControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ImportControllerTest
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
        ///A test for Upload
        ///</summary>


        [TestMethod()]
        public void UploadTest()
        {

            var controller = new ImportController<Client, ImportAppStatusRow>();
            var tempPath = System.IO.Path.GetTempPath();
            controller.UploadsDir = tempPath;

            var postedFile = new Moq.Mock<HttpPostedFileBase>();
            var len = 1000;
            var content = new byte[len];
            var rnd = new Random();
            rnd.NextBytes(content);
            System.IO.MemoryStream ms = new System.IO.MemoryStream(content);
            postedFile.Setup(f => f.InputStream).Returns(ms);
            postedFile.Setup(f => f.SaveAs(Moq.It.IsAny<string>())).Verifiable();


            var result = controller.Upload(postedFile.Object);

            var redirect = result as RedirectToRouteResult;
            Assert.IsTrue(redirect != null);

            var id = (Guid)redirect.RouteValues["id"];

            var action = (string)redirect.RouteValues["action"];
            Assert.IsTrue(!string.IsNullOrEmpty(action));
            Assert.IsTrue(action.ToLower() == "Preview".ToLower());

            postedFile.Verify(f => f.SaveAs(Moq.It.IsAny<string>()));

        }


        [TestMethod()]
        public void CombineTest()
        {

            var controller = new ImportController_Accessor<Client, ImportAppStatusRow>();


            Client sharedClient;
            ImportAppStatusRow newImportRow;
            IEnumerable<Joinedrecord<Client, ImportAppStatusRow>> result;
            CombineHelper(controller, out sharedClient, out newImportRow, out result);

            Assert.IsTrue(result.Count() == 2);

            var update = result.Single(f => f.Target == sharedClient);
            Assert.IsTrue(update.Action == ImportActionEnum.Update);
            var insert = result.Single(f => f.Source == newImportRow);
            Assert.IsTrue(insert.Action == ImportActionEnum.Insert);

        }

        /// <summary>
        /// test for a case when the controller supports updates only and we give it a new record
        /// </summary>
        [TestMethod]
        public void Combine1()
        {
            var controller = new ImportController_Accessor<Client, ImportAppStatusRow>();
            controller._importAction = ImportActionEnum.Update;
            Client sharedClient;
            ImportAppStatusRow newImportRow;
            IEnumerable<Joinedrecord<Client, ImportAppStatusRow>> result;
            CombineHelper(controller, out sharedClient, out newImportRow, out result);

            Assert.IsTrue(result.Count() == 2);

            var update = result.Single(f => f.Target == sharedClient);
            Assert.IsTrue(update.Action == ImportActionEnum.Update);
            Assert.IsTrue(update.Exception == null);
            
            var insert = result.Single(f => f.Source == newImportRow);
            Assert.IsTrue(insert.Action == ImportActionEnum.Undefined);
            Assert.IsTrue(insert.Exception != null);
        }

        private static void CombineHelper(ImportController_Accessor<Client, ImportAppStatusRow> controller, out Client sharedClient, out ImportAppStatusRow newImportRow, out IEnumerable<Joinedrecord<Client, ImportAppStatusRow>> result)
        {
            var rnd = new Random();
            sharedClient = new Client() { Id = 1, FirstName = "Moshe" };
            var dbOnlyClient = new Client() { Id = 2, FirstName = "asdf" };
            var sharedRow = new ImportAppStatusRow() { Id = 1 };
            newImportRow = new ImportAppStatusRow() { Id = 3 };//new

            var dbClients = new List<Client>()
            {
                sharedClient,
                dbOnlyClient
            };

            Moq.Mock<IRepository<Client>> repository = new Moq.Mock<IRepository<Client>>();
            repository.SetupGet(f => f.Select).Returns(dbClients.AsQueryable());


            controller.Repository = repository.Object;



            var chunk = new List<ImportAppStatusRow>()
            {
                sharedRow,
                newImportRow
            };

            result = controller.Combine(chunk, repository.Object);
        }

    }
}
