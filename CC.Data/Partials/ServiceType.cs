using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace CC.Data
{
	[MetadataType(typeof(ServiceTypeMetadata))]
	public partial class ServiceType
	{
		public ServiceConstraint DefaultConstraint
		{
			get { return this.ServiceConstraints.Where(f => f.FundId == null).SingleOrDefault(); }
			set
			{

				var d = this.DefaultConstraint;
				if (d == null)
				{
					this.ServiceConstraints.Add(value);
				}
				else if (value == null)
				{
					this.ServiceConstraints.Remove(value);
				}
				else
				{
					d = value;
				}
			}
		}
	}
	public class ServiceTypeMetadata
	{
	}
}
