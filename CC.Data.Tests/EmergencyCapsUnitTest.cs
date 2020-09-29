using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using CC.Data;

namespace CC.Data.Tests
{

	[TestClass]
	public class EmergencyCapsUnitTests
	{
		Random rnd = new Random();

		[TestMethod]
		public void SingleReport_SingleCap()
		{

			List<EmergencyCap> EmergencyCaps = new List<EmergencyCap>();
			List<App> Apps = new List<App>();
			List<ClientReport> ClientReports = new List<ClientReport>();
			List<Client> Clients = new List<Client>();


			var relevantEmergencyCap = new EmergencyCap()
			{
				CapPerPerson = 100,//rnd.Next(),
				DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
				StartDate = DateTime.Now.AddMonths(-3),
				EndDate = DateTime.Now,
				Currency = new Currency()
				{
					Id = rnd.Next(100, 999).ToString(),
					ExcRate = (decimal)rnd.NextDouble() * 10
				}
			};


			var relevantClientReport = new ClientReport()
			{
				ReportDate = relevantEmergencyCap.StartDate + new TimeSpan((relevantEmergencyCap.EndDate - relevantEmergencyCap.StartDate).Ticks / 2),
				Amount = relevantEmergencyCap.CapPerPerson,
				Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
				PurposeOfGrant = "blah blah blah",
				Id = rnd.Next(),
				Rate = null,
				Client = new Client()
				{
					Id = rnd.Next(),
					FirstName = "a",
					LastName = "b"
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
						Id = rnd.Next(),
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
									EmergencyCaps = new[] { relevantEmergencyCap }
								},
								AgencyGroup = new AgencyGroup()
								{
									Id = rnd.Next(),
									Currency = new Currency()
									{
										Id = rnd.Next(100, 999).ToString(),
										ExcRate = (decimal)rnd.NextDouble() * 10
									},
									Country = new Country()
									{
										Id = rnd.Next(),
										EmergencyCaps = new[] { relevantEmergencyCap }
									}
								}
							},

						}

					},
				}
			};



			//set the connection between the 
			relevantEmergencyCap.Funds.Add(relevantClientReport.SubReport.MainReport.AppBudget.App.Fund);
			relevantEmergencyCap.Countries.Add(relevantClientReport.SubReport.MainReport.AppBudget.App.AgencyGroup.Country);

			ClientReports = new List<ClientReport>() { relevantClientReport };

			EmergencyCaps = new List<EmergencyCap> { relevantEmergencyCap };


			relevantEmergencyCap.CapPerPerson = (relevantClientReport.Amount ?? 0)
				* relevantClientReport.SubReport.MainReport.ExcRate
				* relevantClientReport.SubReport.MainReport.AppBudget.App.Fund.Currency.ExcRate
			/ relevantEmergencyCap.Currency.ExcRate;

			Clients = new List<Client>() { relevantClientReport.Client };

			var q = Queries.EmergencyCapSummary(EmergencyCaps.AsQueryable(), Clients.AsQueryable(), ClientReports.AsQueryable(), relevantClientReport.SubReport.MainReportId);
			foreach (var item in q)
			{
				//check the currency conversion/aggregation calculations
				Assert.AreEqual((double)item.TotalAmount, (double)item.CapPerPerson, 10e-9, "Calculated amount should be equal to cap");
				Assert.AreEqual((double)item.TotalAmount, (double)item.CapPerPerson, 10e-9, "Calculated amount should be equal to cap");

				//the discretionary percentage should be preserved after currency conversion
				Assert.AreEqual(item.DiscretionaryPercentage, relevantClientReport.Discretionary / relevantClientReport.Amount);
                

				System.Diagnostics.Debug.WriteLine("{0} {1}", item.TotalAmount, item.CapPerPerson);
			}

			relevantEmergencyCap.CapPerPerson *= 2;
			Assert.IsFalse(q.Any(Queries.EmergencyCapValidationResult.CapPerPersonExceededExpression));


			relevantEmergencyCap.CapPerPerson /= 4;
			Assert.IsTrue(q.Any(Queries.EmergencyCapValidationResult.CapPerPersonExceededExpression));

			relevantClientReport.Discretionary = null;
			Assert.IsFalse(q.Any(Queries.EmergencyCapValidationResult.DiscretionaryExceededExpression));

			relevantClientReport.Discretionary = relevantClientReport.Amount * (relevantEmergencyCap.DiscretionaryPercentage / 2);
			Assert.IsFalse(q.Any(Queries.EmergencyCapValidationResult.DiscretionaryExceededExpression));

			relevantClientReport.Discretionary = relevantClientReport.Amount * (relevantEmergencyCap.DiscretionaryPercentage * 2);
			Assert.IsTrue(q.Any(Queries.EmergencyCapValidationResult.DiscretionaryExceededExpression));

            relevantEmergencyCap.StartDate = relevantEmergencyCap.EndDate;
            // 


		}
        /*
        public void GenericRun()
        {
            for (int j = 0; j < 100; j++)
            {
                var i = GenerateInput();
                IQueryable<Queries.EmergencyCapValidationResult> res,Exres = calcECapsForClient(i.ClientMID, i.cReports, i.caps, i.mainID).AsQueryable();
                res = Queries.EmergencyCapSummary(i.caps.AsQueryable(), i.clients.AsQueryable(), i.cReports.AsQueryable(), i.mainID);
                foreach (var item in res) // check that both methods returned same values
                    Assert.IsTrue(Exres.Contains<Queries.EmergencyCapValidationResult>(item));
                Assert.IsTrue(res.Count<Queries.EmergencyCapValidationResult>() == Exres.Count<Queries.EmergencyCapValidationResult>());
            }

        }
        private input GenerateInput() ///  TO DO
        {
            const int CLIENTS_PER_AGENCY=10;
            const int NUM_AGENCIES = 10;
            const int MAINREPS_PER_AGENCY = 2; // should be at least 2
            #region curr_country_fund
            Currency[] currencies = new Currency[NUM_AGENCIES / 2] {
                new Currency{
                        Id = "shekel",
                        ExcRate = (decimal)0.33M
                    },
                     new Currency{
                        Id = "usd",
                        ExcRate = (decimal)1M
                    },
                     new Currency{
                        Id = "rubl",
                        ExcRate = (decimal)0.15M
                    },
                     new Currency{
                        Id = "rupia",
                        ExcRate = (decimal)0.08M
                    },
                     new Currency{
                        Id = "yen",
                        ExcRate = (decimal)0.006M
                    }};
            Country[] countries = new Country[NUM_AGENCIES / 2] {
                new Country{
                    Id = 0,
                    Name = "Israel" ,
                },
                new Country{
                    Id = 1,
                    Name = "USA" ,
                },
                new Country{
                    Id = 2,
                    Name = "Russia" ,
                },
                new Country{
                    Id = 3,
                    Name = "India" ,
                },
                new Country{
                    Id = 4,
                    Name = "Japan" ,
                }
            };
            AgencyGroup[] AgencyGroups = new AgencyGroup[NUM_AGENCIES / 2];
            for(int i =0;i<NUM_AGENCIES / 2;i++)
            {
                AgencyGroups[i] = new AgencyGroup();
                AgencyGroups[i].Id = i;
                AgencyGroups[i].Currency = currencies[i];
                AgencyGroups[i].Country = countries[i];
            }
            Fund[] funds = new Fund[NUM_AGENCIES / 2] {
                new Fund{
                    Id = 0,
                    Name = "Fund0",
                    Currency = currencies[0]
                },
                new Fund{
                    Id = 1,
                    Name = "Fund1",
                    Currency = currencies[1]
                },
                new Fund{
                    Id = 2,
                    Name = "Fund2",
                    Currency = currencies[2]
                },
                new Fund{
                    Id = 3,
                    Name = "Fund3",
                    Currency = currencies[3]
                },
                new Fund{
                    Id = 4,
                    Name = "Fund4",
                    Currency = currencies[4]
                }
            };
            
            #endregion
            #region apps_budgets
            App[] Apps = new App[NUM_AGENCIES / 2] {
                new App{
                    Id = 0,
                    Name = "#APP1"
                },
                new App{
                    Id = 0,
                    Name = "#APP2"
                },
                new App{
                    Id = 0,
                    Name = "#APP3"
                },
                new App{
                    Id = 0,
                    Name = "#APP4"
                },
                new App{
                    Id = 0,
                    Name = "#APP5"
                }
            };
            for(int i = 0;i<NUM_AGENCIES / 2;i++)
            {
                Apps[i].AgencyGroup = AgencyGroups[i];
                AppBudget newOne = new AppBudget(){ Id = i , App = Apps[i] };
                Apps[i].AppBudgets.Add(newOne); // still need to add relevant mainreports to newOne
                Apps[i].Fund = funds[i];
            }
            #endregion


            #region Creation_of_clients
            Client[] Clients = new Client[CLIENTS_PER_AGENCY*NUM_AGENCIES];
            int?[] MasterID = new int?[CLIENTS_PER_AGENCY];
            int Target = -1;

            for (int i = 0; i < CLIENTS_PER_AGENCY * NUM_AGENCIES;i++ )  
            {
                int j=0;
                Clients[i] = new Client()
                {
                    Id = rnd.Next(),
                    FirstName = "John",
                    LastName = "Snow",
                    Country = countries[i/(CLIENTS_PER_AGENCY*2)]
                };
                // duplicate if needed
               
                if (i < CLIENTS_PER_AGENCY && rnd.Next(100) < 10) // with prob 10% there will be duplicate in all  agencies
                {
                    MasterID[i] = Clients[i].Id;
                    Target = Clients[i].Id;
                }
                if (i >= CLIENTS_PER_AGENCY && MasterID[i%CLIENTS_PER_AGENCY] != null)
                {  // with prob 50% make this client a duplicate in agency
                    Clients[i].MasterId = (rnd.Next(100) < 100) ? MasterID[i % CLIENTS_PER_AGENCY] : null;
                }
            }
            #endregion
            #region Creation_of_EC
            EmergencyCap[] caps = new EmergencyCap[NUM_AGENCIES];
            for (int i = 0; i < NUM_AGENCIES; i++)
            {
                DateTime start = ((i & 1) == 0) ? DateTime.Now.AddMonths(-MAINREPS_PER_AGENCY) : DateTime.Now.AddMonths(-MAINREPS_PER_AGENCY + 1);
                DateTime end = ((i & 1) == 0) ? DateTime.Now.AddMonths(-MAINREPS_PER_AGENCY+1) : DateTime.Now;
                caps[i] = new EmergencyCap()
                {
                    CapPerPerson = rnd.Next(100,300),
                    DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                    StartDate = start,
                    EndDate = end,
                    Currency = currencies[(i / 2)]
                };
                caps[i].Countries.Add(countries[i/2]);
                caps[i].Funds.Add(funds[i/2]);
            }
            #endregion
            #region Creation_of MR
            MainReport[] mainreports = new MainReport[MAINREPS_PER_AGENCY * NUM_AGENCIES];
            for (int i = 0; i < MAINREPS_PER_AGENCY * NUM_AGENCIES; i++)
            {
                int agencyID = (i/MAINREPS_PER_AGENCY);
                mainreports[i] = new MainReport()
                {
                    Id = i,
                    ExcRate = 1,//(decimal)rnd.NextDouble() * 10,
                    // Months of reports go from -MAINREPS_PER_AGENCY till now 1 month per report
                    Start = DateTime.Now.AddMonths(-MAINREPS_PER_AGENCY + i - agencyID * MAINREPS_PER_AGENCY),
                    End = DateTime.Now.AddMonths(-MAINREPS_PER_AGENCY + i - agencyID * MAINREPS_PER_AGENCY + 1),
                    AppBudget = Apps[i / (MAINREPS_PER_AGENCY * 2)].AppBudgets.First<AppBudget>(),
                    Status = MainReport.Statuses.Approved
                };
                // add to relevant AppBudget list
                mainreports[i].AppBudget.MainReports.Add(mainreports[i]); // could be obsolete
            }
            #endregion
            #region Client_Reports
            ClientReport[] ClientReports = new ClientReport[NUM_AGENCIES*CLIENTS_PER_AGENCY];
            for(int i =0;i<NUM_AGENCIES*CLIENTS_PER_AGENCY;i++)
            {
                int relIndex = ((i&1)==0)?(i/(CLIENTS_PER_AGENCY*2)):(i/(CLIENTS_PER_AGENCY*2)+1);
                EmergencyCap relevantEmergencyCap = caps[relIndex];
                int AgencyId = i/(CLIENTS_PER_AGENCY);
                int mainIndex = ((i&1)==0)?AgencyId:(AgencyId+1);
                ClientReports[i] = new ClientReport()
                {
                    ReportDate = relevantEmergencyCap.StartDate + new TimeSpan((relevantEmergencyCap.EndDate - relevantEmergencyCap.StartDate).Ticks / 2),
                    Amount = relevantEmergencyCap.CapPerPerson,
                    Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
                    PurposeOfGrant = "blah blah blah",
                    Id = rnd.Next(),
                    Rate = null,
                    Client = Clients[i],
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
                        MainReport = mainreports[mainIndex]
                    }
                };
            }
            #endregion

            int mid = (Target > 0 )?Target:Clients[rnd.Next(0,CLIENTS_PER_AGENCY * NUM_AGENCIES)].Id;

            input res = new input()
            {
                ClientMID = mid,
                cReports = ClientReports.ToList<ClientReport>(),
                clients = Clients.ToList<Client>(),
                caps = caps.ToList<EmergencyCap>(),
                mainID = ClientReports.First<ClientReport>(f => (f.Client.Id == mid)).SubReport.MainReport.Id
            };
            return res;              
        }
        private class input
        {
            public int ClientMID 
            {get;set;}
            public List<ClientReport> cReports 
            {get;set;}
            public List<Client> clients
            { get; set; }

            public List<EmergencyCap> caps 
            {get;set;}
            public int mainID 
            {get;set;}
            public input() { ClientMID = 0; cReports = new List<ClientReport>(); clients = new List<Client>(); caps = new List<EmergencyCap>(); mainID = 0; }
        }

        [TestMethod]
        public void Test1()
        {
            input i1;
            i1 = GenerateInput();
            Assert.IsTrue(i1.cReports.Any<ClientReport>(f => f.Client.Id == i1.ClientMID));
            ClientReport  cr = i1.cReports.Find(f => f.Client.Id == i1.ClientMID);
            Assert.IsTrue(cr.SubReport.MainReport.Id == i1.mainID);
            Assert.IsTrue(capIsRelevant(i1.caps.First<EmergencyCap>(), i1.cReports.First<ClientReport>()));
            return;
        }
        [TestMethod]
        public void Test2()
        {
            input i1;
            i1 = GenerateInput();
            var res1 = Queries.EmergencyCapSummary(i1.caps.AsQueryable(), i1.clients.AsQueryable(), i1.cReports.AsQueryable(), i1.mainID);
            var res11 = from f in res1 where f.ClientId == i1.ClientMID select f;
            var res2 = calcECapsForClient(i1.ClientMID, i1.cReports, i1.caps, i1.mainID);
            Assert.IsTrue(compResults(res1,res2.AsQueryable()));

        }
        private List<Queries.EmergencyCapValidationResult> calcECapsForClient(int clientMID, List<ClientReport> list_CR, List<EmergencyCap> caps, int mainID)
        {
            List<Queries.EmergencyCapValidationResult> res = new List<Queries.EmergencyCapValidationResult>();
            foreach (ClientReport cr in list_CR)
            {
                if (cr.SubReport.MainReport.Id == mainID || (cr.SubReport.MainReport.Status == MainReport.Statuses.Approved || cr.SubReport.MainReport.Status == MainReport.Statuses.AwaitingProgramOfficerApproval))
                {
                    
                    foreach (EmergencyCap cap in caps)
                    {
                        if (capIsRelevant(cap, cr))
                        {
                            if (cr.Client.Id == clientMID || cr.Client.MasterId == clientMID)
                            {
                                Queries.EmergencyCapValidationResult inter = new Queries.EmergencyCapValidationResult();
                                inter.TotalAmount += (cr.Amount ?? 0);
                                inter.ClientFirstName = cr.Client.FirstName;
                                inter.ClientId = cr.Client.Id;
                                inter.ClientLastName = cr.Client.LastName;
                                inter.CapPerPerson = cap.CapPerPerson * cap.Currency.ExcRate;
                                res.Add(inter);
                            }
                        }
                    }
                }
            }
            return res;
        }

        private bool compResults(IQueryable<Queries.EmergencyCapValidationResult> a,IQueryable<Queries.EmergencyCapValidationResult> b)
        {
            bool res = true;
            foreach (var item in a)
            {
                if (b.Contains<Queries.EmergencyCapValidationResult>(item) == false)
                {
                    res = false;
                    break;
                }
            }
            return res;
        }
        private bool capIsRelevant(EmergencyCap cap, ClientReport cr)
        {
            bool res = false;
            foreach (Fund f in cap.Funds) // check if Fund is relevant
            {
                if (cr.SubReport.MainReport.AppBudget.App.Fund == f)
                {
                    res = true;
                    break;
                }
                if (res == false)
                    return false;
            }
            DateTime startTime = cap.StartDate.AddMinutes(-1);
            DateTime endTime = cap.EndDate.AddMinutes(1);
            if (cr.SubReport.MainReport.Start >= startTime && cr.SubReport.MainReport.End <= endTime)
                return true;
            return false;
        }
        */
        [TestMethod]
        public void DuplicateClientCheck()
        {
            #region input_setup
            Client C1 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "John",
                LastName = "Snow",
                Country = new Country() { Id = 0 , Name = "Israel"} 
            };
            Client C2 = new Client() // duplicate client
            {
                Id = rnd.Next(),
                MasterId = C1.Id,
                FirstName = "John",
                LastName = "Snow",
                Country = C1.Country
            };
            Client C3 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "Angela",
                LastName = "Merkel",
                Country = new Country() { Id = 2, Name = "Germany" }
            };
            EmergencyCap capFund0Israel = new EmergencyCap()
            {
                Id = 0,
                CapPerPerson = 100,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "shekel",
                    ExcRate = (decimal)0.33M
                }
            };
            EmergencyCap capFund1Israel = new EmergencyCap()
            {
                Id = 1,
                CapPerPerson = 300,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = capFund0Israel.Currency
            };
            EmergencyCap capFund2Germany = new EmergencyCap()
            {
                Id = 2,
                CapPerPerson = 200,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "euro",
                    ExcRate = (decimal)1.15M
                }
            };
            EmergencyCap[] caps = new EmergencyCap[3] {capFund0Israel , capFund1Israel , capFund2Germany };
            // link country to emergencyCap
            capFund0Israel.Countries.Add(C1.Country);
            capFund1Israel.Countries.Add(C2.Country);
            capFund2Germany.Countries.Add(C3.Country);
            AgencyGroup agencyGroup = new AgencyGroup(){ Id = 0, Currency = capFund0Israel.Currency , Country = C1.Country };
            Fund[] funds = new Fund[3] {
                new Fund{
                    Id = 0,
                    Name = "Fund0",
                    Currency = capFund0Israel.Currency
                },
                new Fund{
                    Id = 0,
                    Name = "Fund1",
                    Currency = capFund1Israel.Currency
                },
                new Fund{
                    Id = 1,
                    Name = "Fund1",
                    Currency = capFund2Germany.Currency
                }};
            capFund0Israel.Funds.Add(funds[0]);
            capFund1Israel.Funds.Add(funds[1]);
            capFund2Germany.Funds.Add(funds[2]);
            App[] Apps = new App[3] {
                new App{
                    Id = 0,
                    Name = "#APP1"
                },
                new App{
                    Id = 1,
                    Name = "#APP2"
                },
                new App{
                    Id = 2,
                    Name = "#APP3"
                }
            };
            for (int i = 0; i < 3; i++)
            {
                Apps[i].AgencyGroup = agencyGroup;
                AppBudget newOne = new AppBudget() { Id = i, App = Apps[i] };
                Apps[i].AppBudgets.Add(newOne); // still need to add relevant mainreports to newOne
                Apps[i].Fund = funds[i];
            }
            MainReport[] mainreports = new MainReport[3];
            for (int i = 0; i < 3; i++)
            {
                mainreports[i] = new MainReport()
                {
                    Id = i,
                    ExcRate = 1,//(decimal)rnd.NextDouble() * 10,
                    Start = caps[i].StartDate.AddMinutes(1),
                    End = caps[i].EndDate.AddMinutes(-1),
                    AppBudget = Apps[i].AppBudgets.First<AppBudget>(),
                    Status = MainReport.Statuses.Approved
                };
                // add to relevant AppBudget list
                mainreports[i].AppBudget.MainReports.Add(mainreports[i]); // could be obsolete
            }
            Client[] clients = new Client[] { C1, C2, C3 };
            SubReport[] subreports = new SubReport[3];
            for (int i = 0; i < 3; i++)
                subreports[i] = new SubReport()
                    {   MainReportId = i, 
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
                        MainReport = mainreports[i ]
                    };
            ClientReport[] ClientReports = new ClientReport[7];
            for (int i = 0; i < 6; i++)
            {
                EmergencyCap relevantEmergencyCap = caps[i/2];
                ClientReports[i] = new ClientReport()
                {
                    ReportDate = relevantEmergencyCap.StartDate.AddDays(i % 2),
                    Amount = relevantEmergencyCap.CapPerPerson/2,
                    Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
                    PurposeOfGrant = "blah blah blah",
                    Id = rnd.Next(),
                    Rate = null,
                    Client = clients[i/2],
                    SubReport = subreports[i/2]
                };
            }
            ClientReports[6] = new ClientReport()  // this report should be ignored
            {
                ReportDate = capFund0Israel.EndDate.AddDays(2),
                Amount = capFund0Israel.CapPerPerson / 2,
                Discretionary = capFund0Israel.CapPerPerson * capFund0Israel.DiscretionaryPercentage,
                PurposeOfGrant = "blah blah blah",
                Id = rnd.Next(),
                Rate = null,
                Client = clients[0],
                SubReport = subreports[0]
            };
            
            #endregion
            #region testing
            var q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 0);
            //Assert.IsTrue(q.Any(f=> Math.Abs((double)f.TotalAmount - (double)(300*0.33+100*0.33)) < 10e-9)); // test fails here
  
            Assert.IsFalse(q.Any(f => Math.Abs((double)f.TotalAmount - (double)(200 * 1.15)) < 10e-9));
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 1);
            Assert.IsTrue(q.Any(f => Math.Abs((double)f.TotalAmount - (double)(300 * 0.33 + 100 * 0.33)) < 10e-9));
            Assert.IsTrue(q.Count() == 2);
            Assert.IsFalse(q.Any(f => Math.Abs((double)f.TotalAmount - (double)(200 * 1.15)) < 10e-9));
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 2);
            Assert.IsFalse(q.Any(f => Math.Abs((double)f.TotalAmount - (double)(300 * 0.33 + 100 * 0.33)) < 10e-9));
            Assert.IsTrue(q.Count() == 1);
            Assert.IsTrue(q.Any(f => Math.Abs((double)f.TotalAmount - (double)(200 * 1.15)) < 10e-9));

            #endregion


        }
        [TestMethod]
        public void ECEdgeDate()
        {
            #region input_setup
            Client C1 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "John",
                LastName = "Snow",
                Country = new Country() { Id = 0, Name = "Israel" }
            };
            Client C2 = new Client() // duplicate client
            {
                Id = rnd.Next(),
                FirstName = "John",
                LastName = "Snow",
                Country = new Country() { Id = 1, Name = "Russia" }
            };
            Client C3 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "Angela",
                LastName = "Merkel",
                Country = new Country() { Id = 2, Name = "Germany" }
            };
            EmergencyCap capIsrael = new EmergencyCap()
            {
                Id = 0,
                CapPerPerson = 100,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "shekel",
                    ExcRate = (decimal)0.33M
                }
            };
            EmergencyCap capRussia = new EmergencyCap()
            {
                Id = 1,
                CapPerPerson = 300,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "rubl",
                    ExcRate = (decimal)0.15M
                }
            };
            EmergencyCap capGermany = new EmergencyCap()
            {
                Id = 2,
                CapPerPerson = 200,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "euro",
                    ExcRate = (decimal)1.15M
                }
            };
            EmergencyCap[] caps = new EmergencyCap[3] { capIsrael, capRussia, capGermany };
            // link country to emergencyCap
            capIsrael.Countries.Add(C1.Country);
            capRussia.Countries.Add(C2.Country);
            capGermany.Countries.Add(C3.Country);
            AgencyGroup[] AgencyGroups = new AgencyGroup[3];
            for (int i = 0; i < 3; i++)
            {
                AgencyGroups[i] = new AgencyGroup();
                AgencyGroups[i].Id = i;
                AgencyGroups[i].Currency = caps[i].Currency;
                AgencyGroups[i].Country = caps[i].Countries.First<Country>();
            }
            Fund[] funds = new Fund[3] {
                new Fund{
                    Id = 0,
                    Name = "Fund0",
                    Currency = capIsrael.Currency
                },
                new Fund{
                    Id = 0,
                    Name = "Fund1",
                    Currency = capRussia.Currency
                },
                new Fund{
                    Id = 1,
                    Name = "Fund2",
                    Currency = capGermany.Currency
                }};
            capIsrael.Funds.Add(funds[0]);
            capRussia.Funds.Add(funds[1]);
            capGermany.Funds.Add(funds[2]);
            App[] Apps = new App[3] {
                new App{
                    Id = 0,
                    Name = "#APP1"
                },
                new App{
                    Id = 1,
                    Name = "#APP2"
                },
                new App{
                    Id = 2,
                    Name = "#APP2"
                }
            };
            for (int i = 0; i < 3; i++)
            {
                Apps[i].AgencyGroup = AgencyGroups[i];
                AppBudget newOne = new AppBudget() { Id = i, App = Apps[i] };
                Apps[i].AppBudgets.Add(newOne); // still need to add relevant mainreports to newOne
                Apps[i].Fund = funds[i];
            }

            MainReport[] mainreports = new MainReport[3];
            for (int i = 0; i < 3; i++)
            {
                mainreports[i] = new MainReport()
                {
                    Id = i,
                    ExcRate = 1,//(decimal)rnd.NextDouble() * 10,
                    Start = caps[i].StartDate.AddMinutes(1),
                    End = caps[i].EndDate.AddMinutes(-1),
                    AppBudget = Apps[i].AppBudgets.First<AppBudget>(),
                    Status = MainReport.Statuses.Approved
                };
                // add to relevant AppBudget list
                //mainreports[i].AppBudget.MainReports.Add(mainreports[i]); // could be obsolete
            }
            Client[] clients = new Client[] { C1, C2, C3 };
            SubReport[] subreports = new SubReport[3];
            for (int i = 0; i < 3; i++)
                subreports[i] = new SubReport()
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
                    MainReport = mainreports[i]
                };
            ClientReport[] ClientReports = new ClientReport[6];
            for (int i = 0; i < 6; i++)
            {
                EmergencyCap relevantEmergencyCap = caps[i / 2];
                ClientReports[i] = new ClientReport()
                { // check start/end dates of EC
                    ReportDate = ((i % 1) == 0) ? relevantEmergencyCap.StartDate : relevantEmergencyCap.EndDate,
                    Amount = relevantEmergencyCap.CapPerPerson / 2,
                    Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
                    PurposeOfGrant = "blah blah blah",
                    Id = rnd.Next(),
                    Rate = null,
                    Client = clients[i / 2],
                    SubReport = subreports[i / 2]
                };
                subreports[i / 2].ClientReports.Add(ClientReports[i]);
            }

            // link funds in reports to funds in caps
            #endregion
            #region testing
            var q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 0);
            Assert.IsTrue(q.Count() == 1); //  1 result for 2 reports of different dates ?
            Assert.IsTrue(q.Any(f =>f.ClientId == C1.Id));
            Assert.AreEqual((double)(100 * 0.33), (double)q.First().TotalAmount, 10e-9, "should be equal"); // test fails here
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 1);
            Assert.IsTrue(q.Count() == 1);
            Assert.IsTrue(q.Any(f => f.ClientId == C2.Id));
            Assert.AreEqual((double)(300 * 0.15), (double)q.First().TotalAmount, 10e-9, "should be equal");
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 2);
            Assert.IsTrue(q.Count() == 1);
            Assert.IsTrue(q.Any(f => f.ClientId == C3.Id));
            Assert.AreEqual((double)(200 * 1.15), (double)q.First().TotalAmount, 10e-9, "should be equal");

            #endregion
        }
        [TestMethod]
        public void basic()
        {
            #region input_setup
            Client C1 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "John",
                LastName = "Snow",
                Country = new Country() { Id = 0, Name = "Israel" }
            };
            Client C2 = new Client() 
            {
                Id = rnd.Next(),
                FirstName = "John",
                LastName = "Snow",
                Country = new Country() { Id = 1, Name = "Russia" }
            };
            Client C3 = new Client()
            {
                Id = rnd.Next(),
                FirstName = "Angela",
                LastName = "Merkel",
                Country = new Country() { Id = 2, Name = "Germany" }
            };
            EmergencyCap capIsrael = new EmergencyCap()
            {
                Id = 0,
                CapPerPerson = 100,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "shekel",
                    ExcRate = (decimal)0.33M
                }
            };
            EmergencyCap capRussia = new EmergencyCap()
            {
                Id = 1,
                CapPerPerson = 300,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "rubl",
                    ExcRate = (decimal)0.15M
                }
            };
            EmergencyCap capGermany = new EmergencyCap()
            {
                Id = 2,
                CapPerPerson = 200,
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now.AddMonths(1),
                Currency = new Currency()
                {
                    Id = "euro",
                    ExcRate = (decimal)1.15M
                }
            };
            EmergencyCap[] caps = new EmergencyCap[3] { capIsrael, capRussia, capGermany };
            // link country to emergencyCap
            capIsrael.Countries.Add(C1.Country);
            capRussia.Countries.Add(C2.Country);
            capGermany.Countries.Add(C3.Country);
            AgencyGroup[] AgencyGroups = new AgencyGroup[3];
            for (int i = 0; i < 3; i++)
            {
                AgencyGroups[i] = new AgencyGroup();
                AgencyGroups[i].Id = i;
                AgencyGroups[i].Currency = caps[i].Currency;
                AgencyGroups[i].Country = caps[i].Countries.First<Country>();
            }
            Fund[] funds = new Fund[3] {
                new Fund{
                    Id = 0,
                    Name = "Fund0",
                    Currency = capIsrael.Currency
                },
                new Fund{
                    Id = 1,
                    Name = "Fund1",
                    Currency = capRussia.Currency
                },
                new Fund{
                    Id = 2,
                    Name = "Fund2",
                    Currency = capGermany.Currency
                }};
            capIsrael.Funds.Add(funds[0]);
            capRussia.Funds.Add(funds[1]);
            capGermany.Funds.Add(funds[2]);
            App[] Apps = new App[3] {
                new App{
                    Id = 0,
                    Name = "#APP1"
                },
                new App{
                    Id = 1,
                    Name = "#APP2"
                },
                new App{
                    Id = 2,
                    Name = "#APP2"
                }
            };
            for (int i = 0; i < 3; i++)
            {
                Apps[i].AgencyGroup = AgencyGroups[i];
                AppBudget newOne = new AppBudget() { Id = i, App = Apps[i] };
                Apps[i].AppBudgets.Add(newOne); // still need to add relevant mainreports to newOne
                Apps[i].Fund = funds[i];
            }

            MainReport[] mainreports = new MainReport[3];
            for (int i = 0; i < 3; i++)
            {
                mainreports[i] = new MainReport()
                {
                    Id = i,
                    ExcRate = 1,//(decimal)rnd.NextDouble() * 10,
                    Start = caps[i].StartDate.AddMinutes(1),
                    End = caps[i].EndDate.AddMinutes(-1),
                    AppBudget = Apps[i].AppBudgets.First<AppBudget>(),
                    Status = MainReport.Statuses.Approved
                };
                // add to relevant AppBudget list
                //mainreports[i].AppBudget.MainReports.Add(mainreports[i]); // could be obsolete
            }
            Client[] clients = new Client[] { C1, C2, C3 };
            SubReport[] subreports = new SubReport[3];
            for (int i = 0; i < 3; i++)
                subreports[i] = new SubReport()
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
                    MainReport = mainreports[i]
                };
            ClientReport[] ClientReports = new ClientReport[3];
            for (int i = 0; i < 3; i++)
            {
                EmergencyCap relevantEmergencyCap = caps[i];
                ClientReports[i] = new ClientReport()
                { // check start/end dates of EC
                    ReportDate = relevantEmergencyCap.StartDate ,
                    Amount = relevantEmergencyCap.CapPerPerson,
                    Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
                    PurposeOfGrant = "blah blah blah",
                    Id = rnd.Next(),
                    Rate = null,
                    Client = clients[i ],
                    SubReport = subreports[i ]
                };
                subreports[i].ClientReports.Add(ClientReports[i]);
            }

            // link funds in reports to funds in caps
            #endregion
            #region testing
            var q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 0);
            Assert.IsTrue(q.Count() == 1);
            Assert.AreEqual((double)(100 * 0.33), (double)q.First().TotalAmount, 10e-9, "should be equal");
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 1);
            Assert.IsTrue(q.Count() == 1);
            Assert.AreEqual((double)(300 * 0.15), (double)q.First().TotalAmount, 10e-9, "should be equal");
            q = Queries.EmergencyCapSummary(caps.AsQueryable(), clients.AsQueryable(), ClientReports.AsQueryable(), 2);
            Assert.IsTrue(q.Count() == 1);
            Assert.AreEqual((double)(200 * 1.15), (double)q.First().TotalAmount, 10e-9, "should be equal");
            #endregion
        }
        // assume relevantEmergencyCap and Fund use same currency
        private ClientReport createClientReport(Client c,EmergencyCap relevantEmergencyCap,Country country,int mainID)
        {
            return
                new ClientReport()
                {
                    ReportDate = relevantEmergencyCap.StartDate + new TimeSpan((relevantEmergencyCap.EndDate - relevantEmergencyCap.StartDate).Ticks / 2),
                    Amount = relevantEmergencyCap.CapPerPerson,
                    Discretionary = relevantEmergencyCap.CapPerPerson * relevantEmergencyCap.DiscretionaryPercentage,
                    PurposeOfGrant = "blah blah blah",
                    Id = rnd.Next(),
                    Rate = null,
                    Client = c,
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
                            Id = mainID,
                            ExcRate = 1,//(decimal)rnd.NextDouble() * 10,
                            AppBudget = new AppBudget()
                            {
                                Id = rnd.Next(),
                                App = new App()
                                {
                                    Id = rnd.Next(),
                                    Fund = new Fund()
                                    {
                                        Id = rnd.Next(),
                                        Currency = relevantEmergencyCap.Currency,
                                        EmergencyCaps = new[] { relevantEmergencyCap }
                                    },
                                    
                                    AgencyGroup = new AgencyGroup()
                                    {
                                        Id = rnd.Next(),
                                        Currency = new Currency()
                                        {
                                            Id = rnd.Next(100, 999).ToString(),
                                            ExcRate = 1//(decimal)rnd.NextDouble() * 10
                                        },
                                        Country = country
                                    }
                                },

                            }

                        },
                    }
                };
        }
        #region additional_tests

        [TestMethod]
        public void Currency_change()
        {
            var relevantEmergencyCap = new EmergencyCap()
            {
                CapPerPerson = 100,//rnd.Next(),
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now,
                Currency = new Currency()
                {
                    Id ="100",
                    ExcRate = (decimal)rnd.NextDouble() * 10
                     //rnd.Next(100, 999).ToString(),
                }
            };
            // add relevantEmergencyCap to Currency's list
            relevantEmergencyCap.Currency.EmergencyCaps.Add(relevantEmergencyCap);
            Currency old_Currency = relevantEmergencyCap.Currency;
            Currency new_Currency = new Currency()
            {
                Id = "200",
                ExcRate = (decimal)rnd.NextDouble() * 10
            };
            relevantEmergencyCap.Currency = new_Currency;
            
            Assert.IsFalse(old_Currency.EmergencyCaps.Contains(relevantEmergencyCap));
            Assert.IsTrue(new_Currency.EmergencyCaps.Contains(relevantEmergencyCap));

        }
        [TestMethod]
        public void Countries_check()
        {
            var relevantEmergencyCap = new EmergencyCap()
            {
                CapPerPerson = 100,//rnd.Next(),
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now,
                Currency = new Currency()
                {
                    Id = "100",
                    ExcRate = (decimal)rnd.NextDouble() * 10
                    //rnd.Next(100, 999).ToString(),
                }
            };
            var new_country = new Country()
            {
                Id = 100,Code = "100", Name = "Israel" 
            };
            
            Assert.IsTrue(relevantEmergencyCap.Countries.Count == 0);
            relevantEmergencyCap.Countries.Add(new_country); // adding new country
            Assert.IsTrue(relevantEmergencyCap.Countries.Count == 1);
            relevantEmergencyCap.Countries.Add(new_country); // trying to insert same country twice
            Assert.IsTrue(relevantEmergencyCap.Countries.Count == 1);
            Assert.IsTrue(new_country.EmergencyCaps.Contains(relevantEmergencyCap)); // new country update check

            var new_country2 = new Country()
            {
                Id = 101,
                Code = "101",
                Name = "Russia"
            };

            relevantEmergencyCap.Countries.Add(new_country2);
            Assert.IsTrue(new_country2.EmergencyCaps.Contains(relevantEmergencyCap));
            Assert.IsTrue(relevantEmergencyCap.Countries.Count == 2);
            relevantEmergencyCap.Countries.Clear();

            Assert.IsFalse(new_country2.EmergencyCaps.Contains(relevantEmergencyCap));
            Assert.IsFalse(new_country.EmergencyCaps.Contains(relevantEmergencyCap));
        }
        [TestMethod]
        public void Funds_check()
        {
            var relevantEmergencyCap = new EmergencyCap()
            {
                CapPerPerson = 100,//rnd.Next(),
                DiscretionaryPercentage = 0.5M,// (decimal)rnd.NextDouble(),
                StartDate = DateTime.Now.AddMonths(-3),
                EndDate = DateTime.Now,
                Currency = new Currency()
                {
                    Id = "100",
                    ExcRate = (decimal)rnd.NextDouble() * 10
                    //rnd.Next(100, 999).ToString(),
                }
            };
            var new_fund = new Fund()
            {
                Id = 100,
                Name = "Checkup outdated fund",
                StartDate = DateTime.Today.AddDays(-3),
                EndDate = DateTime.Today.AddDays(-1) // outdated fund
            };
            var new_fund2 = new Fund()
            {
                Id = 100,
                Name = "Checkup updated fund",
                StartDate = DateTime.Today.AddDays(-3),
                EndDate = DateTime.Today.AddDays(1) // updated fund
            };
            var new_fund3 = new Fund()
            {
                Id = 100,
                Name = "Checkup updated fund",
                StartDate = DateTime.Today.AddDays(-3),
                EndDate = DateTime.Today.AddDays(1) // updated fund
            };
            var new_fund4 = new Fund()
            {
                Id = 101,
                Name = "Checkup updated fund",
                StartDate = DateTime.Today.AddDays(-3),
                EndDate = DateTime.Today.AddDays(1) // updated fund
            };
            Assert.IsTrue(relevantEmergencyCap.Funds.Count == 0);
            relevantEmergencyCap.Funds.Add(new_fund);
            Assert.IsTrue(relevantEmergencyCap.Funds.Count == 1); // outdated fund added - no checking
            Assert.IsTrue(new_fund.EmergencyCaps.Contains(relevantEmergencyCap));
            relevantEmergencyCap.Funds.Add(new_fund2);
            relevantEmergencyCap.Funds.Add(new_fund3);
            Assert.IsTrue(relevantEmergencyCap.Funds.Count == 3);  // new fund with same Id added - no checking
            relevantEmergencyCap.Funds.Add(new_fund4);
            Assert.IsTrue(new_fund2.EmergencyCaps.Contains(relevantEmergencyCap));
            relevantEmergencyCap.Funds.Remove(new_fund);
            Assert.IsFalse(new_fund.EmergencyCaps.Contains(relevantEmergencyCap));
            Assert.IsTrue(new_fund4.EmergencyCaps.Contains(relevantEmergencyCap));
            relevantEmergencyCap.Funds.Remove(new_fund4);
            Assert.IsFalse(new_fund4.EmergencyCaps.Contains(relevantEmergencyCap));
        }
        #endregion
    }
}
