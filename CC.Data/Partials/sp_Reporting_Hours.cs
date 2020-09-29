using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CC.Data
{
    public partial class sp_Reporting_HcHours_Result
	{
		public decimal? AddlQuantity { get { return this.HcCapHours - this.Quantity; } }
		public decimal? AvgRateUsd
		{
			get
			{
				if (this.Quantity == 0)
				{
					return null;
				}
				else
				{
					return this.UsdAmount / this.Quantity;
				}
			}
		}
		public decimal? AvgRateEur
		{
			get
			{
				if (this.Quantity == 0)
				{
					return null;
				}
				else
				{
					return this.EurAmount / this.Quantity;
				}
			}
		}
		public decimal? AddlUsd
		{
			get { return this.AvgRateUsd * this.AddlQuantity; }
		}
		public decimal? AddlEur
		{
			get { return this.AvgRateEur * this.AddlQuantity; }
		}
	}
}
