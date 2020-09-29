using CC.Data;
using CC.Web.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
	public class UsersModel : jQueryDataTableParamModel
	{
		public IQueryable<UsersListRow> GetUsers(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions)
		{
			return from ag in db.Users.Where(permissions.UsersFilter)
				   select new UsersListRow
				   {
					   Id = ag.Id,
					   RegionId = ag.Agency.AgencyGroup.Country.RegionId,
					   CountryId = ag.Agency.AgencyGroup.CountryId,
					   AgencyGroupId = ag.Agency.GroupId,
					   AgencyId = ag.AgencyId,
					   FirstName = ag.FirstName,
					   LastName = ag.LastName,
					   Username = ag.UserName,
					   Email = ag.Email,
					   UniqueId = ag.UniqueId,
					   RoleId = ag.RoleId,
					   Role = ag.Role.Name,
					   Disabled = ag.Disabled,
					   RoleEnd = ag.RoleId == (int)FixedRoles.RegionOfficer ? ag.Region.Name :
							   ag.RoleId == (int)FixedRoles.RegionAssistant ? ag.Region.Name :
							   (ag.RoleId == (int)FixedRoles.Ser || ag.RoleId == (int)FixedRoles.SerAndReviewer) ? ag.AgencyGroup.Name :
							   (ag.RoleId == (int)FixedRoles.AgencyUser
							   || ag.RoleId == (int)FixedRoles.AgencyUserAndReviewer
							   || ag.RoleId == (int)FixedRoles.DafEvaluator
							   || ag.RoleId == (int)FixedRoles.DafReviewer) ? ag.Agency.Name : null

				   };
		}
	}

	public class UsersExportListRow
	{
		[Display(Name = "Id", Order = 1)]
		public int Id { get; set; }
		[Display(Name = "Username", Order = 2)]
		public string Username { get; set; }
		[Display(Name = "FirstName", Order = 3)]
		public string FirstName { get; set; }
		[Display(Name = "LastName", Order = 4)]
		public string LastName { get; set; }
		[Display(Name = "Email", Order = 5)]
		public string Email { get; set; }
		[Display(Name = "Role", Order = 6)]
		public string Role { get; set; }
		[Display(Name = "", Order = 7)]
		public string RoleEnd { get; set; }
	}

	public class UsersListRow : UsersExportListRow
	{
		public int RoleId { get; set; }
		public Guid UniqueId { get; set; }
		public bool Disabled { get; set; }
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int? RegionId { get; set; }
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int? CountryId { get; set; }
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int? AgencyGroupId { get; set; }
		[System.ComponentModel.DataAnnotations.ScaffoldColumn(false)]
		public int? AgencyId { get; set; }
	}
}