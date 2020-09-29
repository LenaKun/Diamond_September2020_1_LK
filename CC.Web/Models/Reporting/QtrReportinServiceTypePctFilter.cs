using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;


namespace CC.Web.Models.Reporting
{
	[CustomBinder(typeof(QtrReportinServiceTypePctFilterBinder))]
	public class QtrReportinServiceTypePctFilter : ReportingServiceTypePctFilterBase
	{
		[ClientOnlyRegexAttribute(@"^\d{1,2}/\d{2,4}$", ErrorMessage = "Please enter Qtr/Yr")]
		public string From { get; set; }

		[ClientOnlyRegexAttribute(@"^\d{1,2}/\d{2,4}$", ErrorMessage = "Please enter Qtr/Yr")]
		public string To { get; set; }


		public int? FromYear
		{
			get;
			set;
		}
		public int? FromQuarter
		{
			get;
			set;
		}

		public int? ToYear
		{
			get;
			set;
		}
		public int? ToQuarter
		{
			get;
			set;
		}
	}

	

	internal class QtrReportinServiceTypePctFilterBinder : System.Web.Mvc.DefaultModelBinder
	{
		public override object BindModel(System.Web.Mvc.ControllerContext controllerContext, System.Web.Mvc.ModelBindingContext bindingContext)
		{

			var model = base.BindModel(controllerContext, bindingContext);

			var f = (QtrReportinServiceTypePctFilter)model;
			var From = ParseQtr(bindingContext.ValueProvider.GetValue("QtrReportingServiceTypePctFilter.From").AttemptedValue);
			if (From != null)
			{
				f.FromQuarter = From.Item1;
				f.FromYear = From.Item2;
			}
			var To = ParseQtr(bindingContext.ValueProvider.GetValue("QtrReportingServiceTypePctFilter.To").AttemptedValue);
			if (To != null)
			{
				f.ToQuarter = To.Item1;
				f.ToYear = To.Item2;
			}

			return model;
		}
		private Tuple<int,int> ParseQtr(object o)
		{
			var s = o as string;
			if (s == null) { return null; }
			else
			{
				var ss = s.Trim().Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
				if (ss.Length == 2)
				{
					var iss = new int[2];
					if (int.TryParse(ss[0], out iss[0]) && int.TryParse(ss[1], out iss[1]))
					{
						return new Tuple<int, int>(iss[0], iss[1]);
					}
				}
			}
			return null;
		}
			
	}
}
