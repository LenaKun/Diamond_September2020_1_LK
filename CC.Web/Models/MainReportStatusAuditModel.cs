using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using CC.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data.Services;
using System.Net.Mail;

namespace CC.Web.Models
{
	public class MainReportStatusHistoryModel : MainReportDetailsBase
	{


		public MainReportStatusHistoryModel() { }
		public MainReportStatusHistoryModel(int id)
		{
			this.Id = id;
		}

		public IGenericRepository<AppBudgetService> appBudgetServicesRepository { get; set; }
		public IGenericRepository<SubReport> SubReportsRepository { get; set; }
		public IGenericRepository<MainReportStatusAudit> StatusHistoryRepository { get; set; }
		public IGenericRepository<User> usersRepostiory { get; set; }
		public override void LoadData()
		{
			base.LoadData();


			var q = this.StatusHistoryRepository.GetAll().Where(f => f.MainReportId == this.Id).Select(f => new HistoryRowModes()
			{
				StatusChangeDate = f.StatusChangeDate,
				OldStatus = (MainReport.Statuses)f.OldStatusId,
				NewStatus = (MainReport.Statuses)f.NewStatusId,
				UserName = f.User.UserName

			}).Distinct().OrderByDescending(f => f.StatusChangeDate);


			Rows = q;

		}




		public IEnumerable<HistoryRowModes> Rows { get; set; }

		public class HistoryRowModes
		{

			[Display(Name = "Date Of Status Change")]
			public DateTime? StatusChangeDate { get; set; }
			[Display(Name = "Old Status")]
			public MainReport.Statuses OldStatus { get; set; }
			[Display(Name = "New Status")]
			public MainReport.Statuses NewStatus { get; set; }
			[Display(Name = "User Name")]
			public string UserName { get; set; }
		}
	}


}