﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Web.Mvc;

namespace CC.Web.Models
{
	/// <summary>
	/// Used to render the main reports list
	/// </summary>
	public class MainReportsListModel : ModelBase
	{
		public MainReportsListModel()
		{
			this.Filter = new MainReportsFilter();
			if (this.IsCurrentUserPo)
			{
				this.Filter.StatusId = (int)MainReport.Statuses.AwaitingProgramOfficerApproval;
			}
            else if (this.IsCurrentUserPa)
            {
                this.Filter.StatusId = (int)MainReport.Statuses.AwaitingProgramAssistantApproval;
            }
            else if (this.IsCurrentUserBmf)
            {
                this.Filter.StatusId = (int)MainReport.Statuses.Approved;
            }

		}

		public IEnumerable<MainReportRowModel> Reports { get; set; }
		public MainReportsFilter Filter { get; set; }

		public bool IsCurrentUserPo { get { return (FixedRoles.GlobalOfficer | FixedRoles.RegionOfficer).HasFlag((FixedRoles)this.User.RoleId); } }
        public bool IsCurrentUserPa { get { return (FixedRoles.RegionAssistant).HasFlag((FixedRoles)this.User.RoleId); } }
        public bool IsCurrentUserBmf { get { return (FixedRoles.BMF).HasFlag((FixedRoles)this.User.RoleId); } }
		public class MainReportsFilter
		{
			public MainReportsFilter()
			{
				Statuses = EnumExtensions.ToSelectList<MainReport.Statuses>(this.StatusId);
			}

			public bool CanReviseMainReport { get; set; }


			[Display(Name = "Region")]
			public int? RegionId { get; set; }

			[Display(Name = "Country")]
			public int? CountryId { get; set; }

			[Display(Name = "State")]
			public int? StateId { get; set; }

			[Display(Name = "Ser")]
			public int? AgencyGroupId { get; set; }

            [Display(Name = "GG Only")]
            public bool GGOnly { get; set; }

			[DateFormat()]
			[DataType(DataType.Date)]
			public DateTime? Start { get; set; }
			[DateFormat()]
			[DataType(DataType.Date)]
			public DateTime? End { get; set; }

			public int? StatusId { get; set; }
			public SelectList Statuses { get; set; }

		}


		public void LoadData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, CC.Data.User user)
		{
			var dbData = db.MainReports
				.Where(permissions.MainReportsFilter);

			if (this.Filter != null)
			{
				if (this.Filter.RegionId.HasValue)
					dbData = dbData.Where(f => f.AppBudget.App.AgencyGroup.Country.RegionId == this.Filter.RegionId);
				if (this.Filter.CountryId.HasValue)
					dbData = dbData.Where(f => f.AppBudget.App.AgencyGroup.CountryId == this.Filter.CountryId);
				if (this.Filter.StateId.HasValue)
					dbData = dbData.Where(f => f.AppBudget.App.AgencyGroup.StateId == this.Filter.StateId.Value);
				if (this.Filter.AgencyGroupId.HasValue)
					dbData = dbData.Where(f => f.AppBudget.App.AgencyGroupId == this.Filter.AgencyGroupId);
				if (this.Filter.Start.HasValue)
					dbData = dbData.Where(f => f.End > this.Filter.Start);
				if (this.Filter.End.HasValue)
					dbData = dbData.Where(f => f.Start < this.Filter.End);
				if (this.Filter.StatusId.HasValue)
					dbData = dbData.Where(f => f.StatusId == (int)this.Filter.StatusId.Value);
                if (this.Filter.GGOnly)
                    dbData = dbData.Where(f => f.AppBudget.App.Fund.MasterFundId == 73);
			}
		}
	}

}