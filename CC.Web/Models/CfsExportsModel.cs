using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Models
{
	public class CfsExportsModel : ModelBase
	{
		public IEnumerable<System.Web.Mvc.SelectListItem> Months { get; set; }

		public CfsExportsModel()
		{
			List<int> _months = new List<int>();
			for (var i = 1; i < 13; i++)
			{
				_months.Add(i);
			}
			Months = _months.Select(f => new System.Web.Mvc.SelectListItem() { Value = f.ToString(), Text = f.ToString() });

			this.newCfsEligibleClientsFilter = new NewCfsEligibleClientsFilter();
			this.cfsEligibleWithLevelChangeFilter = new CfsEligibleWithLevelChangeFilter();
			this.cfsEligibleEndDateFilter = new CfsEligibleEndDateFilter();
		}

		public NewCfsEligibleClientsFilter newCfsEligibleClientsFilter { get; set; }
		public CfsEligibleWithLevelChangeFilter cfsEligibleWithLevelChangeFilter { get; set; }
		public CfsEligibleEndDateFilter cfsEligibleEndDateFilter { get; set; }
	}

	#region filter classes
	public class NewCfsEligibleClientsFilter
	{
		public NewCfsEligibleClientsFilter()
		{
			MonthId = DateTime.Today.Month + 1;
			Year = DateTime.Today.Year;
		}

		[Display(Name = "Month")]
		public int? MonthId { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }
	}

	public class CfsEligibleWithLevelChangeFilter
	{
		public CfsEligibleWithLevelChangeFilter()
		{
			MonthId = DateTime.Today.Month;
			Year = DateTime.Today.Year;
		}

		[Display(Name = "Month")]
		public int? MonthId { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }
	}

	public class CfsEligibleEndDateFilter
	{
		public CfsEligibleEndDateFilter()
		{
			MonthId = DateTime.Today.Month;
			Year = DateTime.Today.Year;
		}

		[Display(Name = "Month")]
		public int? MonthId { get; set; }

		[Display(Name = "Year")]
		public int? Year { get; set; }
	}
	#endregion
}