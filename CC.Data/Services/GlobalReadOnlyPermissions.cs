using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Text;

namespace CC.Data.Services
{
    class GlobalReadOnlyPermissions : GlobalOfficerPermissions
    {
        public GlobalReadOnlyPermissions(User user) : base(user) { }
        public override bool CanEditCeefFields { get { return false ; } }
		public override bool CanSeeProgramField
		{
			get
			{
				return false;
			}
		}
    }
}
