using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models.Import.ClientReports
{
	public class UploadModel : IValidatableObject
	{
		public int? SubReportId { get; set; }
		public int? AppBudgetServiceId { get; set; }
		public int? MainReportId { get; set; }
		public Guid? Id { get; set; }

		public HttpPostedFileWrapper File { get; set; }

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (this.SubReportId == null && this.AppBudgetServiceId == null && this.MainReportId == null)
			{
				yield return new ValidationResult("");
			}
		}

		public IEnumerable<string> CsvColumns { get; set; }

	}


}