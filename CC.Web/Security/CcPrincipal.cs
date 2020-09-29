using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Security;
using System.Security.Principal;
using System.Data.Entity;

namespace CC.Web.Security
{
	public class CcIdentity : IIdentity
	{
		public CcIdentity(IIdentity identity)
		{
			_authenticationType = identity.AuthenticationType;
			_isAuthenticated = identity.IsAuthenticated;
			_name = identity.Name;
		}

		private string _authenticationType;
		public string AuthenticationType
		{
			get { return _authenticationType; }
		}

		private bool _isAuthenticated;
		public bool IsAuthenticated
		{
			get { return _isAuthenticated; }
		}

		private string _name;
		public string Name
		{
			get { return _name; }
		}
	}

	public class CcPrincipal : System.Security.Principal.IPrincipal
	{
		public CcPrincipal(IPrincipal principal)
		{
			_principal = principal;

		}

		private bool _isLoaded;

		private User _ccUser;

		public User CcUser
		{
			get
			{
				if (!_isLoaded)
				{
					using (var db = new ccEntities(LazyLoadingEnabled: false, ProxyCreationEnabled: false))
					{
						
						_ccUser = db.Users.Include(f=>f.Agency.AgencyGroup).Include(f=>f.AgencyGroup).Include(f=>f.AgencyGroups)
							.SingleOrDefault(f => f.MembershipUser.LoweredUserName == _principal.Identity.Name);
						if (_ccUser == null)
						{
							System.Web.Security.FormsAuthentication.SignOut();
							System.Web.Security.FormsAuthentication.RedirectToLoginPage();
							HttpContext.Current.Response.End();
							return null;
						}
						_isLoaded = true;
					}
				}
				return _ccUser;
			}
		}

		private IPrincipal _principal;

		public IIdentity Identity
		{
			get { return _principal.Identity; }
		}

		public bool IsInRole(string role)
		{
			return role.Equals(((FixedRoles)CcUser.RoleId).ToString(), StringComparison.InvariantCultureIgnoreCase);
		}
		public static User GetCurrentCcUser()
		{
			if (HttpContext.Current != null)
			{
				if (HttpContext.Current.User.Identity.IsAuthenticated)
				{
					var ccpr = HttpContext.Current.User as CcPrincipal;
					if (ccpr != null)
					{
						return ccpr.CcUser;
					}
				}
			}
			return null;
		}
	}
}