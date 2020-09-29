using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
	class AgencyUserAndReviewerPermissions : AgencyUserPermissions
	{
		public AgencyUserAndReviewerPermissions(User user)
			: base(user)
		{

		}
	}
}
