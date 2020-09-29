using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CC.Data;

namespace CC.Web.Models
{
    public class ClientEditModel:ClientCreateModel
    {
        
		/// <summary>
		/// The Government Issued ID field should be blocked for any Clients whose Approval Status is Approved.  Only the Admin should be able to change this field for Approved CC IDs
		/// </summary>
		/// <param name="c"></param>
		/// <param name="u"></param>
		/// <returns></returns>
		public static bool IsNationalIdEditable(Client c, User u)
		{
			return c.ApprovalStatusId == (int)ApprovalStatusEnum.New || c.NationalIdTypeId != 1 || u.RoleId == (int)FixedRoles.Admin;
		}
        

    }
}