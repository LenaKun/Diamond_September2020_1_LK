using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CC.Web.Areas.Admin.Models
{
	public class GeneralSettingsModel
	{
		[Display(Name = "New Ser/Org notify to email")]
		public string NewSerOrgNotifyEmail { get; set; }

		[Display(Name = "CFS Daily Digest Notifications")]
		public string CfsDailyLastDate { get; set; }

		[Display(Name = "CFS Automated Export Date")]
		public string CfsRecordsExportDateTime { get; set; }

        [Display(Name = "CFS Daily Digest Fire Hour (Please use 24h format, for 12 AM use 0)")]
        public int? CfsDailyDigestFireHour { get; set; }
	}
}