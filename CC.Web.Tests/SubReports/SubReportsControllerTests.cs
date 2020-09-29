using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CC.Web.Controllers;
using CC.Web.Models;

namespace CC.Web.Tests.SubReports
{
	[TestClass]
	public class SubReportsControllerTests
	{
		[TestMethod]
		public void TestMethod1()
		{
            //var controller = new SubReportsController();
            //controller.CcUser = new CC.Data.User();
			var s = new Moq.Mock<System.Web.HttpContextBase>()
			{

			};
			var identity = new System.Security.Principal.GenericIdentity("admin");
			var principal = new System.Security.Principal.GenericPrincipal(identity,null);
			s.Setup(f => f.User).Returns(principal);

			var att = new System.Web.Mvc.CcAuthorizeAttribute(CC.Data.FixedRoles.Admin);
			
			
            //var actionresult = controller.Create(new SubReportCreateModel());
		}
	}

}
