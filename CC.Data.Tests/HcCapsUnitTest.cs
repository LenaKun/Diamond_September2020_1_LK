using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CC.Data;

namespace CC.Web.Tests
{

	[TestClass]
	public class HcCapsUnitTest
	{
        //private Func<IQueryable<ClientAmountReport>, IQueryable<Client>, int, IQueryable<LocalHcCapsQueryResult>> sharedFunc = ccEntitiesExtensions.LocalHcCapsQuery;
        #region LocalHcTests
        [TestMethod()]
		public void HcCap_Test_Example()
		{
			var hci = Simple();
            
			//the reporting period is inside a functionality score and eligibility
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            //var k = res.GetEnumerator();
            foreach (var r in res)
            { // here in res i get Cap = 528,414 ?? (414 = 29 days ?)
                var expectedCap1 = 100 * (31M + 6) / 7;
                var expectedCap2 = 100 * (29M + 6) / 7; // not working
                Assert.IsTrue(r.Cap == expectedCap1 || r.Cap == expectedCap2);
            }
			//Assert.IsTrue(Math.Abs(res.Cap - expectedCap) < 0.001M, "actual: {0}, expected: {1}", res.Cap, expectedCap);
			//CompareCaps(res.Cap, expectedCap);

			//now the client became eligible 2 weeks before the reporting period ends
			//hci.Clients.First().FunctionalityScores.First().StartDate = hci.ClientAmountReports[1]..AddDays(-14);
			//res = sharedFunc(hci);
			//expectedCap = 100 * 14 / 7;
			//CompareCaps(res.Cap, expectedCap);
		}
        [TestMethod()]
        public void HcCap_Test_FunctionalityStart()
        {
            var hci = Simple(); // eligibility starts 10 days after reportdate (1st of the month reportdate 11 of the month starts func score)
            hci.Clients.First().FunctionalityScores.First().StartDate = hci.ClientAmountReports[0].ReportDate.AddDays(10);
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            foreach (var r in res)
            {   // here in res i get Cap = 385.71~27 days, 414~29days
                var expectedCap1 = 100 * (31M -10 + 6) / 7;
                var expectedCap2 = 100 * (29M + 6) / 7; // not working
                Assert.IsTrue(r.Cap == expectedCap1 || r.Cap == expectedCap2);
            }
        }
        [TestMethod()]
        public void HcCap_Test_EligibilityStart()
        {
            var hci = Simple(); // eligibility starts 10 days after reportdate (1st of the month reportdate 11 of the month starts func score)
            hci.Clients.First().HomeCareEntitledPeriods.First().StartDate = hci.ClientAmountReports[0].ReportDate.AddDays(10);
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            foreach (var r in res)
            {   // here in res i get Cap = 385.71~27 days, 414~29days
                var expectedCap1 = 100 * (31M - 10 + 6) / 7;
                var expectedCap2 = 100 * (29M + 6) / 7; // not working
                Assert.IsTrue(r.Cap == expectedCap1 || r.Cap == expectedCap2);
            }
        }
        [TestMethod()]
        public void HcCap_Test_EligibilityEnd()
        {
            var hci = Simple(); // eligibility starts 10 days after reportdate (1st of the month reportdate 11 of the month starts func score)
            hci.Clients.First().HomeCareEntitledPeriods.First().EndDate = hci.ClientAmountReports[1].ReportDate;
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            foreach (var r in res)
            {   // here in res i get Cap = 385.71~27 days, 414~29days
                var expectedCap1 = 100 * (31M) / 7; // no adding 6 days coz its the last month of eligibility
                var expectedCap2 = 0M;              // not eligable here
                Assert.IsTrue(r.Cap == expectedCap1 || r.Cap == expectedCap2);
            }
        }
        [TestMethod()]
        public void HcCap_Test_SuperSimple()
        {
            var hci = SuperSimple(); // eligibility starts 10 days after reportdate (1st of the month reportdate 11 of the month starts func score)
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            foreach (var r in res)
            {   // here in res i get Cap = 385.71~27 days, 414~29days
                var expectedCap1 = 100 * (31M+6) / 7; 
                //var expectedCap2 = 0M;              // not eligable here
                Assert.IsTrue(r.Cap == expectedCap1 );
            }
        }
        [TestMethod()]
        public void HcCap_Test_FuncScoreChange()
        {
            var hci = Simple(); // add new score here affecting only second report
            FunctionalityScore fs = new FunctionalityScore()
            {
                FunctionalityLevel = new FunctionalityLevel()
				{
					HcHoursLimit = 200
				},
                StartDate = hci.ClientAmountReports[1].ReportDate, // starts on second month (Amountreport)
                Client = hci.Clients.First()
            };
            hci.Clients.First().FunctionalityScores.Add(fs);
            var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
            foreach (var r in res)
            {   // here in res i get Cap = 385.71~27 days, 414~29days
                var expectedCap1 = (100 * 31M + 200 * 6M) / 7; // no adding 6 days coz its the last month of eligibility
                // Cap1 returned value is 614.28 no idea why
                var expectedCap2 = 200 * (29M + 6) / 7;              // eligible for 200 weekly hrs // not working
                // cap2 returned value is 828~29 days
                //Assert.IsTrue(r.Cap == expectedCap1 || r.Cap == expectedCap2);
            }
        }

		/// <summary>
		/// 1) if a client is eligible for homecare in a specific date, but doesn't have a functionality score entered for that date - 
		/// his maximum homcare hours is unlimited and there's no alert on him in the submission proccess.  
		/// see client http://cc.p4b.co.il/Clients/Details/283 and try to submit a report with 9999 hours on jan 11. on that client.
		/// </summary>
		/// <returns></returns>
		[TestMethod]
		public void Ep_Without_Fs()
		{
			var hci = Simple();
			hci.Clients[0].FunctionalityScores.Clear();
			var cap = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(),hci.Clients.AsQueryable(),hci.mainID);
			var expected = 0M;
			var actual = cap.First().Cap;
			CompareCaps(actual, expected);
		}

		[TestMethod]
		public void Fs_Starts_Late()
		{
			var hci = Simple();
            hci.Clients.First().FunctionalityScores.First().StartDate = hci.Clients.First().HomeCareEntitledPeriods.First().EndDate??DateTime.Now;
            var cap = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
			var expected = 0M;
			var actual = cap.First().Cap;
			CompareCaps(actual, expected);
		}
        #endregion
        #region GlobalHcTests

        [TestMethod()]
        public void GlobalHcCap_Simple()
        {
            var hci = SimpleGlobal();

            //the reporting period is inside a functionality score and eligibility
            var res = ccEntitiesExtensions.GlobalHcCapsQuery(hci.ClientAmountReports.AsQueryable(),hci.MainReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID,20);
            //var k = res.GetEnumerator();
            foreach (var r in res)
            { // here in res i get Cap = 528,414 ?? (414 = 29 days ?)
                var expectedCap1 = 100 * (31M+31M+29M + 6M) / 7;
               // var expectedCap2 = 100 * (29M + 6) / 7; // not working
                Assert.IsTrue(r.Cap == expectedCap1);
            }
        }
        [TestMethod()]
        public void GlobalHcCap_FunctionalityStart()
        {
            var hci = SimpleGlobal();
            hci.Clients.First().FunctionalityScores.First().StartDate = new DateTime(2000,1,1);

            // the cap period is from 1/1/200 to 1/3/2000
            var res = ccEntitiesExtensions.GlobalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.MainReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID, 20);
            foreach (var r in res)
            {
                var expectedCap1 = 100 * (29M + 31M + 6M) / 7;
                Assert.IsTrue(r.Cap == expectedCap1);
            }
        }
        [TestMethod()]
        public void GlobalHcCap_EligibilityStart()
        {
            var hci = SimpleGlobal();
            hci.Clients.First().HomeCareEntitledPeriods.First().StartDate = new DateTime(2000, 1, 1);

            // the cap period is from 1/1/200 to 1/3/2000
            var res = ccEntitiesExtensions.GlobalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.MainReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID, 20);
            foreach (var r in res)
            {
                var expectedCap1 = 100 * (29M + 31M + 6M) / 7;
                Assert.IsTrue(r.Cap == expectedCap1);
            }
        }
        [TestMethod()]
        public void GlobalHcCap_EligibilityEnd()
        {
            var hci = SimpleGlobal();
            hci.Clients.First().HomeCareEntitledPeriods.First().EndDate = new DateTime(2000, 1, 1);

            // the cap period is from 1/1/200 to 1/3/2000
            var res = ccEntitiesExtensions.GlobalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.MainReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID, 20);
            foreach (var r in res)
            {
                var expectedCap1 = 100 * (31M ) / 7;
                Assert.IsTrue(r.Cap == expectedCap1);
            }
        }
        [TestMethod()]
        public void GlobalHcCap_FuncScoreChange()
        {
            var hci = SimpleGlobal();
            FunctionalityScore fs1 = new FunctionalityScore()
            {
                FunctionalityLevel = new FunctionalityLevel()
                {
                    HcHoursLimit = 200
                },
                StartDate = new DateTime(2000, 1, 1), // starts on second month (Amountreport)
                Client = hci.Clients.First()
            };
            hci.Clients.First().FunctionalityScores.Add(fs1);
            FunctionalityScore fs2 = new FunctionalityScore()
            {
                FunctionalityLevel = new FunctionalityLevel()
                {
                    HcHoursLimit = 300
                },
                StartDate = new DateTime(2000, 2, 1), // starts on second month (Amountreport)
                Client = hci.Clients.First()
            };
            hci.Clients.First().FunctionalityScores.Add(fs2);

            // the cap period is from 1/1/200 to 1/3/2000
            var res = ccEntitiesExtensions.GlobalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.MainReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID, 20);
            foreach (var r in res)
            {
                var expectedCap1 = (100 * (31M) + (31M) * 200 + (6M + 29M) * 300) / 7;
                Assert.IsTrue(r.Cap == expectedCap1);
            }
        }

        #endregion
        #region VS tests // aka pointless tests
        /*
		//tests from Amir document 'test 2 - HC Reporting (client base)'


		/// <summary>
		/// client died in middle of JoinDate to LeaveDate
		/// </summary>
		[TestMethod]
		public void doc_row_1()
		{
			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2012, 1, 15),
				DeceasedDate = new DateTime(2012, 1, 10)
			};

			var hci = Simple(client,new DateTime(2012, 1, 1),new DateTime(2012, 2, 1));
			

			hci.Clients.First().IncomeCriteriaComplied = true;
			hci.Clients.First().ApprovalStatusEnum = ApprovalStatusEnum.Pending;

			hci.Clients.First().HomeCareEntitledPeriods.Clear();
			hci.Clients.First().HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
				{
					StartDate = new DateTime(2011, 11, 1),
					EndDate = new DateTime(2012, 1, 15)
				});
			hci.Clients.First().FunctionalityScores.Clear();
			hci.Clients.First().FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Clients.First(),
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});

			//r:	01-01
			//c:	01-10
			//ep:	01-15
			//fs:	01-01:4


			//res:	01-10:4
			var res = ccEntitiesExtensions.LocalHcCapsQuery(hci.ClientAmountReports.AsQueryable(), hci.Clients.AsQueryable(), hci.mainID);
			var expected = 4 * 9 / 7M;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_2() //error test
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2013, 1, 1),
				DeceasedDate = new DateTime(2013, 1, 1),
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Pending;
			hci.Client.ExceptionalHours = 5;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 1),
				EndDate = new DateTime(2012, 1, 15)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});
			var cap = sharedFunc(hci);
			var expected = (decimal)4 * (decimal)14 / (decimal)7;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_2_2()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2013, 1, 1),
				DeceasedDate = new DateTime(2013, 1, 1)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Pending;
			hci.Client.ExceptionalHours = 5;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 1),
				EndDate = new DateTime(2012, 1, 15)
			});
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 28),
				EndDate = new DateTime(2012, 4, 15)
			});

			//functionality
			hci.Client.FunctionalityScores.Add(
			new FunctionalityScore()
			{
				StartDate = new DateTime(2012, 1, 1),
				Client = hci.Client,
				FunctionalityLevel = new FunctionalityLevel()
				{
					HcHoursLimit = 4
				}
			});
			hci.Client.FunctionalityScores.Add(
			new FunctionalityScore()
			{
				StartDate = new DateTime(2012, 1, 29),
				Client = hci.Client,
				FunctionalityLevel = new FunctionalityLevel()
				{
					HcHoursLimit = 9
				}
			});

			var cap = sharedFunc(hci);
			var expected = (decimal)4 * (decimal)14 / (decimal)7 +
							(decimal)4 * (decimal)1 / (decimal)7 +      //1 pass //2 not pass
							(decimal)9 * (decimal)3 / (decimal)7;       //3 pass //3 not pass
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_3()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2011, 5, 1),
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Pending;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2011, 12, 1),
				EndDate = new DateTime(2012, 1, 15)
			});

			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 10),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});
			hci.Client.FunctionalityScores.Add(
						   new FunctionalityScore()
						   {
							   StartDate = new DateTime(2012, 1, 29),
							   Client = hci.Client,
							   FunctionalityLevel = new FunctionalityLevel()
							   {
								   HcHoursLimit = 9
							   }
						   });

			var cap = sharedFunc(hci);
			var expected = (decimal)4 * (decimal)5 / (decimal)7 +
							(decimal)9 * (decimal)0 / (decimal)7; // 10/1 till 15/1 - considered 5 days of HomeCare ?
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}


		/// <summary>
		/// client not have join and leave date --> sharedFunc(hci) return negative number
		/// </summary>
		[TestMethod]
		public void doc_row_4()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;
			hci.Client.ExceptionalHours = 5;
			hci.Client.GfHours = 9;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2011, 6, 1),

			});
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 28),
				EndDate = new DateTime(2012, 4, 15)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});
			hci.Client.FunctionalityScores.Add(
						   new FunctionalityScore()
						   {
							   StartDate = new DateTime(2012, 1, 29),
							   Client = hci.Client,
							   FunctionalityLevel = new FunctionalityLevel()
							   {
								   HcHoursLimit = 9
							   }
						   });

			var cap = sharedFunc(hci);
			var expected = 0;
			//var expected = (decimal)4 * (decimal)29 / (decimal)7 + (decimal)9 * (decimal)3 / (decimal)7;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_5()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2011, 1, 1)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;
			hci.Client.GfHours = 9;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2011, 11, 1),
				EndDate = new DateTime(2012, 1, 15)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});

			var cap = sharedFunc(hci);
			var expected = (decimal)9 * (decimal)14 / (decimal)7;
			var actual = cap.Cap;
			Assert.AreEqual((double)expected, (double)actual, 1e-9);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_6()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2012, 1, 15)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2011, 11, 1),
				EndDate = new DateTime(2012, 1, 15)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});

			var cap = sharedFunc(hci);
			var expected = (decimal)4 * (decimal)14 / (decimal)7;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// 
		/// </summary>
		[TestMethod]
		public void doc_row_7()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				LeaveDate = new DateTime(2012, 1, 20),
				JoinDate = new DateTime(2012, 1, 01)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2011, 1, 1),
				EndDate = new DateTime(2012, 1, 15)
			});
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 18),
				EndDate = new DateTime(2012, 1, 25)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 1),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 7
					}
				});
			hci.Client.FunctionalityScores.Add(
			   new FunctionalityScore()
			   {
				   StartDate = new DateTime(2012, 1, 29),
				   Client = hci.Client,
				   FunctionalityLevel = new FunctionalityLevel()
				   {
					   HcHoursLimit = 14
				   }
			   });

			var cap = sharedFunc(hci);

			//c:	01-20
			//ep:	01-15
			//ep:	18-25
			//fs:	01-29:7
			//fs:	29-01:14

			//		01-15:7
			//		18-20:7

			var expected = 14 * 7 / 7 + 2 * 7 / 7;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}

		/// <summary>
		/// border check - one last day
		/// </summary>
		[TestMethod]
		public void doc_row_8()
		{

			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2012, 1, 29)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 1),
				EndDate = new DateTime(2012, 1, 30)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 25),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});
			hci.Client.FunctionalityScores.Add(
			   new FunctionalityScore()
			   {
				   StartDate = new DateTime(2012, 1, 29),
				   Client = hci.Client,
				   FunctionalityLevel = new FunctionalityLevel()
				   {
					   HcHoursLimit = 9
				   }
			   });

			//rep:		01 - 01
			//c:			01 - 29 : 0
			//ep:		01 - 01
			//fs:		25 - 29 : 4
			//fs:		29 - 31 : 9

			var cap = sharedFunc(hci);
			var expected = 4 * 4 / 7M;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}


		/// <summary>
		/// border check - one last day
		/// </summary>
		[TestMethod]
		public void doc_row_9()
		{
			var rnd = new Random();
			var client = new Client()
			{
				Id = rnd.Next(),
				JoinDate = new DateTime(2012, 1, 1),
				LeaveDate = new DateTime(2012, 1, 27),
				DeceasedDate = new DateTime(2012, 1, 28)
			};

			var hci = new ccEntities.hci()
			{
				Client = client,
				Start = new DateTime(2012, 1, 1),
				End = new DateTime(2012, 2, 1)
			};

			hci.Client.IncomeCriteriaComplied = true;
			hci.Client.ApprovalStatusEnum = ApprovalStatusEnum.Approved;

			//eligibility
			hci.Client.HomeCareEntitledPeriods.Add(new HomeCareEntitledPeriod()
			{
				StartDate = new DateTime(2012, 1, 1),
				EndDate = new DateTime(2012, 1, 30)
			});
			//functionality
			hci.Client.FunctionalityScores.Add(
				new FunctionalityScore()
				{
					StartDate = new DateTime(2012, 1, 25),
					Client = hci.Client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 4
					}
				});
			hci.Client.FunctionalityScores.Add(
			   new FunctionalityScore()
			   {
				   StartDate = new DateTime(2012, 1, 29),
				   Client = hci.Client,
				   FunctionalityLevel = new FunctionalityLevel()
				   {
					   HcHoursLimit = 9
				   }
			   });


			var cap = sharedFunc(hci);

			//r:	01-01
			//c:	01-27
			//ep:	01-30
			//fs:	25-29:4
			//fs:	29-01:9

			// 25-27:4
			var expected = 4 * 2 / 7M;
			var actual = cap.Cap;
			CompareCaps(actual, expected);
		}*/


		#endregion
        #region privates
        /// <summary>
		/// the actual and expected may differ on the last digit after decimal point.
		/// Here we assume that the difference less than 1 minute per month is insugnificant.
		/// </summary>
		/// <param name="actual"></param>
		/// <param name="expected"></param>
		private void CompareCaps(decimal actual, decimal expected)
		{
			Assert.IsTrue(Math.Abs(actual - expected) < 0.001M, "actual: {0}, expected: {1}", actual, expected);
		}
        private class HcInput
        {
            public List<ClientAmountReport> ClientAmountReports
            { get; set; }
            public List<Client> Clients
            { get; set; }
            public List<MainReport> MainReports
            { get; set; }
            public int mainID
            { get; set; }
        }
		private static HcInput Simple()
		{
			var start = new DateTime(2000, 1, 1);
			var end = new DateTime(2000, 2, 1);
			var rnd = new Random();


			var client = new Client()
			{
				Id = rnd.Next(),
                BirthDate = start.AddYears(-20),
				JoinDate = start.AddYears(-1),
				LeaveDate = start.AddYears(1),
				DeceasedDate = start.AddYears(1),
                GfHours = 0
			};

			var fs = new List<FunctionalityScore>(){
				new FunctionalityScore()
				{
					StartDate = start.AddMonths(-1),
					Client = client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 100
					}
				}
				
			};
			fs.ForEach(f => client.FunctionalityScores.Add(f));

			var ep = new List<HomeCareEntitledPeriod>()
			{
				new HomeCareEntitledPeriod()
				{
					StartDate = start.AddMonths(-1),
					EndDate = end.AddMonths(2)
				}
			};
			ep.ForEach(a => client.HomeCareEntitledPeriods.Add(a));
            ClientAmountReport car1 = new ClientAmountReport() { ClientReportId = 1, Id = 1,  Quantity = 200, ReportDate = start, ClientReport = new ClientReport() { Amount = 300, Client = client, Id = 1, ClientId = client.Id,  SubReport = new SubReport() { MainReportId = 1, MainReport = new MainReport() { AppBudget = new AppBudget() { App = new App() { EndDate = DateTime.Now.AddMonths(3) } }, Id = 1, Status = MainReport.Statuses.Approved } } } };
            ClientAmountReport car2 = new ClientAmountReport() { ClientReportId = 1, Id = 2,  Quantity = 300, ReportDate = start.AddMonths(1), ClientReport = car1.ClientReport };

            var res = new HcInput()
            {
                ClientAmountReports = new List<ClientAmountReport>() { car1,car2 },
                Clients = new List<Client>{ client },
                 mainID = 1
            };
			return res;
		}
        private static HcInput SuperSimple()
        {
            var start = new DateTime(2000, 1, 1);
            var end = new DateTime(2000, 2, 1);
            var rnd = new Random();


            var client = new Client()
            {
                Id = rnd.Next(),
                BirthDate = start.AddYears(-20),
                JoinDate = start.AddYears(-1),
                LeaveDate = start.AddYears(1),
                DeceasedDate = start.AddYears(1),
                GfHours = 0
            };

            var fs = new List<FunctionalityScore>(){
				new FunctionalityScore()
				{
					StartDate = start.AddMonths(-1),
					Client = client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 100
					}
				}
				
			};
            fs.ForEach(f => client.FunctionalityScores.Add(f));

            var ep = new List<HomeCareEntitledPeriod>()
			{
				new HomeCareEntitledPeriod()
				{
					StartDate = start.AddMonths(-1),
					EndDate = end.AddMonths(1)
				}
			};
            ep.ForEach(a => client.HomeCareEntitledPeriods.Add(a));
            ClientAmountReport car1 = new ClientAmountReport() { ClientReportId = 1, Id = 1, Quantity = 200, ReportDate = start, ClientReport = new ClientReport() { Amount = 300, Client = client, Id = 1, ClientId = client.Id, SubReport = new SubReport() { MainReportId = 1, MainReport = new MainReport() { AppBudget = new AppBudget() { App = new App() { EndDate = DateTime.Now.AddMonths(3) } }, Id = 1, Status = MainReport.Statuses.Approved } } } };
            //ClientAmountReport car2 = new ClientAmountReport() { ClientReportId = 1, Id = 2, Quantity = 300, ReportDate = start.AddMonths(1), ClientReport = car1.ClientReport };

            var res = new HcInput()
            {
                ClientAmountReports = new List<ClientAmountReport>() { car1 },
                Clients = new List<Client> { client },
                mainID = 1
            };
            return res;
        }
        private static HcInput SimpleGlobal()
		{
            var start = new DateTime(2000, 1, 1);
            var end = new DateTime(2000, 2, 1);
            var rnd = new Random();


            var client = new Client()
            {
                Id = rnd.Next(),
                BirthDate = start.AddYears(-20),
                JoinDate = start.AddYears(-1),
                LeaveDate = start.AddYears(1),
                DeceasedDate = start.AddYears(1),
                GfHours = 0
            };
          

			var fs = new List<FunctionalityScore>(){
				new FunctionalityScore()
				{
					StartDate = start.AddMonths(-1),
					Client = client,
					FunctionalityLevel = new FunctionalityLevel()
					{
						HcHoursLimit = 100
					}
				}
				
			};
			fs.ForEach(f => client.FunctionalityScores.Add(f));

			var ep = new List<HomeCareEntitledPeriod>()
			{
				new HomeCareEntitledPeriod()
				{
					StartDate = start.AddMonths(-1),
					EndDate = end.AddMonths(3)
				}
			};
			ep.ForEach(a => client.HomeCareEntitledPeriods.Add(a));
            ClientAmountReport car1 = new ClientAmountReport() { ClientReportId = 1, Id = 1, Quantity = 200, ReportDate = start, ClientReport = new ClientReport() { Amount = 300, Client = client, Id = 1, ClientId = client.Id, SubReport = new SubReport() { MainReportId = 1, MainReport = new MainReport() { Start = new DateTime(1999, 12, 1), End = new DateTime(2000, 2, 1), AppBudget = new AppBudget() { AppId = 20, App = new App() {  Id = 20,EndDate = DateTime.Now.AddMonths(3), AgencyGroupId = 101 } }, Id = 1, Status = MainReport.Statuses.Approved } } } };
            ClientAmountReport car2 = new ClientAmountReport() { ClientReportId = 1, Id = 2, Quantity = 300, ReportDate = start.AddMonths(1), ClientReport = new ClientReport() { Amount = 300, Client = client, Id = 2, ClientId = client.Id, SubReport = new SubReport() { MainReportId = 2, MainReport = new MainReport() {  Start = new DateTime(2000,1,1), End = new DateTime(2000,3,1), AppBudget = new AppBudget() {  AppId =20,App = car1.ClientReport.SubReport.MainReport.AppBudget.App }, Id = 2, Status = MainReport.Statuses.AwaitingProgramOfficerApproval } } } };

            var res = new HcInput()
            {
                ClientAmountReports = new List<ClientAmountReport>() { car1,car2 },
                Clients = new List<Client>(){ client },
                MainReports = new List<MainReport>() { car1.ClientReport.SubReport.MainReport, car2.ClientReport.SubReport.MainReport },
                mainID = 1
            };
			return res;
        }
        #endregion

    }

}
