using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CC.Data.Partials;

namespace CC.Data
{
	
	public partial class CriEmergency
	{
		public CriEmergency()
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
					try
					{
						this.Date = new DateTime(value.Value, this.Date.Value.Month, 1);
					}
					catch (ArgumentOutOfRangeException) { this.Date = null; }
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
					try
					{
						this.Date = new DateTime(this.Date.Value.Year, value.Value, 1);
					}
					catch (ArgumentOutOfRangeException) { this.Date = null; }
				}
				else
				{
					this.Date = null;
				}
			}
		}
		public int? Day
		{
			get
			{
				if (this.Date.HasValue)
				{
					return this.Date.Value.Day;
				}
				else { return null; }
			}
			set
			{
				if (value.HasValue)
				{
					
					if (this.Date == null) { this.Date = new DateTime(); }
					try
					{
						this.Date = new DateTime(this.Date.Value.Year, this.Date.Value.Month, value.Value);
					}
					catch (ArgumentOutOfRangeException) { this.Date = null; }
				}
				else
				{
					this.Date = null;
				}
			}
		}

	}

	

}
