using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CC.Data;

namespace CC.Web.Models
{
	public class MainReportCreateModel
	{
		public MainReportCreateModel() : base() { this.MainReport = new MainReport(); this.IsValid=true;}
		public MainReportCreateModel(CC.Data.ccEntities db, User user, CC.Data.Services.IPermissionsBase permissions)
			: this()
		{


		}
		public bool IsValid{get;set;}

		public void LoadSelectLists(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{

		}

		public void LoadRelatedData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
		var validStatuses = AppBudget.ValidStatuses.Select(f=>(int?	)f);
			this.MainReport.AppBudget = db.AppBudgets
			.Include(f => f.App.Fund)
			.Include(f => f.App.AgencyGroup)
			.Where(permissions.AppBudgetsFilter)
			.Where(f => validStatuses.Contains(f.StatusId))
			.Single(f => f.Id == this.MainReport.AppBudgetId);
			IsLoaded = true;
		}


		public bool IsLoaded;

		public MainReport MainReport { get; set; }

		public SelectList StartDates
		{
			get
			{
				var d = NewMethod();
				return new SelectList(d.Select(f => new { Id = f, Name = f.ToString("MMM yyyy") }), "Id", "Name", this.MainReport.Start);
			}
		}
		public SelectList Ends
		{
			get
			{
				var d = NewMethod();
				return new SelectList(d.Select(f => f.AddMonths(this.MainReport.AppBudget.App.AgencyGroup.ReportingPeriodId)).Select(f => new { Id = f, Name = f.ToString("MMM yyyy") }), "Id", "Name", this.MainReport.Start);
			}
		}
		private List<DateTime> NewMethod()
		{
			var appStart = this.MainReport.AppBudget.App.StartDate;
			var appEnd = this.MainReport.AppBudget.App.EndDate;
			var monthsPerReport = this.MainReport.AppBudget.App.AgencyGroup.ReportingPeriodId;

			var d = new List<DateTime>();
			while (appStart.AddMonths(monthsPerReport).Date <= appEnd.Date)
			{
				d.Add(appStart);
				appStart = appStart.AddMonths(monthsPerReport);
			}
			return d;
		}
	}
}