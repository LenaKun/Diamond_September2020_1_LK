using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
    public class HomeCareEntitledPeriodsIndexModel:ModelBase
    {
        public int ClientId { get; set; }
		public bool CanEdit { get { return (FixedRoles.AgencyUser | FixedRoles.Ser | FixedRoles.AgencyUserAndReviewer | FixedRoles.SerAndReviewer | FixedRoles.Admin).HasFlag((FixedRoles)this.Permissions.User.RoleId); } }
    }
}