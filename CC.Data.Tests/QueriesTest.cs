using CC.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;


namespace CC.Data.Tests
{
    
    
    /// <summary>
    ///This is a test class for QueriesTest and is intended
    ///to contain all QueriesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class QueriesTest
    {
        Random rnd = new Random();

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
        ///A test for EmergencyCapSummary
        ///</summary>
        [TestMethod()]
        public void EmergencyCapSummaryTest()
        {
            Func<IQueryable<EmergencyCap>, IQueryable<Client>, IQueryable<ClientReport>, int, IQueryable<Queries.EmergencyCapValidationResult>> actual;
            actual = Queries.EmergencyCapSummary;
            List<EmergencyCap> EmergencyCaps = new List<EmergencyCap>();
            List<App> Apps = new List<App>();
            List<ClientReport> ClientReports = new List<ClientReport>();
            List<Client> Clients = new List<Client>();
            String[] Countries = { "Israel", "Russia", "USA", "Germany", "Italy", "Poland", "Brazil", "Australia", "New Zeland", "Serbia" };

            #region Building_Queryables
            for (int i = 0; i < 10; i++)  // building queryables
            {
                EmergencyCaps.Add(new EmergencyCap()
                {
                    Id = i,
                    Name = Countries[i]+ " Cap",
                    CapPerPerson = 100+i,//rnd.Next(),
                    DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                    StartDate = DateTime.Now.AddMonths(-3),
                    EndDate = DateTime.Now,
                    Currency = new Currency()
                    {
                        Id = rnd.Next(100, 999).ToString(),
                        ExcRate = (decimal)rnd.NextDouble() * 10
                    },
                    Countries = new List<Country>()
                });
                EmergencyCaps.Find(f => f.Id == i).Countries.Add(new Country() { Id = i,Name = Countries[i] });
                ClientReports.Add(new ClientReport()
			{
				ReportDate = DateTime.Now.AddMonths(-3) + new TimeSpan(( DateTime.Now - DateTime.Now.AddMonths(-3)).Ticks / 2),
				Amount = EmergencyCaps.Find(f => f.Id == i).CapPerPerson,
				Discretionary = EmergencyCaps.Find(f => f.Id == i).CapPerPerson * EmergencyCaps.Find(f => f.Id == i).DiscretionaryPercentage,
				PurposeOfGrant = "blah blah blah",
				Id = i,
				Rate = null,
				Client = new Client()
				{
					Id = rnd.Next(),
					FirstName = "a"+i.ToString(),
					LastName = "b"+i.ToString()
				},
				SubReport = new SubReport()
				{
					Id = rnd.Next(),
					AppBudgetService = new AppBudgetService()
					{
						Id = rnd.Next(),
						Service = new Service()
						{
							Id = rnd.Next(),
							ReportingMethodId = (int)Service.ReportingMethods.Emergency
						}
					},
					MainReport = new MainReport()
					{
						Id = i,
						ExcRate = (decimal)rnd.NextDouble() * 10,
						AppBudget = new AppBudget()
						{
							Id = rnd.Next(),
							App = new App()
							{
								Id = rnd.Next(),
								Fund = new Fund()
								{
									Id = rnd.Next(),
									Currency = new Currency()
									{
										Id = rnd.Next(100, 999).ToString(),
										ExcRate = (decimal)rnd.NextDouble() * 10
									},
									EmergencyCaps = new[] { EmergencyCaps.Find(f => f.Id == i) }
								},
								AgencyGroup = new AgencyGroup()
								{
									Id = rnd.Next(),
									Currency = new Currency()
									{
										Id = rnd.Next(100, 999).ToString(),
										ExcRate = (decimal)rnd.NextDouble() * 10
									},
                                    Country = EmergencyCaps.Find(f => f.Id == i).Countries.ToList<Country>().Find(f => f.Id == i)
								}
							},

						}

					}
				}
			});
                var myclientrep = ClientReports.Find(f => f.Id == i);
                EmergencyCaps.Find(f => f.Id == i).Funds.Add(myclientrep.SubReport.MainReport.AppBudget.App.Fund);
                EmergencyCaps.Find(f => f.Id == i).CapPerPerson = (ClientReports.Find(f => f.Id == i).Amount ?? 0)
                * ClientReports.Find(f => f.Id == i).SubReport.MainReport.ExcRate
                * ClientReports.Find(f => f.Id == i).SubReport.MainReport.AppBudget.App.Fund.Currency.ExcRate
            / EmergencyCaps.Find(f => f.Id == i).Currency.ExcRate;
            }
            #endregion
            #region testing
            for (int i = 0; i < 10; i++)
            {
                var q = actual(EmergencyCaps.AsQueryable(), Clients.AsQueryable(), ClientReports.AsQueryable(), ClientReports.Find(f => f.Id == i).SubReport.MainReportId);
                foreach (var item in q)
                {
                    Assert.IsTrue(item.CapName == Countries[i] + " Cap");
                    Assert.AreEqual((double)item.TotalAmount, (double)item.CapPerPerson, 10e-9, "Calculated amount should be equal to cap");
                    Assert.IsTrue(item.DiscretionaryPercentage == EmergencyCaps.Find(f => f.Id == i).DiscretionaryPercentage);
                    Assert.IsTrue(item.DiscretionaryPercentage == EmergencyCaps.Find(f => f.Id == i).DiscretionaryPercentage);
                    Assert.IsTrue(item.EmergencyCapId == i);
                    Assert.IsTrue(item.ClientFirstName == "a"+i.ToString());
                }
            }
            var a = actual(EmergencyCaps.AsQueryable(), Clients.AsQueryable(), ClientReports.AsQueryable(), 11);
            foreach (var item in a)
            {
                Assert.IsTrue(false);
            }
            EmergencyCaps.Clear();
            a = actual(EmergencyCaps.AsQueryable(), Clients.AsQueryable(), ClientReports.AsQueryable(), ClientReports.Find(f => f.Id == 5).SubReport.MainReportId);
            foreach (var item in a)
            {
                Assert.IsTrue(false);
             }
            #endregion

         }
    }
}
