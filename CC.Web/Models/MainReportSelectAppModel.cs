using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CC.Data;

namespace CC.Web.Models
{
	public class MainReportSelectAppModel : ModelBase
	{
		public MainReportSelectAppModel() : base() { this.StatusId = (int)MainReport.Statuses.New; }
		public MainReportSelectAppModel(CC.Data.ccEntities db, User user, CC.Data.Services.IPermissionsBase permissions)
			: this()
		{

			LoadData(db, permissions, user);

		}

		public void LoadData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, User user)
		{
			var availableAgencyGroups = db.AgencyGroups.Where(permissions.AgencyGroupsFilter).Select(f => new { Id = f.Id, Name = f.DisplayName });
			Funds = new SelectList(db.Funds.Select(f => new { Id = f.Id, Name = f.Name }).OrderBy(f => f.Name), "Id", "Name", this.FundId);
			Apps = new SelectList(db.Apps.Where(this.Permissions.AppsFilter)
				.Where(f => FundId == null || f.FundId == FundId)
				.Select(f => new { Id = f.Id, Name = f.Name })
				.OrderBy(f => f.Name), "Id", "Name", this.AppId);
		}

		public int? FundId { get; set; }
		[Required]
		public int? AppId { get; set; }
		public string AgencyGroupName { get; set; }
		public int StatusId { get; set; }
		public SelectList Apps { get; set; }
		public SelectList Funds { get; set; }

	}
}