using CC.Web.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web;
using System.Web.Routing;
using System.Security.Principal;
using System.IO;
using System.Data;
using System.Data.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using CC.Data;
using Moq;
using System.Collections.Generic;
using CC.Web.Models;
using System.Web.Mvc;

namespace CC.Data.Tests
{  // class to remove the readonly
    public class FakeObjectSet<T> : IObjectSet<T> where T : class
    {
        HashSet<T> _data;
        IQueryable _query;

        public FakeObjectSet() : this(new List<T>()) { }

        public FakeObjectSet(IEnumerable<T> initialData)
        {
            _data = new HashSet<T>(initialData);
            _query = _data.AsQueryable();
        }

        public void Add(T item)
        {
            _data.Add(item);
        }

        public void AddObject(T item)
        {
            _data.Add(item);
        }

        public void Remove(T item)
        {
            _data.Remove(item);
        }

        public void DeleteObject(T item)
        {
            _data.Remove(item);
        }

        public void Attach(T item)
        {
            _data.Add(item);
        }

        public void Detach(T item)
        {
            _data.Remove(item);
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
   /*
            this.AddObject("FunctionalityLevels", new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-2), MinScore = 4, MaxScore = 7 });
            this.AddObject("FunctionalityLevels", new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-3), MinScore = 1, MaxScore = 4 });
            this.AddObject("FunctionalityLevels", new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-2), MinScore = 7, MaxScore = 10 });
            this.AddObject("FunctionalityScores", new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(-1), ClientId = 0 ,DiagnosticScore = 5 });
            this.AddObject("FunctionalityScores", new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(0), ClientId = 1, DiagnosticScore = 2 });
            this.AddObject("FunctionalityScores", new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(1), ClientId = 2, DiagnosticScore = 8 });
    */
    /// <summary>
    ///This is a test class for FunctionalityScoresControllerTest and is intended
    ///to contain all FunctionalityScoresControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FunctionalityScoresControllerTest
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
        ///A test for Create
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        
        //[HostType("ASP.NET")]
        //[AspNetDevelopmentServerHost("E:\\Projects\\For_Backup\\CC(sergey)\\Code\\CC.Web", "/")]
        //[UrlToTest("http://localhost:61118/")]
        public interface IccEntities : IDisposable
        {
            IObjectSet<FunctionalityLevel> FunctionalityLevels { get; }
            IObjectSet<FunctionalityScore> FunctionalityScores { get; }
            int SaveChanges();
        }
        private static Mock<ccEntities> initDB(List<FunctionalityScore> fs,List<FunctionalityLevel> fl)
        {
            var db = new Mock<ccEntities>();
            var referencePoint = new DateTime(2000, 1, 1);
            if (fs == null)
            {
                fs = new List<FunctionalityScore>(){
                new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(-1), ClientId = 0 ,DiagnosticScore = 5 },
                new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(0), ClientId = 1, DiagnosticScore = 2 },
                new FunctionalityScore() { Id = 0, StartDate = referencePoint.AddMonths(1), ClientId = 2, DiagnosticScore = 8 } };
            }
            if (fl == null)
            {
                fl = new List<FunctionalityLevel>(){
                new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-2), MinScore = 4, MaxScore = 7 },
                new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-3), MinScore = 1, MaxScore = 4 },
                new FunctionalityLevel() { Id = 0, StartDate = referencePoint.AddMonths(-2), MinScore = 7, MaxScore = 10 } };
            }
           // db.Setup(x => x.FunctionalityScores).Returns(new FakeObjectSet<FunctionalityScore>(fs));
           // db.Setup(x => x.FunctionalityLevels).Returns(new FakeObjectSet<FunctionalityLevel>(fl));
            return db;
        }
        [TestMethod()]
        public void CreateTest()
        {

            FunctionalityScoresController target = new FunctionalityScoresController();
            var db = FunctionalityScoresControllerTest.initDB(null,null);
           

            
            //FunctionalityScore score = new FunctionalityScore() { ClientId = 0, StartDate = db.referencePoint, DiagnosticScore = 4 };
            //target.MakeScore(score, db);
           // Assert.IsTrue(target.ModelState.Values.Any(f => f.Errors.Any(a => a.ErrorMessage == "Duplicate functionality scores are not allowed")));
            //target.Permissions = new Services.;
            //score.DiagnosticScore = 8M;

            ActionResult expected = null; // TODO: Initialize to an appropriate value
            bool actual;
            //actual = target.MakeScore(score,db) as ViewResult;
            //Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for MakeScore
        ///</summary>
        // TODO: Ensure that the UrlToTest attribute specifies a URL to an ASP.NET page (for example,
        // http://.../Default.aspx). This is necessary for the unit test to be executed on the web server,
        // whether you are testing a page, web service, or a WCF service.
        [TestMethod()]
        //[HostType("ASP.NET")]
        //[UrlToTest("http://localhost/CC")]
        public void MakeScoreTest()
        {
            /*FunctionalityScoresController target = new FunctionalityScoresController(); // TODO: Initialize to an appropriate value
            FunctionalityScore score = null; // TODO: Initialize to an appropriate value
            ccEntities db = new MockDBf(); // TODO: Initialize to an appropriate value
            FunctionalityScore expected = null; // TODO: Initialize to an appropriate value
            FunctionalityScore actual;
            actual = target.MakeScore(score, db);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");*/
        }
    }
}
