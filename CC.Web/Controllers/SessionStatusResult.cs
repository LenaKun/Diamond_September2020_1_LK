using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Web.Controllers
{
	class SessionStatusResult
	{
		public bool Expired { get; set; }

		public double ExpiresIn { get; set; }
	}
}
