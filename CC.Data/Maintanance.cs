using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public static class Maintanance
	{
		public static void NeedToApply()
		{
			using (var db = new CC.Data.ccEntities())
			{
				var ids = db.ProcessNeedToApplyClients();
			}
		}

        public static void NoContact()
        {
            using (var db = new CC.Data.ccEntities())
            {
                var ids = db.ProcessNoContact();
            }
        }

        public static void RejectedClients()
        {
            using (var db = new CC.Data.ccEntities())
            {
                var ids = db.ProcessRejectedClients();
            }
        }
	}
}
