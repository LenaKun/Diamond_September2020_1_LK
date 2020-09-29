using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using CC.Data;

namespace CC.Web.Models
{
	public class HomeCareReportCreateModel
	{
		public HomeCareReportCreateModel() { }
		public HomeCareReportCreateModel(int serviceId, int agencyId)
			: this()
		{

		}

		public HomeCareReportCreateModel(int id)
		{

		}




		[Display(Name = "Report ID")]
		public int Id { get; set; }

		public int AgencyId { get; set; }
		[Display(Name = "Agency")]
		public string AgencyName { get; set; }

		public int SerId { get; set; }
		[Display(Name = "Ser")]
		public string SerName { get; set; }

		public int ServiceId { get; set; }
		[Display(Name = "Service")]
		public string ServiceName { get; set; }

		public Range<DateTime> Period { get; set; }

		public int? PrevSubReportId { get; set; }
		public System.Web.Mvc.SelectList PrevReports { get; set; }

	}

	public class HomeCareReportModel
	{
		public int SubReportId { get; set; }

	}
}