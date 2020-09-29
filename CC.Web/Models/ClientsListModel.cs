using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Web.Mvc;
using System.Collections;

namespace CC.Web.Models
{
    public class ClientsListModel:jQueryDataTableParamModel
    {
		public CC.Data.Services.IPermissionsBase Permissions { get; set; }
		public bool CanExportBegData
		{
			get
			{
				if (Permissions == null || Permissions.User == null)
				{
					return false;
				}
				return  Permissions.User.RoleId == (int)FixedRoles.Admin ||
                    Permissions.User.RoleId == (int)FixedRoles.Maintenance ||
                    Permissions.User.RoleId == (int)FixedRoles.RegionOfficer ||
                    Permissions.User.RoleId == (int)FixedRoles.RegionAssistant ||
                    Permissions.User.RoleId == (int)FixedRoles.AuditorReadOnly ||
					Permissions.User.RoleId == (int)FixedRoles.GlobalOfficer;
			}
		}
        public ClientsListFilter Filter { get; set; }
        public ClientsListEditModel UpdateModel { get; set; }

        public IEnumerable<IEnumerable<string>> Data { get; set; }
        public Dictionary<int, string> ApprovalStatuses { get; set; }

        public ClientsListModel()
        {
            this.Filter = new ClientsListFilter();
            this.UpdateModel = new ClientsListEditModel();
        }

        public ClientsListModel(CC.Data.Repositories.CcRepository db)
        {
            this.ApprovalStatuses = db.ApprovalStatuses.Select.OrderBy(f=>f.Name).ToDictionary(f => f.Id, f => f.Name);
        }

		public SelectList GetExportList()
		{
			Dictionary<string, string> exportList = new Dictionary<string, string>();
			exportList.Add(ClientExportList.Clients.ToString(), ClientExportList.Clients.DisplayName());
			exportList.Add(ClientExportList.Eligibility.ToString(), ClientExportList.Eligibility.DisplayName());
			exportList.Add(ClientExportList.Functionality.ToString(), ClientExportList.Functionality.DisplayName());
			exportList.Add(ClientExportList.ApprovalStatusChanges.ToString(), ClientExportList.ApprovalStatusChanges.DisplayName());
			if (this.CanExportBegData)
			{
				exportList.Add(ClientExportList.BEG.ToString(), ClientExportList.BEG.DisplayName());
				exportList.Add(ClientExportList.Duplicates.ToString(), ClientExportList.Duplicates.DisplayName());
			}
			if(Permissions.User.RoleId != (int)FixedRoles.BMF)
			{
				exportList.Add(ClientExportList.UnmetNeedsOther.ToString(), ClientExportList.UnmetNeedsOther.DisplayName());
			}
			exportList.Add(ClientExportList.GovHcHours.ToString(), ClientExportList.GovHcHours.DisplayName());
			exportList.Add(ClientExportList.LeaveEntries.ToString(), ClientExportList.LeaveEntries.DisplayName());
			exportList.Add(ClientExportList.HAS.ToString(), ClientExportList.HAS.DisplayName());
			return new SelectList((IEnumerable)exportList, "Key", "Value");
		}
    }

    public class ClientsListFilter
    {
        public ClientsListFilter()
        {
            Take = 10;
            Skip = 0;
        }
        public int? AgencyId { get; set; }
        public int? ApprovalStatusId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
        public int? Id { get; set; }
        public string NationalId { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public int? RegionId { get; set; }
		public int? AgencyGroupId { get; set; }
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime? CreateDateFrom { get; set; }
		[DateFormat()]
		[DataType(DataType.Date)]
		public DateTime? CreateDateTo { get; set; }
    }

    public class ClientsListEditModel
    {
        public List<int> SelectedClientIds { get; set; }

        [Required]
        public int? NewApprovalStatusId { get; set; }

        public ClientsListEditModel()
        {
            SelectedClientIds = new List<int>();
        }
    }
    
	public enum ClientExportList
	{
		[Display(Name = "Clients")]
		Clients = 0,
		[Display(Name = "Eligibility")]
		Eligibility = 1,
		[Display(Name = "Functionality")]
		Functionality = 2,
		[Display(Name = "Approval Status Changes")]
		ApprovalStatusChanges = 3,
		[Display(Name = "BEG")]
		BEG = 4,
		[Display(Name = "Duplicates")]
		Duplicates = 5,
		[Display(Name = "Unmet Needs - Other")]
		UnmetNeedsOther = 6,
		[Display(Name = "Govt HC Hours")]
		GovHcHours = 7,
		[Display(Name = "Leave Entries")]
		LeaveEntries = 8,
		[Display(Name = "HAS")]
		HAS = 9
	}
}