//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Xml.Linq;
//using System.IO;
//using System.Xml;


//namespace CC.Data
//{
//    public class DataBaseInitializer
//    {
//        private CcDataContext context;
        
//        private Random random = new Random();
//        private string RandomString(int len)
//        {
//            StringBuilder sb = new StringBuilder();
//            for (int i = 0; i < len; i++)
//            {
//                sb.Append(random.Next(10));
//            }
//            return sb.ToString();
//        }

//        //DUMMY DATA
//        public void AddClients()
//        {
//            var dal = new CcDataContext();
//            {
//                var agencies = new List<Agency>();
//                for (int i = 0; i < 100; i++)
//                {
//                    agencies.Add(
//                    new Agency()
//                    {
//                        Name = "Agency" + this.RandomString(3)
//                    });
//                }
//                agencies.ForEach(a => dal.Agencies.Add(a));
//                dal.SaveChanges();

//                DateTime t = DateTime.Now;
//                int clientcount = 1000;
//                for (int i = 0; i < clientcount; i++)
//                {
//                    Client c = new Client()
//                    {
//                        FirstName = string.Format("FirstName{0}", i),
//                        LastName = string.Format("LastName{0}", i),
//                        MiddleName = string.Format("midName{0}", i),
//                        Address = string.Format("FirstName{0}", i),
//                        AgencyId = agencies[random.Next(agencies.Count)].Id,
//                        City = string.Format("City{0}", random.Next(100)),
//                        CountryCode = this.RandomString(3),
//                        Date_Emigrated = new DateTime(1991, 1, 1).AddMonths(random.Next(100)).ToShortDateString(),
//                        Deceased = random.Next(2) == 1,
//                        DOB = new DateTime(1919, 1, 1).AddDays(random.Next(365 * 10)),
//                        DOD = new DateTime(1919, 1, 1).AddDays(random.Next(365 * 10)),
//                        EmigrationDate = new DateTime(1950, 1, 1).AddDays(random.Next(365 * 20)),
//                        JoinDate = new DateTime(2000, 1, 1).AddDays(random.Next(365 * 20)),
//                        LeaveDate = null,
//                        LeaveReason = null,
//                        LeaveRemarks = this.RandomString(15),
//                        NaziPersecutionDetails = this.RandomString(15),
//                        Other_Address = this.RandomString(15),
//                        Other_Dob = new DateTime(1919, 1, 1).AddDays(random.Next(365 * 20)),
//                        Other_FirstName = this.RandomString(15),
//                        Other_LastName = this.RandomString(15),
//                        Phone = this.RandomString(12),
//                        POB_City = this.RandomString(15),
//                        POB_Country = this.RandomString(3),
//                        PrevFirstName = this.RandomString(15),
//                        Previous_AddressInIsrael = this.RandomString(15),
//                        Previous_First_Name = this.RandomString(15),
//                        Previous_Last_Name = this.RandomString(15),
//                        Provided_other_addresses = this.RandomString(15),
//                        Remarks = this.RandomString(100),
//                        StateCode = this.RandomString(2),
//                        UpdatedAt = DateTime.Now
//                    };
//                    ;
//                    dal.Clients.Add(c);
//                    int res = dal.SaveChanges();
                    
//                }


//            }
//        }


//    }
//}
