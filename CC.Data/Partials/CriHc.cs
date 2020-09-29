using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CC.Data.Partials;

namespace CC.Data
{

	public partial class CriHc
	{
		public CriHc()
			: base()
		{
		}

		public int? Year
		{
			get
			{
				if (this.Date.HasValue)
				{
					return this.Date.Value.Year;
				}
				else { return null; }
			}
			set
			{
				if (value.HasValue)
				{
					if (this.Date == null) { this.Date = new DateTime(); }
					this.Date = new DateTime(value.Value, this.Date.Value.Month, 1);
				}
				else
				{
					this.Date = null;
				}
			}
		}
		public int? Month
		{
			get
			{
				if (this.Date.HasValue)
				{
					return this.Date.Value.Month;
				}
				else { return null; }
			}
			set
			{
				if (value.HasValue)
				{
					if (this.Date == null) { this.Date = new DateTime(); }
					this.Date = new DateTime(this.Date.Value.Year, value.Value, 1);
				}
				else
				{
					this.Date = null;
				}
			}
		}

	}

}
