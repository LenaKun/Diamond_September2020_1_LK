using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CC.Data;

namespace CC.Data.Tests
{
    public static class Helper
    {


        public static User GetUser(FixedRoles role, string AgencyName = "")
        {

            var context = new ccEntities();
            User newUser;
            string userName = "test_new" + role.ToString();
            try
            {
                newUser = context.Users.First(c => c.UserName == userName);
            }
            catch
            {

                
              
                newUser = User.CreateUser(userName, userName);
                if (AgencyName == "") AgencyName = "Agency1_FirstTest";
                Agency a1 = context.Agencies.First(c => c.Name == AgencyName);
                newUser.AgencyId = a1.Id;
                newUser.RoleId = (int)role;
                newUser.Email = userName;
             //   newUser.RegionId = a1.RegionId;
                newUser.AgencyGroupId = a1.AgencyGroup.Id;
                newUser.MembershipUser.SetPassword(userName);
                context.Users.AddObject(newUser);
                context.MembershipUsers.AddObject(newUser.MembershipUser);

                context.SaveChanges();

            }

            return newUser;
        }



        public static User GetSerUser(string GroupName)
        {

            var context = new ccEntities();
            User newUser=null;
            int ag_id = 0;
            var g1=context.AgencyGroups.Where(g=>g.Name==GroupName);
            if (g1.Any())
            {
                ag_id = g1.First().Id;
                string userName = "ser_" + ag_id;
                FixedRoles role = FixedRoles.Ser;

                var us = context.Users.Where(u => u.UserName == userName);

                if (!us.Any())
                {
                    newUser = User.CreateUser(userName, userName);

                    newUser.RegionId = g1.First().Country.RegionId;
                    newUser.RoleId = (int)role;
                    newUser.Email = userName;
                    newUser.AgencyGroupId = ag_id;
                    newUser.MembershipUser.SetPassword(userName);
                    context.Users.AddObject(newUser);
                    context.MembershipUsers.AddObject(newUser.MembershipUser);

                    context.SaveChanges();
                }
                else
                {

                    newUser = us.First();
                }
            }

            return newUser;
        }




        public static List<int> GetTestClientIdsList()
        {

            ccEntities context = new ccEntities();
            Client cl1 = context.Clients.First(x => x.FirstName == "Client1");
            Client cl2 = context.Clients.First(x => x.FirstName == "Client2");
            Client cl3 = context.Clients.First(x => x.FirstName == "Client3");
            List<int> clList = new List<int>() { cl1.Id, cl2.Id, cl3.Id };
            return clList;

        }

        public static Client GetClient(string clName, bool defValue = false)
        {
            ccEntities context = new ccEntities();
            Client cl1 = context.Clients.First(x => x.FirstName == clName);
            if (defValue)
            //set properties to default values before changes
            {
                cl1.GfHours = 0;

                context.SaveChanges();

            }
            return cl1;
        }


        public static List<Client> GetTestClientsList()
        {

            ccEntities context = new ccEntities();
            Client cl1 = context.Clients.First(x => x.FirstName == "Client1");
            Client cl2 = context.Clients.First(x => x.FirstName == "Client2");
            Client cl3 = context.Clients.First(x => x.FirstName == "Client3");
            List<Client> clList = new List<Client>() { cl1, cl2, cl3 };
            return clList;

        }



        public static User GetAdminUser()
        {
            return GetUser(FixedRoles.Admin, "Agency1_FirstTest");



        }


        public static User GetAgencyUser(string AgencyName)
        {
            return GetUser(FixedRoles.AgencyUser, AgencyName);
        }


        public static User GetRegionalUser(string AgencyName)
        {
            return GetUser(FixedRoles.RegionOfficer, AgencyName);
        }



        public static void PrepareTestData()
        {
            AddTestRegions();
            AddCurrencies();
            AddCountryStateRegions();
            AddServices();
            AddTestAgenciesGroups();
            AddAgencyGroupServices();
            AddTestAgencies();
            AddTestClients();
            AddFunds();
            AddAppServices();

        }

        public static void AddTestRegions()
        {
            CreateRegion("First");
            CreateRegion("Second");

        }


        public static void CreateRegion(string regionName)
        {

            Region r = new Region();

            r.Name = regionName;
            var context = new ccEntities();
            Region r1;
            try
            {
                r1 = context.Regions.First(x => x.Name == regionName);
            }
            catch
            {
                context.Regions.AddObject(r);
                context.SaveChanges();
            }

        }

        public static void CreateClient(string FirstName, string LastName, string AgencyName)
        {




            var context = new ccEntities();
            Client c1 = new Client();
            int UpdateById = context.Users.First().Id;
            

            c1.Agency = context.Agencies.First(c => c.Name == AgencyName);
            
            
            c1.FirstName = FirstName;
            c1.LastName = LastName;
            c1.UpdatedById = UpdateById;
            c1.CountryId = c1.Agency.AgencyGroup.CountryId;
            c1.StateId = c1.Agency.AgencyGroup.StateId;
            c1.JoinDate = DateTime.Now;
            
            try
            {
                var client = context.Clients.First(x => x.FirstName == FirstName);
            }
            catch
            {
                context.Clients.AddObject(c1);
                context.SaveChanges();
            }



        }

        public static void SetClientData(string FirstName,DateTime leaveDate, decimal exHours, decimal gfHours)
        {
            ccEntities context = new ccEntities();
            var client = context.Clients.First(x => x.FirstName == FirstName);
            
                client.LeaveDate = leaveDate;
                client.ExceptionalHours = exHours;
                client.GfHours = gfHours;
                context.SaveChanges();
            
        }

        public static void AddFuncScore(string FirstName, int dScore, FunctionalityLevel fl)
        {
            ccEntities context = new ccEntities();
            var client = context.Clients.First(x => x.FirstName == FirstName);
            FunctionalityScore fs = new FunctionalityScore();
            fs.ClientId = client.Id;
            fs.DiagnosticScore = dScore;
            fs.FunctionalityLevel = fl;
            fs.UpdatedAt = DateTime.Now;
            fs.UpdatedBy = GetAdminUser().Id;
            context.SaveChanges();
            


        }



        public static void AddFuncScore(string FirstName, int dScore, int fl)
        {
            ccEntities context = new ccEntities();
            var client = context.Clients.First(x => x.FirstName == FirstName);
            FunctionalityScore fs = new FunctionalityScore();
            fs.ClientId = client.Id;
            fs.DiagnosticScore = dScore;
            fs.FunctionalityLevelId = fl;
            fs.UpdatedAt = DateTime.Now;
            fs.UpdatedBy = GetAdminUser().Id;
            context.FunctionalityScores.AddObject(fs);
            context.SaveChanges();



        }


        public static void AddHomeCarePeriod(string FirstName, DateTime from, DateTime to)
        {
            ccEntities context = new ccEntities();
            var client = context.Clients.First(x => x.FirstName == FirstName);
            HomeCareEntitledPeriod hc = new HomeCareEntitledPeriod();
            hc.ClientId = client.Id;
            hc.StartDate = from;
            hc.EndDate = to;
            hc.UpdatedAt = DateTime.Now;
            hc.UpdatedBy = GetAdminUser().Id;
            context.HomeCareEntitledPeriods.AddObject(hc);
            context.SaveChanges();

        }



        public static void AddTestClients()
        {
            DateTime from = DateTime.Parse("01-01-2012");
            DateTime to = DateTime.Parse("01-01-2013");

            CreateClient("Client1", "Moshe", "Agency1_FirstTest");
            CreateClient("Client2", "Alon", "Agency2_SecondTest");
            CreateClient("Client3", "Tamar", "Agency3_FirstTest");
            
            //only one time

            //AddFuncScore("Client1",5 ,26);
            //AddFuncScore("Client2", 15, 27);
            //AddFuncScore("Client3", 4, 25);


            //AddHomeCarePeriod("Client1", from, to);
            //AddHomeCarePeriod("Client2", from, to);
            //AddHomeCarePeriod("Client3", from, to);
        
        }



        public static void CreateAgency(string agencyName, string groupName)
        {
            var context = new ccEntities();
          
          
            int gr_id = 0;
           

             var gr = context.AgencyGroups.Where(x => x.Name == groupName);
             if (gr.Any()) gr_id = gr.First().Id;


             var ag = context.Agencies.Where(x => x.Name == agencyName);
             if (!ag.Any())
             {
                 Agency a = new Agency();
                 a.Name = agencyName;
                 a.GroupId = gr_id;
                 context.Agencies.AddObject(a);
                 try
                 {
                     context.SaveChanges();
                 }
                 catch (Exception ex)
                 {

                 }
             }
            
            
                 
                
           




        }


        public static App GetApp(string AppName)
        {
            ccEntities context = new ccEntities();
            var g1 = context.Apps.Where(g => g.Name == AppName);
            if (g1.Any())
                return g1.First();
            else
                return null;

        }


        public static AgencyGroup GetAgencyGroup(string GroupName)
        {
            ccEntities context = new ccEntities();
            var g1 = context.AgencyGroups.Where(g => g.Name == GroupName);
            if (g1.Any())
                return g1.First();
            else
                return null;

        }

        public static void CreateAgencyGroup(string GroupName, string CountryName, string StateName)
        {
            var context = new ccEntities();
            AgencyGroup a = new AgencyGroup();
            a.Name = GroupName;
            var ct=context.Countries.Where(x => x.Name == CountryName);
            var st = context.States.Where(x => x.Name == StateName);


            int c_id=0, st_id = 0;

            if (ct.Any())  c_id=ct.First().Id;
            if (st.Any()) st_id = st.First().Id;
          

            var g=context.AgencyGroups.Where(c => c.Name == a.Name);
            if (!g.Any())
            {

                AgencyGroup g_new=new AgencyGroup();
                g_new.Name = GroupName;
                g_new.Addr1= GroupName+ "Test Addr1" ;
                g_new.Addr2=GroupName+" Test Addr2";
                g_new.CountryId=c_id;
                if (st_id!=0) g_new.StateId=st_id;
                g_new.ReportingPeriodId=1;
                context.AgencyGroups.AddObject(g_new);
                try
                {
                    context.SaveChanges();
                }
                catch (Exception ex)
                {

                }
            }
            

         
  
               
            

       }

        public static void CreateCountryState(string regName, string CountryName, string CountryCode, string StateName, string StateCode)
       {

           ccEntities context=new ccEntities();
           var ct = context.Countries.Where(x => x.Name == CountryName && x.Region.Name==regName);
           var st = context.States.Where(x => x.Name == StateName);
           var reg = context.Regions.Where(x => x.Name == regName);

            int ct_id=0, st_id=0, reg_id=0;
           if (reg.Any()) //must be - create before
           {
               reg_id = reg.First().Id;
           
           }

           if (!ct.Any())
           {
               Country c = new Country();
               c.Name = CountryName;
               c.Code = CountryCode;
               c.RegionId = reg_id;
               c.IncomeVerificationRequired = true;
               context.Countries.AddObject(c);
               try
               {
                   context.SaveChanges();
               }
               catch (Exception ex)
               {

               }

               ct = context.Countries.Where(x => x.Name == CountryName && x.Region.Name == regName);
               ct_id = ct.First().Id;
          

           }

           if (!st.Any() && StateName!="")
           {
               State s = new State();
               s.CountryId = ct_id;
               s.Name = StateName;
               s.Code = StateCode;
               context.States.AddObject(s);
               try
               {
                   context.SaveChanges();
               }
               catch(Exception ex)
               {
                   
               }

           }


       }


        public static void AddCountryStateRegions()
        {
            CreateCountryState("First", "USA_Test","11", "NewYork_Test","22");
            CreateCountryState("First", "Canada_Test","33", "Alberta_Test","44");
            CreateCountryState("Second", "Israel_Test", "55", "", "");

        }

        public static void AddTestAgenciesGroups()
        {

            CreateAgencyGroup("First_Group1","USA_Test","NewYork_Test");
            CreateAgencyGroup("Second_Group2","Canada_Test","Alberta_Test");
            CreateAgencyGroup("Third_Group3","Israel_Test","");

        }


        public static void AddHomeCareServiceType()
        {
            var context = new ccEntities();
            var t = context.ServiceTypes.Where(c => c.Name == "Homecare");
            if (!t.Any())
            {
                ServiceType s = new ServiceType();
                s.Name = "HomeCare";
                context.ServiceTypes.AddObject(s);
                context.SaveChanges();

            }
        }


        public static void CreateService(string TypeName, string SerName)
        {
            var context = new ccEntities();
            var s = context.Services.Where(c => c.Name ==SerName);
            
            var t = context.ServiceTypes.Where(c => c.Name == TypeName);
            int t_id = 0;
            if (t.Any()) t_id = t.First().Id; 
            if (!s.Any())
            {
                Service ser = new Service();
                ser.Name = SerName;
                ser.TypeId = t_id;
                ser.ReportingMethodId = 1;
                context.Services.AddObject(ser);
                context.SaveChanges();

            }

        }

        public static void AddServices()
        {

            AddHomeCareServiceType();
            CreateService("HomeCare", "Test_Service1");
            CreateService("HomeCare", "Test_Service2");
            CreateService("HomeCare", "Test_Service3");


        }

        public static void AddTestAgencies()
        {
        
            CreateAgency("Agency1_FirstTest", "First_Group1");
            CreateAgency("Agency2_SecondTest", "Second_Group2");
            CreateAgency("Agency3_FirstTest", "First_Group1");

        }

        public static void CreateGroupService(string GroupName, string SerName)
        {

            ccEntities context = new ccEntities();
           
            int s_id = 0;
            var gs = context.AgencyGroups.Where(g=>g.Name==GroupName);
            if (gs.Any()) //must exists: created before
            {
                var gs_ser = gs.First().Services.Where(s => s.Name == SerName);
                if (!gs_ser.Any()) //must exists: created before
                {

                    var ser= context.Services.Where(s => s.Name == SerName);
                    if (ser.Any())
                    {
                        s_id = ser.First().Id;
                        gs.First().Services.Add(ser.First());
                        context.SaveChanges();
                    }



                }
            
            }


        }

        public static void AddAgencyGroupServices()
        {

            CreateGroupService("First_Group1", "Test_Service1");
            CreateGroupService("First_Group1", "Test_Service2");
            CreateGroupService("First_Group1", "Test_Service3");

            CreateGroupService("Second_Group2", "Test_Service1");
            CreateGroupService("Second_Group2", "Test_Service2");
            CreateGroupService("Second_Group2", "Test_Service3");

        }

        public static void CreateFund(string MasterName, string FundName, DateTime from, DateTime to, decimal Amount, string curr)
        {
            ccEntities context = new ccEntities();
            var ms = context.MasterFunds.Where(m => m.Name == MasterName);
            int ms_id = 0;
            if (!ms.Any())
            {
                MasterFund mf = new MasterFund();
                mf.Name = MasterName;
                mf.StartDate = from;
                mf.EndDate = to;
                mf.Amount = Amount;
                mf.CurrencyCode = curr;

                context.MasterFunds.AddObject(mf);
                context.SaveChanges();
                ms = context.MasterFunds.Where(m => m.Name == MasterName);

            }
            ms_id = ms.First().Id;

            var fn = context.Funds.Where(m => m.Name == FundName && m.MasterFundId==ms_id);
            if (!fn.Any())
            {
                Fund f = new Fund();
                f.Name = FundName;
                f.StartDate = from;
                f.EndDate = to;
                f.Amount = Amount;
                f.CurrencyCode = curr;
                f.MasterFundId = ms_id;

                context.Funds.AddObject(f);
                context.SaveChanges();
            }

        }


        public static void CreateCurrency(string CurrCode, string CurrName)
        {

            ccEntities context = new ccEntities();
            
            var c1 = context.Currencies.Where(c=>c.Id==CurrCode);
            if (!c1.Any())
            {
                Currency curr = new Currency();
                curr.Name = CurrName;
                curr.Id = CurrCode;
                context.Currencies.AddObject(curr);
            }


        }

        public static void AddCurrencies()
        {
            CreateCurrency("USD", "US Dollar");
            CreateCurrency("EUR", "Euro");

        }


        public static void AddFunds()
        {
            DateTime from=DateTime.Parse("01/01/2012");
            DateTime to=DateTime.Parse("01/01/2013");

            CreateFund("Test_Master1", "Test_Fund1",from, to, 1000,"USD");
            CreateFund("Test_Master2", "Test_Fund2",from, to, 500,"EUR");


        }

        public static void CreateApp(string appName, string FundName, string serName, bool cont, decimal grant, decimal req, DateTime from, DateTime to)
        {
            ccEntities context = new ccEntities();
            var fn = context.Funds.Where(m => m.Name == FundName);
            int fn_id = 0;
            if (fn.Any()) //must be
            {
                fn_id = fn.First().Id;

            }

            var a = context.Apps.Where(app => app.Name == appName);
            if (!a.Any())
            {

                App app = new App();
                app.Name = appName;
                app.AgencyContribution = cont;
                app.CcGrant = grant;
                app.RequiredMatch = req;
                app.StartDate = from;
                app.EndDate = to;
                app.FundId = fn_id;
                context.Apps.AddObject(app);
                try
                {
                    context.SaveChanges();

                }
                catch (Exception ex)
                {

                }
            }

            a = context.Apps.Where(app => app.Name == appName);
            if (a.Any()) //must be
            {

                var ser=a.First().Services.Where(s => s.Name == serName);
                if (!ser.Any())
                {
                    var s1 = context.Services.Where(x => x.Name == serName);
                    if (s1.Any()) //must be
                    {
                        Service ss = s1.First();
                        a.First().Services.Add(ss);
                        context.SaveChanges();
                    }



                }


            }
            

            


        }

        public static void AddAppServices()
        {
            DateTime from=DateTime.Parse("01/01/2012");
            DateTime to=DateTime.Parse("01/01/2013");

            CreateApp("Test_App1", "Test_Fund1", "Test_Service1", true, 100, 100, from, to);
            CreateApp("Test_App2", "Test_Fund1", "Test_Service2", false, 200, 200, from, to);
            CreateApp("Test_App3", "Test_Fund2","Test_Service1", true, 500, 500, from, to);
            CreateApp("Test_App4", "Test_Fund2", "Test_Service2",true, 100, 100, from, to);

        }

    }

}
