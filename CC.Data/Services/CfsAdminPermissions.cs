using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data.Services
{
	class CfsAdminPermissions : PermissionsBase
	{
		public CfsAdminPermissions(User user) : base(user) { }

		public override Expression<Func<Agency, bool>> AgencyFilter
		{
			get
			{
				return a => a.AgencyGroup.CfsDate.HasValue;
			}
		}
		public override Expression<Func<Client, bool>> ClientsFilter
		{
			get
			{
				return c => c.Agency.AgencyGroup.CfsDate.HasValue;
			}
		}
		public override Expression<Func<Client, bool>> CfsClientsFilter
		{
			get
			{
				return c => true;
			}
		}
		public override Expression<Func<AgencyGroup, bool>> AgencyGroupsFilter
		{
			get
			{
				return c => c.CfsDate.HasValue;
			}
		}
	}
}
