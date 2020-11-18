using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class MainReportDetailsBase : ModelBase
	{
		public IGenericRepository<MainReport> mainReportsRepository { get; set; }
		public virtual void LoadData()
		{
           
			var mainReport = mainReportsRepository.GetAll(this.Permissions.MainReportsFilter)
				.Select(f => new MainReportDetailsBase()
				{
					Id = f.Id,
					AppId = f.AppBudget.AppId,
					AppBudgetId = f.AppBudgetId,
					AgencyGroupId = f.AppBudget.App.AgencyGroupId,
					AgencyGroupIsAudit = f.AppBudget.App.AgencyGroup.IsAudit,
					FundName = f.AppBudget.App.Fund.Name,
                    MasterFundName = f.AppBudget.App.Fund.MasterFund.Name,
					AppName = f.AppBudget.App.Name,
					Start = f.Start,
					End = f.End,
					AgencyGroupName = f.AppBudget.App.AgencyGroup.DisplayName,
					ExcRate = f.ExcRate,
					ExcRateSource = f.ExcRateSource,
					CurrencyId = f.AppBudget.App.CurrencyId,
					AppAmount = (decimal?)f.AppBudget.App.CcGrant,
					AppMatch = (decimal?)f.AppBudget.App.RequiredMatch,
					SerAppAmount = (decimal?)f.AppBudget.AppBudgetServices.Sum(a => a.CcGrant),
					StatusId = f.StatusId,
					RegionId = f.AppBudget.App.AgencyGroup.Country.RegionId,
					Adjusted = f.Adjusted,
					Revised = f.Revision,
					RequiresAdminApproval = f.RequiresAdminApproval,
                    LastReport = f.LastReport,
					SC = f.AppBudget.App.AgencyGroup.SupportiveCommunities,
					DCC = f.AppBudget.App.AgencyGroup.DayCenter,
					ProgramOverviewFileName = f.ProgramOverviewFileName,
					MhsaFileName = f.MhsaFileName,
					AppAvgReimbursementCost = f.AppBudget.App.AvgReimbursementCost
				}).SingleOrDefault(f => f.Id == this.Id);
			if (mainReport == null)
			{
				throw new InvalidOperationException("Main report not found");
			}

			this.AppId = mainReport.AppId;
			this.AppName = mainReport.AppName;
			this.AppBudgetId = mainReport.AppBudgetId;
			this.AgencyGroupId = mainReport.AgencyGroupId;
			this.AgencyGroupIsAudit = mainReport.AgencyGroupIsAudit;
			this.RegionId = mainReport.RegionId;
			this.FundName = mainReport.FundName;
            this.MasterFundName = mainReport.MasterFundName;
			this.Start = mainReport.Start;
			this.End = mainReport.End.AddMonths(-1); //end month including
			this.AgencyGroupName = mainReport.AgencyGroupName;
			this.ExcRate = mainReport.ExcRate;
			this.ExcRateSource = mainReport.ExcRateSource;
			this.LastAgencyComment = mainReport.LastAgencyComment;
            this.LastReport = mainReport.LastReport;
			this.SC = mainReport.SC;
			this.DCC = mainReport.DCC;
			this.ProgramOverviewFileName = mainReport.ProgramOverviewFileName;
			this.MhsaFileName = mainReport.MhsaFileName;
			this.AppAvgReimbursementCost = mainReport.AppAvgReimbursementCost;
			using (var db = new ccEntities())
			{

				this.AgencyComments = (from mr in db.MainReports
									   where mr.Id == this.Id
									   from c in mr.AgencyComments
									   orderby c.Date descending
									   select new CommentView
									   {
										   Content = c.Content,
										   Username = c.User.UserName,
										   Date = c.Date,
										   IsFile = c.IsFile
									   }).ToList();

				this.PoComments = (from mr in db.MainReports
								   where mr.Id == this.Id
								   from c in mr.PoComments
								   orderby c.Date descending
								   select new CommentView
								   {
									   Content = c.Content,
									   Username = c.User.UserName,
									   Date = c.Date
								   }).ToList();

				this.InternalComments = (from mr in db.MainReports
										 where mr.Id == this.Id
										 from c in mr.InternalComments
										 select new CommentView
										 {
											 Content = c.Content,
											 Username = c.User.UserName,
											 Date = c.Date
										 }).ToList();

				this.PostApprovalComments = (from mr in db.MainReports
											 where mr.Id == this.Id
											 from c in mr.PostApprovalComments
											 orderby c.Date descending
											 select new CommentView
											 {
												 Id = c.Id,
												 Content = c.Content,
												 Username = c.User.UserName,
												 Date = c.Date
											 }).ToList();
			}
			this.CurrencyId = mainReport.CurrencyId;
			this.LastPoComment = mainReport.LastPoComment;
			this.CurrencyId = mainReport.CurrencyId;
			this.AppAmount = mainReport.AppAmount;
			this.AppMatch = mainReport.AppMatch;
			this.SerAppAmount = mainReport.SerAppAmount;
			this.StatusId = mainReport.StatusId;
			this.AdministrativeOverheadOverflow = mainReport.AdministrativeOverheadOverflow;
			this.Adjusted = mainReport.Adjusted;
			this.Revised = mainReport.Revised;
			this.RequiresAdminApproval = mainReport.RequiresAdminApproval;

			this.CanSubmit = (FixedRoles.Admin | FixedRoles.Ser | FixedRoles.SerAndReviewer).HasFlag((FixedRoles)this.User.RoleId) && MainReport.EditableStatuses.Contains(this.Status);

			if ((FixedRoles.RegionOfficer | FixedRoles.GlobalOfficer | FixedRoles.Admin).HasFlag((FixedRoles)this.User.RoleId) 
					&& (this.Status == MainReport.Statuses.AwaitingProgramOfficerApproval || this.Status == MainReport.Statuses.AwaitingAgencyResponse))
			{
				this.PoApprovalModel = new MainReportApproveModel()
				{

					Id = this.Id,
					RequiresAdminApproval = this.RequiresAdminApproval,
					UserRole = (FixedRoles)this.Permissions.User.RoleId,
					MainReportStatus = this.Status,
					UserName = this.Permissions.User.UserName,
					AgencyGroupIsAudit = this.AgencyGroupIsAudit
				};
			}

			if ((FixedRoles.RegionOfficer | FixedRoles.RegionAssistant | FixedRoles.GlobalOfficer | FixedRoles.Admin).HasFlag((FixedRoles)this.User.RoleId) 
					&& (this.Status == MainReport.Statuses.AwaitingProgramAssistantApproval || this.Status == MainReport.Statuses.AwaitingAgencyResponse))
            {
                this.PaApprovalModel = new MainReportApproveModel()
                {

                    Id = this.Id,
                    RequiresAdminApproval = this.RequiresAdminApproval,
                    UserRole = (FixedRoles)this.Permissions.User.RoleId,
                    MainReportStatus = this.Status,
                    UserName = this.Permissions.User.UserName,
					AgencyGroupIsAudit = this.AgencyGroupIsAudit
                };
            }
		}

		public bool CanSubmit { get; set; }

		public bool Revised { get; set; }
		public MainReportApproveModel PoApprovalModel { get; set; }
        public MainReportApproveModel PaApprovalModel { get; set; }

		public int AppBudgetId { get; set; }
		public int AppId { get; set; }
		public int AgencyGroupId { get; set; }
		public bool AgencyGroupIsAudit { get; set; }
		public int RegionId { get; set; }

		/// <summary>
		/// MainReport ID
		/// </summary>
		public int Id { get; set; }
		public string FundName { get; set; }
        public string MasterFundName { get; set; }
		public string AppName { get; set; }
		[UIHint("Month")]
		public DateTime Start { get; set; }
		[UIHint("Month")]
		public DateTime End { get; set; }
		[Display(Name = "Ser")]
		public string AgencyGroupName { get; set; }

		public bool Adjusted { get; set; }
		[DataType(System.ComponentModel.DataAnnotations.DataType.Currency)]
		[Display(Name = "Exc. Rate")]
		public decimal? ExcRate { get; set; }

		[Display(Name = "Exc. Rates Source")]
		public string ExcRateSource { get; set; }

		[Display(Name = "Last Agency Remarks")]
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		public string LastAgencyComment { get; set; }
		[DataType(DataType.Html)]
		[System.Web.Mvc.AllowHtml]
		[Display(Name = "Last Program Officer Remarks")]
		public string LastPoComment { get; set; }

		public string CurrencyId { get; set; }

		[Display(Name = "App Amount")]
		public decimal? AppAmount { get; set; }
		public decimal? SerAppAmount { get; set; }
        public bool LastReport { get; set; }


		public int StatusId { get; set; }
		public MainReport.Statuses Status { get { return (MainReport.Statuses)this.StatusId; } }
		public int? PrevStatusId { get; set; }
		public MainReport.Statuses? PrevStatus { get { return this.PrevStatusId.HasValue ? (MainReport.Statuses)this.PrevStatusId : (MainReport.Statuses?)null; } }
		public System.Web.Mvc.SelectList Statuses { get { return EnumExtensions.ToSelectList<MainReport.Statuses>(null); } }

		public decimal AdministrativeOverheadOverflow { get; set; }

		public IEnumerable<CommentView> PoComments { get; set; }

		public IEnumerable<CommentView> AgencyComments { get; set; }

		public IEnumerable<CommentView> InternalComments { get; set; }

		public IEnumerable<CommentView> PostApprovalComments { get; set; }

		public class CommentView
		{
			[DataType(System.ComponentModel.DataAnnotations.DataType.Html)]
			public string Content { get; set; }
			public string Username { get; set; }
			public DateTime Date { get; set; }
			public bool IsFile { get; set; }
			public int Id { get; internal set; }
		}

		public bool RequiresAdminApproval { get; set; }

		public decimal? AppMatch { get; set; }

		public bool SC { get; set; }

		public bool DCC { get; set; }

		public string ProgramOverviewFileName { get; set; }
		public string MhsaFileName { get; set; }
		public decimal? AppAvgReimbursementCost { get; set; }
	}

}