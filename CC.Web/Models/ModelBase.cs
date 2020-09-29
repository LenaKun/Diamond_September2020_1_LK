using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using System.Web.Security;
using System.Security;
using System.Security.Principal;
using CC.Data.Services;

namespace CC.Web.Models
{
	public class ModelBase
	{

		private User _user;
		public User User
		{
			get
			{
				return _user ?? (
				_user = ((CC.Web.Security.CcPrincipal)System.Web.HttpContext.Current.User).CcUser);
			}
			set { _user = value; }
		}

		public IPrincipal Principal { get; set; }

		private IPermissionsBase _permissions;
		public virtual IPermissionsBase Permissions
		{
			get
			{
				if(this.User==null) return null;
				return _permissions ?? (_permissions = CC.Data.Services.PermissionsFactory.GetPermissionsFor(this.User));
			}
		}

	}

	public class GenericModel<T> : ModelBase
	{
		public GenericModel() { }
		public GenericModel(T value)
			: base()
		{
			this.Data = value;
		}
		public GenericModel(T value, User u)
			: this(value)
		{
			User = u;
		}
		public T Data { get; set; }
	}
}