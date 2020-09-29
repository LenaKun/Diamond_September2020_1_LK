using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CC.Data;
using CC.Data.Repositories;
namespace CC.Data.Tests
{
    [TestClass]
    public class AgencyRepositoryTest
    {
        public AgencyRepositoryTest()
        {
            Helper.PrepareTestData();

        }

        public IRepository<Agency> GetAgenciesByRole(FixedRoles role, string AgencyName="")
        {
          
            User user = Helper.GetUser(role,AgencyName);
            ccEntities entity = new ccEntities(user);
            CcRepository cr = new CcRepository(user);

            return cr.Agencies;


        }

        [TestMethod]
        public void GetAgenciesForGlobalOfficer()
        {

            IRepository<Agency> ag1 = GetAgenciesByRole(FixedRoles.GlobalOfficer);
           
            IQueryable<Agency> ag2 = new ccEntities().Agencies;
            Assert.IsTrue( ag1.Select.Count() == ag2.Count(), "Global Officer can see all agencies");

        }

        [TestMethod]
        public void GetAgenciesForAdmin()
        {

            IRepository<Agency> ag1 = GetAgenciesByRole(FixedRoles.GlobalOfficer);
     
            IQueryable<Agency> ag2 = new ccEntities().Agencies;
            Assert.IsTrue(ag1.Select.Count() == ag2.Count(), "Admin can see all agencies");
          
        }



        [TestMethod]
        public void GetAgenciesForRegionalOfficer_SameRegion()
        {
            //regional officer can only see agencies for this region
            ccEntities entities = new ccEntities();
            User user = Helper.GetUser(FixedRoles.RegionOfficer, "Agency1_FirstTest");
           //get region from agency
            int agId = (int)user.AgencyId;
          //  int regId =(int)user.Agency.RegionId;

            IRepository<Agency> ag1 = GetAgenciesByRole(FixedRoles.RegionOfficer);
     
            //agencies from same region  
        //   IQueryable<Agency> ag2 = new ccEntities().Agencies.Where(r=>r.RegionId==regId);
        //    Assert.IsTrue(ag1.Select.Count() == ag2.Count(), "Regional officer get all agencies from his region");
           
             
        }

        [TestMethod]
        public void GetAgenciesForUserAgency_SameRegion()
        {
            //regional officer can only see agencies for this region
            ccEntities entities = new ccEntities();
            User user = Helper.GetUser(FixedRoles.AgencyUser, "Agency1_FirstTest");
            //get region from agency
            int agId = (int)user.AgencyId;
        //    int regId = (int)user.Agency.RegionId;

            IRepository<Agency> ag1 = GetAgenciesByRole(FixedRoles.AgencyUser,"Agency1_FirstTest");

            //agencies from same region  
            IQueryable<Agency> ag2 = new ccEntities().Agencies.Where(a => a.Id == agId);
            Assert.IsTrue(ag1.Select.Count() == ag2.Count(), "Agency officer get all agencies from his agency");


        }
    
    }
  }
