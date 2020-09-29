using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
	public partial class AppsImport
	{

		public int? Year
		{
			get
			{
				if (this.StartDate.HasValue) { return this.StartDate.Value.Year; }
				else
					return null;
			}
			set
			{
				if (value.HasValue)
				{
					this.StartDate = new DateTime(value.Value, 1, 1);
					this.EndDate = new DateTime(value.Value + 1, 1, 1);
				}
				else
				{
					this.StartDate = null;
					this.EndDate = null;
				}
			}
		}
	}
}
