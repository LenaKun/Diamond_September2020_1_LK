using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using CC.Data.Services;
using System.ComponentModel.DataAnnotations;

namespace CC.Web.Models
{
	public class HseapDetailedModel
	{
		public static IQueryable<HseapDetailedRow> HseapDetailedData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, int id)
		{
			var source = from er in db.EmergencyReports.Where(permissions.EmergencyReportsFilter)
						 where er.SubReport.MainReportId == id
						 select new HseapDetailedRow
						 {
							 DateOfGrant = er.ReportDate,
							 ClientId = er.ClientId,
							 LastName = er.Client.LastName,
							 FirstName = er.Client.FirstName,
							 NationalId = er.Client.NationalId,
							 Dob = er.Client.BirthDate,
							 CatCode = er.EmergencyReportType.Name,
							 PurposeOfGrant = er.Remarks,
							 Amount = er.Amount,
							 NaziPersecutionDetails = er.Client.NaziPersecutionDetails,
							 TotalAmount = (er.Amount ?? 0) + er.Discretionary,
						 };
			return source;
		}
	}

	public class HseapDetailedRow
	{
		public DateTime DateOfGrant { get; set; }

		public int ClientId { get; set; }

		public string LastName { get; set; }

		public string FirstName { get; set; }

		public string NationalId { get; set; }

		public DateTime? Dob { get; set; }

		public string CatCode { get; set; }

		public string PurposeOfGrant { get; set; }

		public decimal? Amount { get; set; }

		public string NaziPersecutionDetails { get; set; }

		public decimal TotalAmount { get; set; }
	}

	public class HseapSummaryModel
	{


		/// <summary>
		/// Main report Id
		/// </summary>
		public int Id { get; set; }

		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public DateTime EndDisplay { get { return End.AddDays(-1); } }
		public List<DataRow> Data { get; set; }
		public DataRow TotalsRow { get; set; }
		public class DataRow
		{
			public DataRow() { this.ReportSummary = new DataRowSummary(); this.AppSummary = new DataRowSummary(); }
			public string CatCode { get; set; }
			public DataRowSummary ReportSummary { get; set; }
			public DataRowSummary AppSummary { get; set; }

			public int TypeId { get; set; }

			public string TypeName { get; set; }

			public string TypeDescription { get; set; }

			public string StartDateString { get; set; }

			public string EndDateString { get; set; }
		}
		public class DataRowSummary
		{
			public int ClientsCount { get; set; }
			public int GrantsCount { get; set; }
			public decimal TotalAmount { get; set; }
		}


		public static HseapSummaryModel HseapDetailedData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, int id)
		{
			var result = db.MainReports.Where(permissions.MainReportsFilter)
				.Select(f => new HseapSummaryModel
				{
					Id = f.Id,
					Start = f.Start,
					End = f.End,
					AppId = f.AppBudget.AppId,
				})
				.SingleOrDefault(f => f.Id == id);

            if(result == null)
            {
                throw new ArgumentException("Main report not found");
            }

			var mrs = from sr in db.SubReports.Where(permissions.SubReportsFilter)
					  where sr.MainReportId == result.Id
					  join er in db.EmergencyReports.Where(permissions.EmergencyReportsFilter) on sr.Id equals er.SubReportId
					  group er by new { MainReportId = sr.MainReportId, er.TypeId } into erg
					  select new
					  {
						  MainReportId = erg.Key.MainReportId,
						  TypeId = erg.Key.TypeId,
						  ClientsCount = erg.Select(f => f.ClientId).Distinct().Count(),
						  GrantsCount = erg.Count(),
						  TotalAmount = ((decimal?)erg.Sum(f => f.Total ?? 0) ?? 0)
					  };

			var apps = from mr in db.MainReports.Where(permissions.MainReportsFilter).Where(MainReport.CurrentOrSubmitted(id))
					   where mr.AppBudget.AppId == result.AppId
					   join sr in db.SubReports.Where(permissions.SubReportsFilter) on mr.Id equals sr.MainReportId
					   join er in db.EmergencyReports.Where(permissions.EmergencyReportsFilter) on sr.Id equals er.SubReportId
					   group er by new { AppId = mr.AppBudget.AppId, TypeId = er.TypeId } into erg
					   select new
					   {
						   AppId = erg.Key.AppId,
						   TypeId = erg.Key.TypeId,
						   ClientsCount = erg.Select(f => f.ClientId).Distinct().Count(),
						   GrantsCount = erg.Count(),
						   TotalAmount = ((decimal?)erg.Sum(f => f.Total ?? 0) ?? 0)
					   };

			var q = from t in db.EmergencyReportTypes

					join a in mrs on t.Id equals a.TypeId into mrsg
					from a in mrsg.DefaultIfEmpty()
					join b in apps on t.Id equals b.TypeId into appsg
					from b in appsg.DefaultIfEmpty()
					select new DataRow
					{
						TypeId = t.Id,
						TypeName = t.Name,
						TypeDescription = t.Description,
						ReportSummary = new DataRowSummary
						{
							ClientsCount = ((int?)a.ClientsCount) ?? 0,
							GrantsCount = ((int?)a.GrantsCount) ?? 0,
							TotalAmount = ((decimal?)a.TotalAmount) ?? 0
						},
						AppSummary = new DataRowSummary
						{
							ClientsCount = ((int?)b.ClientsCount) ?? 0,
							GrantsCount = ((int?)b.GrantsCount) ?? 0,
							TotalAmount = ((decimal?)b.TotalAmount) ?? 0
						}
					};


			result.Data = q.ToList();


			var mrst = from sr in db.SubReports.Where(permissions.SubReportsFilter)
					   where sr.MainReportId == result.Id
					   join er in db.EmergencyReports.Where(permissions.EmergencyReportsFilter) on sr.Id equals er.SubReportId
					   group er by new { MainReportId = sr.MainReportId } into erg
					   select new
					   {
						   MainReportId = erg.Key.MainReportId,
						   ClientsCount = erg.Select(f => f.ClientId).Distinct().Count(),
						   GrantsCount = erg.Count(),
						   TotalAmount = ((decimal?)erg.Sum(f => f.Total ?? 0) ?? 0)
					   };

			var appst = from mr in db.MainReports.Where(permissions.MainReportsFilter).Where(MainReport.CurrentOrSubmitted(id))
						where mr.AppBudget.AppId == result.AppId
						join sr in db.SubReports.Where(permissions.SubReportsFilter) on mr.Id equals sr.MainReportId
						join er in db.EmergencyReports.Where(permissions.EmergencyReportsFilter) on sr.Id equals er.SubReportId
						group er by new { AppId = mr.AppBudget.AppId } into erg
						select new
						{
							AppId = erg.Key.AppId,
							ClientsCount = erg.Select(f => f.ClientId).Distinct().Count(),
							GrantsCount = erg.Count(),
							TotalAmount = ((decimal?)erg.Sum(f => f.Total ?? 0) ?? 0)
						};


			result.TotalsRow = new DataRow
			{
				TypeName = "TOTAL",
				TypeId = 0,
				TypeDescription = null,
				ReportSummary = mrst.Select(f => new DataRowSummary
				{
					ClientsCount = f.ClientsCount,
					GrantsCount = f.GrantsCount,
					TotalAmount = f.TotalAmount
				}).FirstOrDefault(),
				AppSummary = appst.Select(f => new DataRowSummary
				{
					ClientsCount = f.ClientsCount,
					GrantsCount = f.GrantsCount,
					TotalAmount = f.TotalAmount
				}).FirstOrDefault()
			};


			return result;
		}
		public static IEnumerable<DataRowExportRow> HseapDetailedExportData(CC.Data.ccEntities db, CC.Data.Services.IPermissionsBase permissions, int id)
		{
			var data = HseapDetailedData(db, permissions, id);

			var result = data.Data.Select(f => new DataRowExportRow
			{
				sds = data.Start.ToMonthString(),
				eds = data.End.AddDays(-1).ToMonthString(),
				tn = f.TypeName + (string.IsNullOrWhiteSpace(f.TypeDescription) ? string.Empty : string.Format("({0})", f.TypeDescription)),
				ccc = f.ReportSummary.ClientsCount,
				cgc = f.ReportSummary.GrantsCount,
				cta = f.ReportSummary.TotalAmount,
				acc = f.AppSummary.ClientsCount,
				agc = f.AppSummary.GrantsCount,
				ata = f.AppSummary.TotalAmount
			}).ToList();
			result.AddRange(new[] { data.TotalsRow }.Select(f => new DataRowExportRow
			{
				sds = data.Start.ToMonthString(),
				eds = data.End.AddDays(-1).ToMonthString(),
				tn = "Total",
				ccc = f.ReportSummary == null ? 0 : f.ReportSummary.ClientsCount,
				cgc = f.ReportSummary == null ? 0 : f.ReportSummary.GrantsCount,
				cta = f.ReportSummary == null ? 0 : f.ReportSummary.TotalAmount,
				acc = f.AppSummary == null ? 0 : f.AppSummary.ClientsCount,
				agc = f.AppSummary == null ? 0 : f.AppSummary.GrantsCount,
				ata = f.AppSummary == null ? 0 : f.AppSummary.TotalAmount
			}));


			return result;
		}

		public class DataRowExportRow
		{
			[Display(Name = "Start Date")]
			public string sds { get; set; }

			[Display(Name = "End Date")]
			public string eds { get; set; }

			[Display(Name = "Category of Grant")]
			public string tn { get; set; }

			[Display(Name = "Number of Nazi Victims Served")]
			public int ccc { get; set; }

			[Display(Name = "Number of Grants	")]
			public int cgc { get; set; }

			[Display(Name = "Total Amount (USD)	")]
			public decimal cta { get; set; }

			[Display(Name = "ITD Number of Nazi Victims Served")]
			public int acc { get; set; }

			[Display(Name = "ITD Number of Grants	")]
			public int agc { get; set; }

			[Display(Name = "ITD Total Amount (USD)	")]
			public decimal ata { get; set; }
		}

		public int AppId { get; set; }
	}
}