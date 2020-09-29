using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CC.Data;
using CC.Data.Services;
using System.ComponentModel.DataAnnotations;
using System.Data.Objects.SqlClient;
using System.Globalization;

namespace CC.Web.Models
{

	public class ClientHomeCareTabJqModel : jQueryDataTableParamModel
	{
		[Display(Name="Service")]
		public int? ServiceId { get; set; }
		
		[Display(Name="From")]
		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		public DateTime? StartDate { get; set; }

		[DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
		[Display(Name="To")]
		public DateTime? EndDate { get; set; }
		
		[Display(Name="Show non submitted")]
		public bool ShowNonSubmitted { get; set; }

        [Display(Name = "GG Only")]
        public bool GGOnly { get; set; }

		public System.Web.Mvc.SelectList ServicesSelectList { get; protected set; }

		public virtual void LoadData(ccEntities db, IPermissionsBase permissions)
		{
            var services = (from c in db.Clients
							where c.Id == ClientId
							from a in c.Agency.AppBudgetServices
							where a.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare
								|| a.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
							group a by new
							{
								id = a.Service.Id,
								name = a.Service.Name
							} into ag
							orderby ag.Key.name
							select ag.Key);


			this.ServicesSelectList = new System.Web.Mvc.SelectList(services, "id", "name");
		}

		public virtual jQueryDataTableResult GetResult(ccEntities db, IPermissionsBase permissions)
		{
			var clientreports = db.ClientReports.Where(permissions.ClientReportsFilter);
			if(!ShowNonSubmitted)
			{
				clientreports = from cr in clientreports
								join mr in db.MainReports.Where(MainReport.Submitted) on cr.SubReport.MainReportId equals mr.Id
								select cr;
			}

			var q = (from cr in clientreports.Where(f => GGOnly ? f.Client.Agency.AgencyGroup.Apps.Any(app => app.Fund.MasterFundId == 73) : true)
					where cr.ClientId == this.ClientId
					where cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Homecare
					   || cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
					from ar in cr.ClientAmountReports
					let agencyApp = db.AgencyApps.FirstOrDefault(f => f.Agency.GroupId == cr.SubReport.AppBudgetService.Agency.GroupId && f.AppId == cr.SubReport.AppBudgetService.AppBudget.AppId)
					select new
					{
						Date = ar.ReportDate,
						Rate = cr.Rate,
						Cur = cr.SubReport.AppBudgetService.AppBudget.App.CurrencyId,
						SubReportId = cr.SubReport.Id,
						ServiceId = cr.SubReport.AppBudgetService.ServiceId,
						ServiceName = cr.SubReport.AppBudgetService.Service.Name,
						AppName = cr.SubReport.AppBudgetService.AppBudget.App.Name,
						FundName = cr.SubReport.AppBudgetService.AppBudget.App.Fund.Name,
						MasterFundName = cr.SubReport.AppBudgetService.AppBudget.App.Fund.MasterFund.Name,
						Quantity = ar.Quantity,
						IsWeekly = cr.SubReport.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly,
						SelectedDayOfWeek = (int?)agencyApp.WeekStartDay,
						MrStart = cr.SubReport.MainReport.Start
					}).ToList();

			DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
			Calendar cal = dfi.Calendar;

            var source = from item in q
						 group item by new
						 {
							 Date = item.Date,
							 Rate = item.Rate,
							 Cur = item.Cur,
							 SubReportId = item.SubReportId,
							 ServiceId = item.ServiceId,
							 ServiceName = item.ServiceName,
							 AppName = item.AppName,
							 FundName = item.FundName,
                             MasterFundName = item.MasterFundName,
							 IsWeekly = item.IsWeekly,
							 WeekNumber = item.IsWeekly ? cal.GetWeekOfYear(item.Date, dfi.CalendarWeekRule, item.SelectedDayOfWeek != null ? (DayOfWeek)item.SelectedDayOfWeek : item.MrStart.DayOfWeek) : item.Date.Month
						 } into arg
						 select new hctr
						 {
							 Date = arg.Key.Date,
							 ServiceId = arg.Key.ServiceId,
							 ServiceName = arg.Key.ServiceName,
							 Quantity = arg.Sum(f => f.Quantity),
							 Rate = arg.Key.Rate,
							 Cur = arg.Key.Cur,
							 Amount = arg.Key.Rate * arg.Sum(f => f.Quantity),
							 FundName = arg.Key.FundName,
							 AppName = arg.Key.AppName,
							 SubReportId = arg.Key.SubReportId,
                             MasterFundName = arg.Key.MasterFundName,
							 IsWeekly = arg.Key.IsWeekly,
							 WeekNumber = "W" + arg.Key.WeekNumber.ToString()
						 };
			var filtered = source.AsQueryable();
			if (StartDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date >= this.StartDate);
			}
			if (EndDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date <= this.EndDate);
			}
			if (this.ServiceId.HasValue)
			{
				filtered = filtered.Where(f => f.ServiceId == this.ServiceId);
			}
			if (this.sSortCol_0 == "DateName") { this.sSortCol_0 = "Date"; }
			var sorted = filtered.OrderByField(this.sSortCol_0, this.sSortDir_0 == "asc");

			var data = sorted.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList();

			return new jQueryDataTableResult
			{
				sEcho = this.sEcho,
				aaData = data,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()

			};


		}

	}

	class hctr
	{
		public DateTime? Date { get; set; }

		public virtual string DateName { get { return Date.HasValue ? Date.Value.ToMonthString() : null; } }

		public decimal? Rate { get; set; }

		public decimal? Quantity { get; set; }

		public decimal? Amount { get; set; }

		public int ServiceId { get; set; }

		public string ServiceName { get; set; }

		public string ServiceTypeName { get; set; }

		public string Cur { get; set; }

		public string FundName { get; set; }

        public string MasterFundName { get; set; }

		public string AppName { get; set; }

		public string WeekNumber { get; set; }

		public bool IsWeekly { get; set; }

		public int SubReportId { get; set; }
	}
	public class ClientEmergenciesTabModel : ClientHomeCareTabJqModel
	{
		public override void LoadData(ccEntities db, IPermissionsBase permissions)
		{
            var services = (from c in db.Clients
                            where c.Id == ClientId
							from a in db.Services
							where a.TypeId == (int)Service.ServiceTypes.EmergencyAssistance
							select new
							{
								id = a.Id,
								name = a.Name
							});

			this.ServicesSelectList = new System.Web.Mvc.SelectList(services, "id", "name");

			var types =  db.EmergencyReportTypes.OrderBy(f=>f.Name).Select(f=>new {Id = f.Id, Name = f.Name});
			
			this.EmergencyTypesSelectList = new System.Web.Mvc.SelectList(types, "Id", "Name");

		}
		public override jQueryDataTableResult GetResult(ccEntities db, IPermissionsBase permissions)
		{
			var source = from cr in db.EmergencyReports.Where(permissions.EmergencyReportsFilter)
                             .Where(f => GGOnly ? f.SubReport.AppBudgetService.AppBudget.App.Fund.MasterFundId == 73 : true)
						 where cr.ClientId == this.ClientId
						 where cr.SubReport.AppBudgetService.Service.TypeId == (int)Service.ServiceTypes.EmergencyAssistance
						 select new etr
						 {
							 Date = cr.ReportDate,
							 Amount = cr.Amount,
							 Discretionary = cr.Discretionary,
							 Total = cr.Total,
							 TypeId = cr.TypeId,
							 TypeName = cr.EmergencyReportType.Name,
							 PurposeOfGrant = cr.Remarks,
							 Cur = cr.SubReport.AppBudgetService.AppBudget.App.CurrencyId,
							 ServiceId = cr.SubReport.AppBudgetService.ServiceId,
							 ServiceName = cr.SubReport.AppBudgetService.Service.Name,
							 AppName = cr.SubReport.AppBudgetService.AppBudget.App.Name,
							 FundName = cr.SubReport.AppBudgetService.AppBudget.App.Fund.Name,
                             MasterFundName = cr.SubReport.AppBudgetService.AppBudget.App.Fund.MasterFund.Name,
							 SubReportId = cr.SubReport.Id
						 };
			var filtered = source;
			if (StartDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date >= this.StartDate);
			}
			if (EndDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date <= this.EndDate);
			}
			if (this.ServiceId.HasValue)
			{
				filtered = filtered.Where(f => f.ServiceId == this.ServiceId);
			}
			if (this.TypeId.HasValue)
			{
				filtered = filtered.Where(f => f.TypeId == this.TypeId);
			}
			if (this.sSortCol_0 == "DateName") { this.sSortCol_0 = "Date"; }
			var sorted = filtered.OrderByField(this.sSortCol_0, this.sSortDir_0 == "asc");

			var data = sorted.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList();

			return new jQueryDataTableResult
			{
				sEcho = this.sEcho,
				aaData = data,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()

			};


		}


		public System.Web.Mvc.SelectList EmergencyTypesSelectList { get; set; }

        [Display(Name = "Emergency Type")]
		public int? TypeId { get; set; }
	}
	class etr : hctr
	{
		public override string DateName
		{
			get
			{
				return this.Date.HasValue ? this.Date.Value.ToShortDateString() : null;
			}
		}

		public decimal Discretionary { get; set; }

		public decimal? Total { get; set; }

		public int TypeId { get; set; }

		public string TypeName { get; set; }

		public string PurposeOfGrant { get; set; }
	}
	public class ClientOtherServicesTabModel : ClientHomeCareTabJqModel
	{
		public override void LoadData(ccEntities db, IPermissionsBase permissions)
		{
            var services = (from c in db.Clients
							where c.Id == ClientId
							from a in c.Agency.AppBudgetServices
							where a.Service.TypeId != (int)Service.ServiceTypes.EmergencyAssistance
							where a.Service.ReportingMethodId != (int)Service.ReportingMethods.Homecare
								&& a.Service.ReportingMethodId != (int)Service.ReportingMethods.HomecareWeekly
							group a by new
							{
								id = a.Service.Id,
								name = a.Service.ServiceType.Name + " - " + a.Service.Name
							} into ag
							orderby ag.Key.name
							select ag.Key);

			this.ServicesSelectList = new System.Web.Mvc.SelectList(services, "id", "name");
		}
		class abc
		{
			public int ClientId { get; set; }
			public int SubReportId { get; set; }

			public DateTime? Date { get; set; }

			public decimal? Amount { get; set; }

			public decimal? Quantity { get; set; }
		}
		public override jQueryDataTableResult GetResult(ccEntities db, IPermissionsBase permissions)
		{

			var source =
				from c in db.Clients.Where(permissions.ClientsFilter)
				where c.Id == this.ClientId
				join cr in db.ViewClientReports on c.Id equals cr.ClientId

				join sr in db.SubReports.Where(permissions.SubReportsFilter) on cr.SubReportId equals sr.Id
				where sr.AppBudgetService.Service.TypeId != (int)Service.ServiceTypes.EmergencyAssistance
				where sr.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.Homecare
					&& sr.AppBudgetService.Service.ReportingMethodId != (int)Service.ReportingMethods.HomecareWeekly
				let clientReportsCount = sr.ClientReports.Count()
				select new ostr
				{
					MrStart = sr.MainReport.Start,
					MrEnd = (DateTime)System.Data.Objects.EntityFunctions.AddDays(sr.MainReport.End, -1),
					Quantity = cr.Quantity,
					Amount = cr.TotalAmount ,
					EstAmount = clientReportsCount == 0 ? (decimal?)null : sr.Amount / clientReportsCount,
					Cur = sr.AppBudgetService.AppBudget.App.CurrencyId,
					ServiceId = sr.AppBudgetService.ServiceId,
					ServiceName = sr.AppBudgetService.Service.Name,
					ServiceTypeName = sr.AppBudgetService.Service.ServiceType.Name,
					AppName = sr.AppBudgetService.AppBudget.App.Name,
					FundName = sr.AppBudgetService.AppBudget.App.Fund.Name,
                    MasterFundName = sr.AppBudgetService.AppBudget.App.Fund.MasterFund.Name,
					SubReportId = sr.Id,
                    MasterFundId = sr.AppBudgetService.AppBudget.App.Fund.MasterFundId
				};
			var filtered = source;
			if (StartDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date >= this.StartDate);
			}
			if (EndDate.HasValue)
			{
				filtered = filtered.Where(f => f.Date <= this.EndDate);
			}
			if (this.ServiceId.HasValue)
			{
				filtered = filtered.Where(f => f.ServiceId == this.ServiceId);
			}
			if (this.sSortCol_0 == "DateName") { this.sSortCol_0 = "Date"; }
			var sorted = filtered.OrderByField(this.sSortCol_0, this.sSortDir_0 == "asc");

			var data = sorted.Skip(this.iDisplayStart).Take(this.iDisplayLength).ToList();

			return new jQueryDataTableResult
			{
				sEcho = this.sEcho,
				aaData = data,
				iTotalRecords = source.Count(),
				iTotalDisplayRecords = filtered.Count()

			};


		}

	}
	class ostr : hctr
	{
		public override string DateName
		{
			get
			{
				return this.Date.HasValue ? this.Date.Value.ToShortDateString() : null;
			}
		}

		public DateTime MrStart { get; set; }

		public DateTime MrEnd { get; set; }

		public decimal? EstAmount { get; set; }

        public int MasterFundId { get; set; }
	}
}