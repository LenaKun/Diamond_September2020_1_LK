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


namespace CC.Web.Tests.ControllersTest.AppBudget
{
    [TestClass]
    public class AppBudgetControllerTest
    {
        public AppBudgetsController GetTarget_ForUser(string GroupName = "")
        {

            AppBudgetsController target = new AppBudgetsController();
          

            target.CcUser = Helper.GetSerUser(GroupName);
           
            return target;

        }
        ccEntities context=new ccEntities();
        AppBudgetsController target;
        ContextMocks mocks;
        public AppBudgetsController Target
        {
            get
            {
                if (target == null)
                {
                    target = GetTarget_ForUser("First_Group1");
                    mocks = new ContextMocks(Target, true, true);
                }
                return target;
            }
            set
            {
                target = value;
            }
        }



        [TestMethod]
        public void AppBudget_Create()
        {

            AppBudgetCreateModel m = new AppBudgetCreateModel();
            App app = Helper.GetApp("Test_App1");
            m.AppId = app.Id;
            m.AgencyGroupId = Helper.GetAgencyGroup("First_Group1").Id;
            var app1 = context.AppBudgets.Where(f => f.AgencyGroupId == m.AgencyGroupId && f.AppId == m.AppId);
            if (!app1.Any())
            {
                Target.Create(m);
            }
            app1 = context.AppBudgets.Where(f => f.AgencyGroupId == m.AgencyGroupId && f.AppId == m.AppId);

            Assert.IsTrue(app1.Any()," new AppBudget must be created");
            


        }


        [TestMethod]
        public void AppBudget_Approve()
        {

            AppBudgetCreateModel m = new AppBudgetCreateModel();
            App app = Helper.GetApp("Test_App1");
            m.AppId = app.Id;
            m.AgencyGroupId = Helper.GetAgencyGroup("First_Group1").Id;
            var app1 = context.AppBudgets.Where(f => f.AgencyGroupId == m.AgencyGroupId && f.AppId == m.AppId);
            if (app1.Any())
            {
                int id = app1.First().Id;
                Target.Submit(id);
                target.CcUser = Helper.GetRegionalUser("Agency1_FirstTest");
                Target.ApproveByRpo(id);
                target.CcUser=Helper.GetUser(FixedRoles.GlobalOfficer, "Agency1_FirstTest");
                Target.ApproveByGpo(id);

            }

            Assert.IsTrue(app1.First().StatusId ==3," must be approved");

        }

    }
}
