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


namespace CC.Web.Tests.ControllersTest.MainReport
{
    [TestClass]
    public class MainReportControllerTest
    {
        public MainReportsController GetTarget_ForUser(string GroupName = "")
        {

            MainReportsController target = new MainReportsController();


            target.CcUser = Helper.GetSerUser(GroupName);

            return target;

        }
        ccEntities context = new ccEntities();
        MainReportsController target;
        ContextMocks mocks;
        public MainReportsController Target
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
        public void MainReport_Create()
        {

            MainReportCreateModel m = new MainReportCreateModel();
            


        }


        [TestMethod]
        public void MainReport_Approve()
        {

            MainReportCreateModel m = new MainReportCreateModel();
            App app = Helper.GetApp("Test_App1");
          

            //Assert.IsTrue(app1.First().StatusId == (int)ApprovalStatusEnum.Approved, " must be approved");

        }

    }
}
