using System;
using CC.Web.Models.UniqueClientReports;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using CC.Data;
using CC.Web.Models;
using CC.Web.Attributes;
using MvcContrib.ActionResults;
using MvcContrib;
using System.Net.Mail;
using System.Configuration;
using System.Linq.Dynamic;
using OpenXmlPowerTools.SpreadsheetWriter;
using CC.Web.Models.Reporting;
using CC.Web.Helpers;
using System.Data.Objects.SqlClient;

namespace CC.Web.Controllers
{

	[CcAuthorize(FixedRoles.Admin, FixedRoles.GlobalOfficer, FixedRoles.GlobalReadOnly, FixedRoles.AuditorReadOnly)]
	public class ReportingController : PrivateCcControllerBase
	{
		public ReportingController()
			: base()
		{
			db.CommandTimeout = 300;
		}

		public ActionResult Index()
		{
			var model = new ReportingHomeModel();

			return View(model);
		}

		#region FundStatus Report

		public ActionResult ExportFundStatusReportToExcel(ReportingHomeModel.FundStatusReportFilter model)
		{
			TryUpdateModel(model, "FundStatusRepfilter");
			var data = ReportingHomeModel.LoadData(db, this.Permissions, this.Permissions.User, model);
			return this.Excel("FundStatusReport", "FundStatusReport", data);
		}

		#endregion

		#region GG Quarterly Report of In-Home Services Clients

		public ActionResult GgFunctionalityLevels(ReportingHomeModel model)
		{
			var year = model.GgRepFilter.Year;
			var funds = model.GgRepFilter.Funds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
			if (!ModelState.IsValid)
			{
				return View("Index", model);
			}


			var sers = db.AgencyGroups.Where(f => !f.ExcludeFromReports).Where(f => f.Apps.Any(a => funds.Contains(a.FundId))).Select(r => new
			{
				RegionName = r.Country.Region.Name,
				CountryName = r.Country.Name,
				StateName = r.State.Name,
				Name = r.Name,
				Id = r.Id
			}).ToList();

			var source = from sr in db.viewGgQuarterlyHcFls
						 where sr.RepYear == year
						 select sr;

			if (funds.Any())
			{
				source = source.Where(sr => funds.Contains(sr.FundId));
			}

			var rawData = (from sr in source
						   group sr by new
						   {
							   AgencyGroupId = sr.AgencyGroupId,
							   Quarter = sr.RepQuarter,
							   Year = sr.RepYear,
							   RflId = sr.RelatedLevelId
						   } into srg
						   select new
						   {
							   AgencyGroupId = srg.Key.AgencyGroupId,
							   Quarter = srg.Key.Quarter,
							   Year = srg.Key.Year,
							   RflId = srg.Key.RflId,
							   Cc = srg.Count()
						   }).ToList();

			var rfls = db.RelatedFunctionalityLevels.ToList();



			var d1 = from ser in sers
					 join row in rawData.GroupBy(f => f.AgencyGroupId) on ser.Id equals row.Key into agDataGroup
					 from agData in agDataGroup.DefaultIfEmpty()
					 orderby ser.Name
					 select new
					 {
						 s = ser,
						 d = agData
					 };

			var exceldata = new List<Row>();

			exceldata.Add(new Row
			{
				Cells = new[] { 
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell(),
                    new Cell(),
					new Cell{Value="1", CellDataType = CellDataType.String},
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell{Value="2", CellDataType = CellDataType.String},
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell{Value="3", CellDataType = CellDataType.String},
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell(),
					new Cell{Value="4", CellDataType = CellDataType.String},
				}
			});
			var headerCells = new string[] { "Region", "Country", "State", "SER", "SER ID", }.Select(f => new Cell
			{
				Value = f,
				CellDataType = CellDataType.String
			}).ToList();
			foreach (var i in Enumerable.Range(1, 4))
			{
				headerCells.AddRange(rfls.Select(f => new Cell { Value = f.Name, CellDataType = CellDataType.String }));
				headerCells.Add(new Cell { Value = string.Format("Q{0} Total", i), CellDataType = CellDataType.String });
			}
			headerCells.Add(new Cell { Value = "Total", CellDataType = CellDataType.String });
			exceldata.Add(new Row { Cells = headerCells });

			foreach (var cell in headerCells) { cell.Bold = true; }



			foreach (var s in d1)
			{
				var r = new Row() { };
				var cells = new List<Cell>();
				cells.Add(new Cell { Value = s.s.RegionName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = s.s.CountryName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = s.s.StateName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = s.s.Name, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = s.s.Id.ToString(), CellDataType = CellDataType.String });

				var totalCount = 0;
				foreach (var i in Enumerable.Range(1, 4))
				{
					var quarterCount = 0;
					foreach (var rfl in rfls)
					{
						var count = 0;
						if (s.d != null) count = s.d.Where(f => f.RflId == rfl.Id && f.Quarter == i).Sum(f => f.Cc);
						quarterCount += count;


						cells.Add(new Cell
						{
							Value = count,
							CellDataType = CellDataType.Number
						});
					}
					cells.Add(new Cell
					{
						Value = quarterCount,
						CellDataType = CellDataType.Number
					});
					totalCount += quarterCount;
				}
				cells.Add(new Cell
				{
					Value = totalCount,
					CellDataType = CellDataType.Number
				});
				r.Cells = cells;

				exceldata.Add(r);
			}

			return this.Excel("data", "data", exceldata);



		}

		#endregion

		#region Clients With Leave Date Removed

		public ActionResult ExportLeaveDateRemovedToExcel(ReportingHomeModel.LeaveDateRemovedFilter model)
		{
			TryUpdateModel(model, "LeaveDateFilter");
			var data = ReportingHomeModel.LoadLeaveDateData(db, this.Permissions, this.Permissions.User, model);
			return this.Excel("LeaveDateRemovedReport", "LeaveDateRemovedReport", data);
		}

		#endregion

		#region YTD Reporting Service Type Pct

		public ActionResult YtdReportingServiceTypePct(ReportingHomeModel model)
		{
			var filter = model.ReportingServiceTypePctFilter;

			var data = ReportingServiceTypePctHelper.getData(db, Permissions, filter);

            if(data.Any(f => f.Errors != null && f.Errors != ""))
            {
                this.ModelState.AddModelError("", data.FirstOrDefault(f => f.Errors != null && f.Errors != "").Errors);
                return View("Index", model);
            }

			return this.Excel("data", "data", data);

		}

		#endregion

		#region Qtr Reporting Service Type Pct

		public ActionResult QtrReportingServiceTypePct(ReportingHomeModel model)
		{
			var filter = model.QtrReportingServiceTypePctFilter;

			var data = ReportingServiceTypePctHelper.getQtrData(db, Permissions, filter);

            if (data.Any(f => f.Errors != null && f.Errors != ""))
            {
                this.ModelState.AddModelError("", data.FirstOrDefault(f => f.Errors != null && f.Errors != "").Errors);
                return View("Index", model);
            }

			return this.Excel("data", "data", data);

		}

		#endregion

		#region Unique Clients Quarterly Report

		
		public ActionResult ExportUniqueClientsToExcel(UniqueClientsFilter filter)
		{
			db.CommandTimeout = 6000;
			var source = UniqueClientsReportsHelper.select1(filter, db);

			switch (filter.ConsolidationLevel)
			{
				case ConsolidationLevels.Ser:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqSer(source, db));
				case ConsolidationLevels.State:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqState(source, db));
				case ConsolidationLevels.Country:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqCountry(source, db));
				case ConsolidationLevels.Region:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqRegion(source, db));
				case ConsolidationLevels.Service:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqService(source, db));
				case ConsolidationLevels.ServiceType:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqServiceType(source, db));
				case ConsolidationLevels.Fund:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqFund(source, db));
				default:
					return this.Excel("Unique Clients Quarterly Report", "data", UniqueClientsReportsHelper.ucrqFull(source, db));
			}
		}



		#endregion

		#region Unique Clients Annualy Report

		public ActionResult ExportUniqueClientsAnnualyToExcel(UniqueClientsFilter filter)
		{
			db.CommandTimeout = 6000;
			var source = UniqueClientsReportsHelper.select1(filter, db);

			switch (filter.ConsolidationLevel)
			{
				case ConsolidationLevels.Ser:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrSer(source, db));
				case ConsolidationLevels.State:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrState(source, db));
				case ConsolidationLevels.Country:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrCountry(source, db));
				case ConsolidationLevels.Region:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrRegion(source, db));
				case ConsolidationLevels.Service:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrService(source, db));
				case ConsolidationLevels.ServiceType:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrServiceType(source, db));
				case ConsolidationLevels.Fund:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrFund(source, db));
				default:
					return this.Excel("Unique Clients Report", "data", UniqueClientsReportsHelper.ucrFull(source, db));
			}
		}

		public class UniqueClientsAnnualyDataRaw
		{
			public string RegionName { get; set; }
			public string CountryName { get; set; }
			public string StateName { get; set; }
			public string SerName { get; set; }
			public string ServiceTypeName { get; set; }
			public string ServiceName { get; set; }
			public int Year { get; set; }
			public int ClientId { get; set; }
			public int? MasterId { get; set; }
			public int StateId { get; set; }
		}

		#endregion

		#region Homecare Costs Report

		public ActionResult HcCostsToExcel(ReportingHomeModel model)
		{
			var subtotals = model.HcCostsFilter.subtotals;

			int? regionId = model.HcCostsFilter.RegionId;

			var regions = (from reg in (db.Regions.ToList()).AsParallel() select (int)reg.Id).ToList();

			var viewClientReports = db.ViewClientReports;
			var appBudgetServices = db.AppBudgetServices.Where(a => a.ServiceId == 348 || a.ServiceId == 349);
			var preJoinSubReports = db.SubReports.Where(s => s.MainReport.StatusId == 2);
			var agencyGroups = db.AgencyGroups.Where(f => !f.ExcludeFromReports).Where(ag => regionId != null ? ag.Country.RegionId == regionId : regions.Contains(ag.Country.RegionId));

			var subReports = from sr in preJoinSubReports
							 join abs in appBudgetServices on sr.AppBudgetServiceId equals abs.Id

							 select new
							 {
								 Id = sr.Id,
								 Year = sr.MainReport.Start.Year,
								 ServiceId = sr.AppBudgetService.ServiceId,
								 AgencyGroupId = abs.Agency.AgencyGroup.Id,
								 ExcRateUsd = db.viewAppExchangeRates.Where(f => f.AppId == sr.AppBudgetService.AppBudget.AppId && f.ToCur == "USD").Select(f => (decimal?)f.Value).FirstOrDefault(),
								 ExcRateEuro = db.viewAppExchangeRates.Where(f => f.AppId == sr.AppBudgetService.AppBudget.AppId && f.ToCur == "EUR").Select(f => (decimal?)f.Value).FirstOrDefault(),
							 };

			var results = (from b in
							   (from a in
									(from sr in subReports
									 join vcr in viewClientReports on sr.Id equals vcr.SubReportId
									 select new
									 {
										 TotalAmount = vcr.Amount,
										 Year = sr.Year,
										 Hours = vcr.Quantity,
										 ServiceId = sr.ServiceId,
										 AgencyGroupId = sr.AgencyGroupId,
										 ExcRateUsd = sr.ExcRateUsd,
										 ExcRateEuro = sr.ExcRateEuro,
									 })
								group a by new { ServiceId = a.ServiceId, AgencyGroupId = a.AgencyGroupId, Year = a.Year } into ag
								select new
								{
									Year = ag.Key.Year,
									ServiceId = ag.Key.ServiceId,
									AgencyGroupId = ag.Key.AgencyGroupId,
									TotalHours = ag.Sum(f => f.Hours),
									ReportAmountUsd = ag.Sum(f => f.TotalAmount * f.ExcRateUsd),
									ReportAmountEuro = ag.Sum(f => f.TotalAmount * f.ExcRateEuro)
								})
						   join ag in agencyGroups on b.AgencyGroupId equals ag.Id
						   select new
						   {
							   RegionName = ag.Country.Region.Name,
							   CountryName = ag.Country.Name,
							   StateName = ag.State.Name != null ? ag.State.Name : string.Empty,
							   SER = ag.Name,
                               SerId = ag.Id,
							   Year = b.Year,
							   ServiceId = b.ServiceId,
							   TotalHours = b.TotalHours,
							   ReportAmountUsd = b.ReportAmountUsd != null ? b.ReportAmountUsd : 0,
							   ReportAmountEuro = b.ReportAmountEuro != null ? b.ReportAmountEuro : 0
						   }).ToList();

			var grp = results.OrderBy(r => r.RegionName).ThenBy(c => c.CountryName).ThenBy(s => s.StateName).ThenBy(ser => ser.SER).GroupBy(x => new { x.RegionName, x.CountryName, x.StateName, x.SER, x.SerId }).ToList();

			List<decimal?> subtotalAmountNursingUsd = new List<decimal?>();
			List<decimal?> subtotalAmountNursingEuro = new List<decimal?>();
			List<decimal?> subtotalAmountHouskeepingUsd = new List<decimal?>();
			List<decimal?> subtotalAmountHouskeepingEuro = new List<decimal?>();
			List<int> yearList = new List<int>();
			List<int> countSubTotalRowsNU = new List<int>();
			List<int> countSubTotalRowsNE = new List<int>();
			List<int> countSubTotalRowsHKU = new List<int>();
			List<int> countSubTotalRowsHKE = new List<int>();
			for (var i = 2013; i <= DateTime.Now.Year; i++)
			{
				yearList.Add(i);
				subtotalAmountNursingUsd.Add(0);
				subtotalAmountNursingEuro.Add(0);
				subtotalAmountHouskeepingUsd.Add(0);
				subtotalAmountHouskeepingEuro.Add(0);
				countSubTotalRowsNU.Add(0);
				countSubTotalRowsNE.Add(0);
				countSubTotalRowsHKU.Add(0);
				countSubTotalRowsHKE.Add(0);
			}

			var exceldata = new List<Row>();
			var firstRowCells = new string[] { "", "", "", "", "" }.Select(f => new Cell
			{
				Value = f,
				CellDataType = CellDataType.String
			}).ToList();
			for (int i = 0; i < 4; i++)
			{
				switch (i)
				{
					case 0: firstRowCells.Add(new Cell { Value = "* Weighted Average Personal/Nursing Care Cost USD", CellDataType = CellDataType.String });
						break;
					case 1: firstRowCells.Add(new Cell { Value = "* Weighted Average Personal/Nursing Care Cost EURO", CellDataType = CellDataType.String });
						break;
					case 2: firstRowCells.Add(new Cell { Value = "* Weighted Average Chore/Housekeeping Cost USD", CellDataType = CellDataType.String });
						break;
					case 3: firstRowCells.Add(new Cell { Value = "* Weighted Average Chore/Housekeeping Cost EURO", CellDataType = CellDataType.String });
						break;
				}
				for (int j = 0; j < yearList.Count - 1; j++)
				{
					firstRowCells.Add(new Cell());
				}
			}
			exceldata.Add(new Row { Cells = firstRowCells });
			foreach (var cell in firstRowCells) { cell.Bold = true; }
			var headerCells = new string[] { "Region", "Country", "State", "SER", "SER ID", }.Select(f => new Cell
			{
				Value = f,
				CellDataType = CellDataType.String
			}).ToList();
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < yearList.Count; j++)
				{
					headerCells.Add(new Cell { Value = yearList[j].ToString(), CellDataType = CellDataType.String });
				}
			}
			exceldata.Add(new Row { Cells = headerCells });
			foreach (var cell in headerCells) { cell.Bold = true; }

			string lastRegion = grp.Count() > 0 ? grp.FirstOrDefault().Key.RegionName : string.Empty;
			int countTotalRows = 1;

			var us = new RegionInfo("en-US");
			var euro = new RegionInfo("fr-FR");

			foreach (var gr in grp)
			{
				var row = new Row() { };
				var cells = new List<Cell>();

				if (lastRegion != gr.Key.RegionName && subtotals)
				{
					var subRow = new Row() { };
					var subCells = new List<Cell>();

					subCells.Add(new Cell { Value = lastRegion, CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "sub total", CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });
                    subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });

					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < yearList.Count; j++)
						{
							switch (i)
							{
								case 0:
									subtotalAmountNursingUsd[j] = countSubTotalRowsNU[j] > 0 ? subtotalAmountNursingUsd[j] / countSubTotalRowsNU[j] : subtotalAmountNursingUsd[j];
									string valueANU = subtotalAmountNursingUsd[j] > 0 ? us.CurrencySymbol + Math.Round((decimal)subtotalAmountNursingUsd[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueANU, CellDataType = CellDataType.String });
									subtotalAmountNursingUsd[j] = 0;
									countSubTotalRowsNU[j] = 0;
									break;
								case 1:
									subtotalAmountNursingEuro[j] = countSubTotalRowsNE[j] > 0 ? subtotalAmountNursingEuro[j] / countSubTotalRowsNE[j] : subtotalAmountNursingEuro[j];
									string valueANE = subtotalAmountNursingEuro[j] > 0 ? euro.CurrencySymbol + Math.Round((decimal)subtotalAmountNursingEuro[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueANE, CellDataType = CellDataType.String });
									subtotalAmountNursingEuro[j] = 0;
									countSubTotalRowsNE[j] = 0;
									break;
								case 2:
									subtotalAmountHouskeepingUsd[j] = countSubTotalRowsHKU[j] > 0 ? subtotalAmountHouskeepingUsd[j] / countSubTotalRowsHKU[j] : subtotalAmountHouskeepingUsd[j];
									string valueAHU = subtotalAmountHouskeepingUsd[j] > 0 ? us.CurrencySymbol + Math.Round((decimal)subtotalAmountHouskeepingUsd[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueAHU, CellDataType = CellDataType.String });
									subtotalAmountHouskeepingUsd[j] = 0;
									countSubTotalRowsHKU[j] = 0;
									break;
								case 3:
									subtotalAmountHouskeepingEuro[j] = countSubTotalRowsHKE[j] > 0 ? subtotalAmountHouskeepingEuro[j] / countSubTotalRowsHKE[j] : subtotalAmountHouskeepingEuro[j];
									string valueAHE = subtotalAmountHouskeepingEuro[j] > 0 ? euro.CurrencySymbol + Math.Round((decimal)subtotalAmountHouskeepingEuro[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueAHE, CellDataType = CellDataType.String });
									subtotalAmountHouskeepingEuro[j] = 0;
									countSubTotalRowsHKE[j] = 0;
									break;
							}
						}
					}

					lastRegion = gr.Key.RegionName;

					subRow.Cells = subCells;
					exceldata.Add(subRow);
				}

				cells.Add(new Cell { Value = gr.Key.RegionName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.Key.CountryName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.Key.StateName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.Key.SER, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = gr.Key.SerId.ToString(), CellDataType = CellDataType.String });

				decimal? totalAmountHKUsd = 0;
				decimal? totalAmountHKEuro = 0;
				decimal? totalAmountNursingUsd = 0;
				decimal? totalAmountNursingEuro = 0;

				for (int i = 0; i < 4; i++)
				{
					for (int j = 0; j < yearList.Count; j++)
					{
						var thn = gr.Where(f => f.ServiceId == 349 && f.Year == yearList[j]).Sum(s => s.TotalHours);
						var thhk = gr.Where(f => f.ServiceId == 348 && f.Year == yearList[j]).Sum(s => s.TotalHours);
						switch (i)
						{
							case 0:
								totalAmountNursingUsd = gr.Where(f => f.ServiceId == 349 && f.Year == yearList[j]).Sum(s => s.ReportAmountUsd);
								totalAmountNursingUsd = thn > 0 ? totalAmountNursingUsd / thn : 0;
								string valueANU = totalAmountNursingUsd > 0 ? us.CurrencySymbol + Math.Round((decimal)totalAmountNursingUsd, 2).ToString() : "N/A";
								countSubTotalRowsNU[j] += valueANU != "N/A" ? 1 : 0;
								cells.Add(new Cell { Value = valueANU, CellDataType = CellDataType.String });
								subtotalAmountNursingUsd[j] += totalAmountNursingUsd;
								break;
							case 1:
								totalAmountNursingEuro = gr.Where(f => f.ServiceId == 349 && f.Year == yearList[j]).Sum(s => s.ReportAmountEuro);
								totalAmountNursingEuro = thn > 0 ? totalAmountNursingEuro / thn : 0;
								string valueANE = totalAmountNursingEuro > 0 ? euro.CurrencySymbol + Math.Round((decimal)totalAmountNursingEuro, 2).ToString() : "N/A";
								countSubTotalRowsNE[j] += valueANE != "N/A" ? 1 : 0;
								cells.Add(new Cell { Value = valueANE, CellDataType = CellDataType.String });
								subtotalAmountNursingEuro[j] += totalAmountNursingEuro;
								break;
							case 2:
								totalAmountHKUsd = gr.Where(f => f.ServiceId == 348 && f.Year == yearList[j]).Sum(s => s.ReportAmountUsd);
								totalAmountHKUsd = thhk > 0 ? totalAmountHKUsd / thhk : 0;
								string valueAHU = totalAmountHKUsd > 0 ? us.CurrencySymbol + Math.Round((decimal)totalAmountHKUsd, 2).ToString() : "N/A";
								countSubTotalRowsHKU[j] += valueAHU != "N/A" ? 1 : 0;
								cells.Add(new Cell { Value = valueAHU, CellDataType = CellDataType.String });
								subtotalAmountHouskeepingUsd[j] += totalAmountHKUsd;
								break;
							case 3:
								totalAmountHKEuro = gr.Where(f => f.ServiceId == 348 && f.Year == yearList[j]).Sum(s => s.ReportAmountEuro);
								totalAmountHKEuro = thhk > 0 ? totalAmountHKEuro / thhk : 0;
								string valueAHE = totalAmountHKEuro > 0 ? euro.CurrencySymbol + Math.Round((decimal)totalAmountHKEuro, 2).ToString() : "N/A";
								countSubTotalRowsHKE[j] += valueAHE != "N/A" ? 1 : 0;
								cells.Add(new Cell { Value = valueAHE, CellDataType = CellDataType.String });
								subtotalAmountHouskeepingEuro[j] += totalAmountHKEuro;
								break;
						}
					}
				}

				row.Cells = cells;
				exceldata.Add(row);

				if (countTotalRows == grp.Count() && subtotals)
				{
					var subRow = new Row() { };
					var subCells = new List<Cell>();

					subCells.Add(new Cell { Value = lastRegion, CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "sub total", CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });
					subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });
                    subCells.Add(new Cell { Value = "", CellDataType = CellDataType.String });

					for (int i = 0; i < 4; i++)
					{
						for (int j = 0; j < yearList.Count; j++)
						{
							switch (i)
							{
								case 0:
									subtotalAmountNursingUsd[j] = countSubTotalRowsNU[j] > 0 ? subtotalAmountNursingUsd[j] / countSubTotalRowsNU[j] : subtotalAmountNursingUsd[j];
									string valueANU = subtotalAmountNursingUsd[j] > 0 ? us.CurrencySymbol + Math.Round((decimal)subtotalAmountNursingUsd[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueANU, CellDataType = CellDataType.String });
									subtotalAmountNursingUsd[j] = 0;
									break;
								case 1:
									subtotalAmountNursingEuro[j] = countSubTotalRowsNE[j] > 0 ? subtotalAmountNursingEuro[j] / countSubTotalRowsNE[j] : subtotalAmountNursingEuro[j];
									string valueANE = subtotalAmountNursingEuro[j] > 0 ? euro.CurrencySymbol + Math.Round((decimal)subtotalAmountNursingEuro[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueANE, CellDataType = CellDataType.String });
									subtotalAmountNursingEuro[j] = 0;
									break;
								case 2:
									subtotalAmountHouskeepingUsd[j] = countSubTotalRowsHKU[j] > 0 ? subtotalAmountHouskeepingUsd[j] / countSubTotalRowsHKU[j] : subtotalAmountHouskeepingUsd[j];
									string valueAHU = subtotalAmountHouskeepingUsd[j] > 0 ? us.CurrencySymbol + Math.Round((decimal)subtotalAmountHouskeepingUsd[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueAHU, CellDataType = CellDataType.String });
									subtotalAmountHouskeepingUsd[j] = 0;
									break;
								case 3:
									subtotalAmountHouskeepingEuro[j] = countSubTotalRowsHKE[j] > 0 ? subtotalAmountHouskeepingEuro[j] / countSubTotalRowsHKE[j] : subtotalAmountHouskeepingEuro[j];
									string valueAHE = subtotalAmountHouskeepingEuro[j] > 0 ? euro.CurrencySymbol + Math.Round((decimal)subtotalAmountHouskeepingEuro[j], 2).ToString() : "N/A";
									subCells.Add(new Cell { Value = valueAHE, CellDataType = CellDataType.String });
									subtotalAmountHouskeepingEuro[j] = 0;
									break;
							}
						}
					}

					subRow.Cells = subCells;
					exceldata.Add(subRow);
				}

				countTotalRows++;

			}

			return this.Excel("Homecare Costs Report", "Homecare Costs Report", exceldata);
		}

		#endregion

		#region Annual German Government Report (GG Report)
		public ActionResult AnnualGGReport(ReportingHomeModel homeModel)
		{
			var model = homeModel.AnnualGGReportFilter;
			var fundIds = model.FundIds.ToList();
			ModelState.Clear();
			TryValidateModel(model);

			var apps = from a in db.Apps.Where(f => !f.AgencyGroup.ExcludeFromReports)
					   where System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", a.StartDate) == model.Year
					   where fundIds.Contains(a.FundId)
					   select a;




			var uniquClientsCount = (from a in
										 (from cr in db.ViewClientReports
										  join sr in db.SubReports on cr.SubReportId equals sr.Id
										  join c in db.Clients on cr.ClientId equals c.Id
										  join a in apps on sr.AppBudgetService.AppBudget.AppId equals a.Id
										  select new
										  {
											  MasterId = c.MasterIdClcd,
											  AgencyGroupId = sr.AppBudgetService.AppBudget.App.AgencyGroupId,
											  CurId = sr.AppBudgetService.AppBudget.App.CurrencyId,
											  FundCurId = sr.AppBudgetService.AppBudget.App.Fund.CurrencyCode,
											  ServiceId = sr.AppBudgetService.ServiceId
										  })
									 group a by new
									 {
										 AgencyGroupId = a.AgencyGroupId,
										 ServiceId = a.ServiceId,
										 CurId = a.CurId,
										 FundCurId = a.FundCurId
									 } into ag
									 select new
									 {
										 Key = ag.Key,
										 Count = ag.Select(f => f.MasterId).Distinct().Count()
									 }).ToList();

			var source = (from sra in db.viewSubreportAmounts
						  join appbs in db.AppBudgetServices.Where(f => !f.Agency.AgencyGroup.ExcludeFromReports) on sra.AppBudgetServiceId equals appbs.Id
						  let app = appbs.AppBudget.App
						  join ex in db.viewAppExchangeRates on new { AppId = app.Id, CurId = app.Fund.CurrencyCode } equals new { AppId = ex.AppId, CurId = ex.ToCur } into exg
						  from ex in exg.DefaultIfEmpty()
						  select new
						  {
							  AgencyGroupId = appbs.AppBudget.App.AgencyGroupId,
							  CurId = appbs.AppBudget.App.CurrencyId,
							  FundCurId = app.Fund.CurrencyCode,
							  ServiceId = appbs.ServiceId,
							  Amount = sra.Amount,
							  ExRate = ex.Value,
							  BudgetRemarks = appbs.Remarks,
							  AppId = app.Id
						  }).ToList();

			var agencygroups = (from ag in db.AgencyGroups
								join a in apps on ag.Id equals a.AgencyGroupId
								select new
								{
									Id = ag.Id,
									AgencyGroupName = ag.Name,
									CountryName = ag.Country.Name,
									StateName = ag.State.Name,
									City = ag.City,
								}).ToList();

			var services = db.Services.ToList();

			var result = from a in source
						 group a by new
						 {
							 AgencyGroupId = a.AgencyGroupId,
							 ServiceId = a.ServiceId,
							 CurId = a.CurId,
							 FundCurId = a.FundCurId
						 } into agroup
						 join ag in agencygroups on agroup.Key.AgencyGroupId equals ag.Id
						 join s in services on agroup.Key.ServiceId equals s.Id
						 join uc in uniquClientsCount on agroup.Key equals uc.Key
						 select new AnnualGGReportRow
						 {
							 AgencyGroupName = ag.AgencyGroupName,
							 City = ag.City,
							 CountryName = ag.CountryName,
							 ServiceName = s.Name,
							 StateName = ag.StateName,
							 TotalAmount = agroup.Sum(f => f.Amount),
							 TotalCcGrant = agroup.Sum(f => f.Amount * f.ExRate),
							 AppIds = string.Join(",", agroup.Select(f => f.AppId).Distinct()),
							 Remarks = string.Join(",", agroup.Where(f => !string.IsNullOrEmpty(f.BudgetRemarks)).Select(f => f.BudgetRemarks)),
							 UniquClientsCount = uc.Count,
							 CurId = agroup.Key.CurId,
							 FundCurId = agroup.Key.FundCurId
						 };

			return this.Excel("AnnualGGReport", "Data", result.ToList());
		}
		#endregion

		#region Client Hours Report

		public ActionResult ClientHoursToExcel(ReportingHomeModel homeModel)
		{
			if (!ModelState.IsValid)
			{
				return View("Index", homeModel);
			}
			var model = homeModel.ClientHrsFilter;
			DateTime from = new DateTime((int)model.FromYear, (int)model.FromMonth, 1);
			DateTime to = new DateTime((int)model.ToYear, (int)model.ToMonth, 1).AddMonths(1);

			var data = db.sp_Reporting_HcHours(model.RegionId, from, to);

			var total = new sp_Reporting_HcHours_Result()
			{
				CountryName = "Total",
				HcCapHours = 0,
				Quantity = 0,
				UsdAmount = 0,
				EurAmount = 0
			};

			var regionGroups = data.GroupBy(a => a.RegionName)
							   .Select(ag =>
							   {
								   var r = new
								   {
									   key = ag.Key,
									   regionRows = ag.OrderBy(f => f.CountryName).ThenBy(f => f.StateName).ThenBy(f => f.StateName).ThenBy(f => f.AgencyGroupName),
									   regionTotals = new sp_Reporting_HcHours_Result
									   {
										   RegionName = ag.Key,
										   CountryName = "Sub Total",
										   EurAmount = ag.Sum(f => f.EurAmount),
										   HcCapHours = ag.Sum(f => f.HcCapHours),
										   Quantity = ag.Sum(f => f.Quantity),
										   UsdAmount = ag.Sum(f => f.UsdAmount)
									   }
								   };
								   total.HcCapHours += r.regionTotals.HcCapHours ?? 0;
								   total.Quantity += r.regionTotals.Quantity ?? 0;
								   total.UsdAmount += r.regionTotals.UsdAmount ?? 0;
								   total.EurAmount += r.regionTotals.EurAmount ?? 0;
								   return r;
							   });

			var exceldata = new List<Row>(){
				new Row { Cells= new string[] { "Region", "Country", "State", "SER", "SER ID", "HC Hours Capacity", "HC Hours Received", 
					"Addl Hours to Max", "*Weighted Average HC Cost USD", "Addl Need", "*Weighted Average HC Cost EU", "Addl Need" }.Select(f => new Cell
					{
						Value = f,
						CellDataType = CellDataType.String
					})
				}
			};

			foreach (var region in regionGroups)
			{
				foreach (var gr in region.regionRows)
				{
					AddRow(exceldata, gr);
				}
				if (model.subtotals)
				{
					AddRow(exceldata, region.regionTotals);
				}
			}
			AddRow(exceldata, total);

			return this.Excel("Client Hours Report", "Client Hours Report", exceldata);
		}

		private static void AddRow(List<Row> exceldata, sp_Reporting_HcHours_Result gr)
		{
			var cells = new List<Cell>();
			cells.Add(new Cell { Value = gr.RegionName, CellDataType = CellDataType.String });
			cells.Add(new Cell { Value = gr.CountryName, CellDataType = CellDataType.String });
			cells.Add(new Cell { Value = gr.StateName, CellDataType = CellDataType.String });
			cells.Add(new Cell { Value = gr.AgencyGroupName, CellDataType = CellDataType.String });
            cells.Add(new Cell { Value = gr.AgencyGroupId != 0 ? gr.AgencyGroupId.ToString() : "", CellDataType = CellDataType.String });
			cells.Add(new Cell { Value = gr.HcCapHours, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.Quantity, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.AddlQuantity, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.AvgRateUsd, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.AddlUsd, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.AvgRateEur, CellDataType = CellDataType.Number });
			cells.Add(new Cell { Value = gr.AddlEur, CellDataType = CellDataType.Number });
			exceldata.Add(new Row { Cells = cells });
		}

		#endregion

		#region Budget Service Type Percents Report

		public ActionResult BSTPToExcel(ReportingHomeModel model)
		{
			var currId = model.BSTPFilter.DisplayCurrency;
			var ignoreUnsubmitted = model.BSTPFilter.IgnoreUnsubmitted;

			var apps = (from app in db.Apps.Where(f => !f.AgencyGroup.ExcludeFromReports)
						where model.BSTPFilter.Year == null || app.StartDate.Year == model.BSTPFilter.Year
						where model.BSTPFilter.RegionId == null || app.AgencyGroup.Country.RegionId == model.BSTPFilter.RegionId
						where model.BSTPFilter.CountryId == null || app.AgencyGroup.CountryId == model.BSTPFilter.CountryId
						where model.BSTPFilter.StateId == null || app.AgencyGroup.StateId == model.BSTPFilter.StateId
						where model.BSTPFilter.FundId == null || app.FundId == model.BSTPFilter.FundId
						where model.BSTPFilter.AgencyGroupId == null || app.AgencyGroupId == model.BSTPFilter.AgencyGroupId
						select new
						{
							Id = app.Id,
							Name = app.Name,
							CurrencyId = app.CurrencyId,
							CcGrant = app.CcGrant,
							AgencyGroup = app.AgencyGroup,
							Fund = app.Fund,
							Currency = app.Currency
						});

			var appBudgetServices = !ignoreUnsubmitted ? db.AppBudgetServices : db.AppBudgetServices.Where(ab =>AppBudget.EditableStatusIds.Contains(ab.AppBudget.StatusId??0));

			var HcCcGrant = from a in
								(from app in apps
								 join abs in appBudgetServices.Where(ab => ab.Service.TypeId == 8) on app.Id equals abs.AppBudget.AppId
								 select new
								 {
									 AppId = app.Id,
									 HcGrant = abs.CcGrant
								 })
							group a by new { AppId = a.AppId } into ag
							select new
							{
								AppId = ag.Key.AppId,
								Amount = ag.Sum(f => f.HcGrant)
							};

			var AdminCcGrant = from a in
								   (from app in apps
									join abs in appBudgetServices.Where(ab => ab.Service.TypeId == 1) on app.Id equals abs.AppBudget.AppId
									select new
									{
										AppId = app.Id,
										AdminGrant = abs.CcGrant
									})
							   group a by new { AppId = a.AppId } into ag
							   select new
							   {
								   AppId = ag.Key.AppId,
								   Amount = ag.Sum(f => f.AdminGrant)
							   };

			var OtherCcGrant = from a in
								   (from app in apps
									join abs in appBudgetServices.Where(ab => ab.Service.TypeId != 8 && ab.Service.TypeId != 1) on app.Id equals abs.AppBudget.AppId
									select new
									{
										AppId = app.Id,
										OtherGrant = abs.CcGrant
									})
							   group a by new { AppId = a.AppId } into ag
							   select new
							   {
								   AppId = ag.Key.AppId,
								   Amount = ag.Sum(f => f.OtherGrant)
							   };

			var q1 = (from app in apps
						   join hcg in HcCcGrant on app.Id equals hcg.AppId into hcgg
						   from hcg in hcgg.DefaultIfEmpty()
						   join acg in AdminCcGrant on app.Id equals acg.AppId into acgg
						   from acg in acgg.DefaultIfEmpty()
						   join ocg in OtherCcGrant on app.Id equals ocg.AppId into ocgg
						   from ocg in ocgg.DefaultIfEmpty()
						   join abs in appBudgetServices on app.Id equals abs.AppBudget.AppId
						   join aex in db.viewAppExchangeRates on new { AppId = app.Id, CurId = currId } equals new { AppId = aex.AppId, CurId = aex.ToCur } into aexg
						   from aex in aexg.DefaultIfEmpty()
						   select new 
						   {
							   RegionName = app.AgencyGroup.Country.Region.Name,
							   CountryName = app.AgencyGroup.Country.Name,
							   StateName = app.AgencyGroup.State != null ? app.AgencyGroup.State.Name : string.Empty,
							   SerName = app.AgencyGroup.Name,
                               SerId = app.AgencyGroup.Id,
							   FundName = app.Fund.Name,
							   AppName = app.Name,
							   AppAmountDC = app.CcGrant * ((decimal?)aex.Value ?? 1),
							   CurrSelected = currId,
							   AppAmount = Math.Round(app.CcGrant, 2),
							   Curr = app.CurrencyId,
							   Homecare =(decimal?) Math.Round(hcg.Amount, 2),
							   Admin =(decimal?) Math.Round(acg.Amount, 2),
							   Other = (decimal?)Math.Round(ocg.Amount, 2),							   
							   StatusIs = abs.AppBudget.StatusId,
                               Errors = aex == null ? "No exchange rate is defined for the " + currId + " in the app " + app.Name : ""
						   }).ToList();
			
			var results = from item in q1.AsEnumerable()
						  select new BudgetServiceTypePercentsRow{
							  RegionName = item.RegionName,
							  CountryName = item.CountryName,
							  StateName = item.StateName,
							  SerName = item.SerName,
                              SerId = item.SerId,
							  FundName = item.FundName,
							  AppName = item.AppName,
							   AppAmountDC = item.AppAmountDC,
							   CurrSelected = item.CurrSelected,
							   AppAmount = item.AppAmount,
							   Curr = item.Curr,
							   Homecare = item.Homecare??0,
							   Admin = item.Admin??0,
							   Other = item.Other??0,
							   Status =item.StatusIs==null?"N/A": ((AppBudgetApprovalStatuses)item.StatusIs).ToString().SplitCapitalizedWords(),
                               Errors = item.Errors
						  };

            if(results.Any(f => f.Errors != null && f.Errors != ""))
            {
                this.ModelState.AddModelError("", results.FirstOrDefault(f => f.Errors != null && f.Errors != "").Errors);
                return View("Index", model);
            }

			var grpres = results.OrderBy(f => f.RegionName).ThenBy(f => f.CountryName).ThenBy(f => f.StateName).ThenBy(f => f.SerName).ThenBy(f => f.FundName).ThenBy(f => f.AppName).GroupBy(a => new { a.RegionName, a.CountryName, a.StateName, a.SerName, a.SerId, a.FundName, a.AppName }).ToList();

			var exceldata = new List<Row>();
			var headerCells = new string[] { "Region", "Country", "State", "Ser Name", "Ser Id", "Fund", "App", "App Amount (display currency)", "Curr", "App Amount", "Curr", "Homecare", "Other", "Admin", "Homecare %", "Other %", "Admin %", "Status" }.Select(f => new Cell
			{
				Value = f,
				CellDataType = CellDataType.String
			}).ToList();
			exceldata.Add(new Row { Cells = headerCells });
			foreach (var cell in headerCells) { cell.Bold = true; }

			foreach (var gr in grpres)
			{
				var row = new Row() { };
				var cells = new List<Cell>();

				cells.Add(new Cell { Value = gr.FirstOrDefault().RegionName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().CountryName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().StateName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().SerName, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = gr.FirstOrDefault().SerId.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().FundName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().AppName, CellDataType = CellDataType.String });
				var appAmountDC = gr.FirstOrDefault().AppAmountDC > 0 ? Math.Round(gr.FirstOrDefault().AppAmountDC, 2).ToString() : "N/A";
				cells.Add(new Cell { Value = appAmountDC, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().CurrSelected, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().AppAmount.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().Curr, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().Homecare.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().Other.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().Admin.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().HomecarePer, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().OtherPer, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().AdminPer, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = gr.FirstOrDefault().Status, CellDataType = CellDataType.String });

				row.Cells = cells;
				exceldata.Add(row);
			}

			return this.Excel("Budget Service Type Percents", "Budget Service Type Percents", exceldata);

		}

		public class BudgetServiceTypePercentsRow
		{
			public string RegionName { get; set; }
			public string CountryName { get; set; }
			public string StateName { get; set; }
			public string SerName { get; set; }
            public int SerId { get; set; }
			public string FundName { get; set; }
			public string AppName { get; set; }
			public decimal AppAmountDC { get; set; }
			public string CurrSelected { get; set; }
			public decimal AppAmount { get; set; }
			public string Curr { get; set; }
			public decimal Homecare { get; set; }
			public decimal Other { get; set; }
			public decimal Admin { get; set; }
			private decimal getall(){return this.Homecare+this.Other+this.Admin;}
			private string getPercentage(decimal target){
				var all=this.getall();
				if(all==0){
					
					return "N/A";
				}
				else{
					return (target/all).ToString("p");
				}
			}
			public string HomecarePer
			{
				get { return getPercentage(this.Homecare); }
			}
			public string OtherPer { get { return getPercentage(this.Other); } }
			public string AdminPer { get { return getPercentage(this.Admin); } }
			public string Status { get; set; }
            public string Errors { get; set; }
		}

		#endregion

		#region Audit Diagnostics Report

		public ActionResult AuditDiagnosticsToExcel(ReportingHomeModel homeModel)
		{
			if (!ModelState.IsValid)
			{
				return View("Index", homeModel);
			}
			var model = homeModel.auditDiagnosticsFilter;
			DateTime t1 = new DateTime((int)model.FromYear, (int)model.FromMonth, 1);
			DateTime t2 = new DateTime((int)model.ToYear, (int)model.ToMonth, 1).AddMonths(1);

			var clients = from c in db.Clients.Where(f => !f.Agency.AgencyGroup.ExcludeFromReports)
						  where model.AgencyId == null || c.AgencyId == model.AgencyId
						  join a in
							  (
								  from vcr in db.ViewClientReports
								  join sr in db.SubReports.Where(s => s.AppBudgetService.Service.ServiceType.Id == 8) on vcr.SubReportId equals sr.Id
								  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
								  where vcr.ReportDate < t2 && vcr.ReportDate >= t1
								  group vcr by vcr.ClientId into vcrg
								  select vcrg.Key
								  ) on c.Id equals a
						  from f in c.FunctionalityScores
						  where f.StartDate < t2
						  select new
						  {
							  AgencyName = c.Agency.Name,
							  ClientId = c.Id,
							  FirstName = c.FirstName,
							  LastName = c.LastName,
							  DiagnosticScore = f.DiagnosticScore,
							  Date = f.StartDate
						  };

			var result = clients.OrderBy(c => c.ClientId).ThenBy(f => f.Date).Distinct().ToList();

			var exceldata = new List<Row>(){
				new Row { Cells= new string[] { "Agency", "CC ID", "First Name", "Last Name", "Diagnostic Score", "Date" }.Select(f => new Cell
					{
						Value = f,
						CellDataType = CellDataType.String
					})
				}
			};

			//removing all diagnostic scores their period is before the selected period
			int i = 0;
			while (i < result.Count - 1)
			{
				if (result[i].ClientId == result[i + 1].ClientId && result[i + 1].Date <= t1)
				{
					result.Remove(result[i]);
				}
				else
				{
					i++;
				}
			}

			foreach (var r in result)
			{
				var row = new Row() { };
				var cells = new List<Cell>();

				cells.Add(new Cell { Value = r.AgencyName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.ClientId.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.FirstName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.LastName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.DiagnosticScore.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.Date.ToShortDateString(), CellDataType = CellDataType.String });

				row.Cells = cells;
				exceldata.Add(row);
			}

			return this.Excel("Audit Diagnostic Report", "Audit Diagnostic Report", exceldata);
		}

		#endregion

		#region App Budget Report

		public ActionResult AppBudgetToExcel(ReportingHomeModel homeModel)
		{
			if (!ModelState.IsValid)
			{
				return View("Index", homeModel);
			}

			var results = (from app in db.Apps.Where(f => !f.AgencyGroup.ExcludeFromReports)
						   where homeModel.appBudgetFilter.FundId == null || app.FundId == homeModel.appBudgetFilter.FundId
						   where homeModel.appBudgetFilter.AgencyGroupId == null || app.AgencyGroupId == homeModel.appBudgetFilter.AgencyGroupId
						   join ab in db.AppBudgets on app.Id equals ab.AppId
						   where homeModel.appBudgetFilter.StatusId == -1 || ab.StatusId == homeModel.appBudgetFilter.StatusId
						   select new AppBudgetRow
						   {
							   FundName = app.Fund.Name,
							   SerName = app.AgencyGroup.Name,
                               SerId = app.AgencyGroupId,
							   AppName = app.Name,
							   AppAmount = app.CcGrant,
							   Curr = app.CurrencyId,
							   Start = app.StartDate,
							   End = app.EndDate,
							   BudgetStatus = ab.StatusId
						   }).ToList();

			var exceldata = new List<Row>();
			var headerCells = new string[] { "Fund", "Ser", "Ser Id", "App", "CC Grant", "Curr", "Start", "End", "Budget Status" }.Select(f => new Cell
			{
				Value = f,
				CellDataType = CellDataType.String
			}).ToList();
			exceldata.Add(new Row { Cells = headerCells });
			foreach (var cell in headerCells) { cell.Bold = true; }

			foreach (var r in results)
			{
				var row = new Row() { };
				var cells = new List<Cell>();

				cells.Add(new Cell { Value = r.FundName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.SerName, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = r.SerId.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.AppName, CellDataType = CellDataType.String });
				var appAmount = r.AppAmount > 0 ? Math.Round(r.AppAmount, 2).ToString() : "N/A";
				cells.Add(new Cell { Value = appAmount, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.Curr, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.Start.ToString("MM/dd/yyyy"), CellDataType = CellDataType.String });
				var end = r.End.AddDays(-1).ToString("MM/dd/yyyy");
				cells.Add(new Cell { Value = end, CellDataType = CellDataType.String });
				var appBudgetStatus = string.Empty;
				switch (r.BudgetStatus)
				{
					case (int)AppBudgetApprovalStatuses.New: appBudgetStatus = AppBudgetApprovalStatuses.New.ToString();
						break;
					case (int)AppBudgetApprovalStatuses.Approved: appBudgetStatus = AppBudgetApprovalStatuses.Approved.ToString();
						break;
					case (int)AppBudgetApprovalStatuses.AwaitingGlobalPoApproval: appBudgetStatus = AppBudgetApprovalStatuses.AwaitingGlobalPoApproval.DisplayName();
						break;
					case (int)AppBudgetApprovalStatuses.AwaitingRegionalPoApproval: appBudgetStatus = AppBudgetApprovalStatuses.AwaitingRegionalPoApproval.DisplayName();
						break;
					case (int)AppBudgetApprovalStatuses.Cancelled: appBudgetStatus = AppBudgetApprovalStatuses.Cancelled.ToString();
						break;
					case (int)AppBudgetApprovalStatuses.Rejected: appBudgetStatus = AppBudgetApprovalStatuses.Rejected.DisplayName();
						break;
					case (int)AppBudgetApprovalStatuses.ReturnedToAgency: appBudgetStatus = AppBudgetApprovalStatuses.ReturnedToAgency.DisplayName();
						break;
					default: appBudgetStatus = "N/A";
						break;
				}
				cells.Add(new Cell { Value = appBudgetStatus, CellDataType = CellDataType.String });

				row.Cells = cells;
				exceldata.Add(row);
			}

			return this.Excel("App Budget", "App Budget", exceldata);

		}

		public class AppBudgetRow
		{
			public string FundName { get; set; }
			public string SerName { get; set; }
            public int SerId { get; set; }
			public string AppName { get; set; }
			public decimal AppAmount { get; set; }
			public string Curr { get; set; }
			public DateTime Start { get; set; }
			public DateTime End { get; set; }
			public int? BudgetStatus { get; set; }
		}

		#endregion

		#region Post Leave Date Reported

		public ActionResult PLDRToExcel(ReportingHomeModel homeModel)
		{
			if (!ModelState.IsValid)
			{
				return View("Index", homeModel);
			}
			var model = homeModel.PLDRFilter;

			DateTime from = new DateTime();
			DateTime to = new DateTime();

			if (model.FromMonth != null || model.FromYear != null)
			{
				if (model.FromYear != null && model.FromMonth != null)
				{
					from = new DateTime((int)model.FromYear, (int)model.FromMonth, 1);
				}
				else if (model.FromMonth != null)
				{
					from = new DateTime(2012, (int)model.FromMonth, 1);
				}
				else
				{
					from = new DateTime((int)model.FromYear, 1, 1);
				}
			}

			if (model.ToMonth != null || model.ToYear != null)
			{
				if (model.ToYear != null && model.ToMonth != null)
				{
					to = new DateTime((int)model.ToYear, (int)model.ToMonth, 1).AddMonths(1);
				}
				else if (model.ToMonth != null)
				{
					to = new DateTime(to.Year, (int)model.ToMonth, 1).AddMonths(1);
				}
				else
				{
					to = new DateTime((int)model.ToYear, 12, 1).AddMonths(1);
				}
			}

			var clients = from client in db.Clients.Where(f => !f.Agency.AgencyGroup.ExcludeFromReports).Where(c => c.LeaveDate != null || c.DeceasedDate != null)
						  join vcr in db.ViewClientReports on client.Id equals vcr.ClientId
						  join sr in db.SubReports on vcr.SubReportId equals sr.Id
						  join mr in db.MainReports.Where(MainReport.Submitted) on sr.MainReportId equals mr.Id
						  where client.LeaveDate < mr.End
						  join cr in db.ViewClientReports on sr.Id equals cr.SubReportId into crG
						  select new
						  {
							  ClientId = client.Id,
							  SubReportId = sr.Id,
							  AgencyGroupId = client.Agency.AgencyGroup.Id,
							  Amount = vcr.Amount,
							  EstAmount = vcr.Amount / crG.Count(),
							  StartDate = mr.Start,
							  EndDate = mr.End,
							  ReportDate = vcr.ReportDate,
							  Remarks = vcr.PurposeOfGrant,
							  Quantity = vcr.Quantity,
							  DeceasedDate = client.DeceasedDate,
							  LeaveDate = client.LeaveDate
						  };
			#region filters

			if (model.FromMonth != null || model.FromYear != null)
			{
				clients = clients.Where(f => f.StartDate >= from);
			}
			if (model.ToMonth != null || model.ToYear != null)
			{
				clients = clients.Where(f => f.EndDate < to);
			}
			switch (model.TypeId)
			{
				case 1: clients = clients.Where(f => f.DeceasedDate != null);
					break;
				case 2: clients = clients.Where(f => f.DeceasedDate == null);
					break;
			}
			if (model.AgencyGroups != null)
			{
				var sers = model.AgencyGroups.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
				clients = clients.Where(f => sers.Contains(f.AgencyGroupId));
			}

			#endregion

			var result = (from client in clients
						  group client by new
						  {
							  ClientId = client.ClientId,
							  SubreportId = client.SubReportId
						  } into g
						  join sr in db.SubReports on g.Key.SubreportId equals sr.Id
						  join c in db.Clients on g.Key.ClientId equals c.Id
						  select new
						  {
							  ClientId = c.Id,
							  FirstName = c.FirstName,
							  LastName = c.LastName,
							  Ser = c.Agency.AgencyGroup.Name,
							  AgencyName = c.Agency.Name,
                              AgencyId = c.AgencyId,
							  LeaveDate = c.LeaveDate,
							  LeaveReasonId = c.LeaveReasonId,
							  DeceasedDate = c.DeceasedDate,
							  FundName = sr.AppBudgetService.AppBudget.App.Fund.Name,
							  AppName = sr.AppBudgetService.AppBudget.App.Name,
							  StartDate = sr.MainReport.Start,
							  EndDate = sr.MainReport.End,
							  ServiceType = sr.AppBudgetService.Service.ServiceType.Name,
							  ServiceName = sr.AppBudgetService.Service.Name,
							  EstAmount = sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.TotalCostWithListOfClientNames || sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientUnit ? g.Sum(f => f.EstAmount) : null,
							  Amount = g.Sum(f => f.Amount) ?? 0,
							  Remarks = sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.TotalCostWithListOfClientNames || sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientUnit ? g.FirstOrDefault().Remarks : "",
							  HcMonth1 = g.Where(f => f.StartDate == f.ReportDate).FirstOrDefault().Quantity ?? 0,
							  HcMonth2 = g.Where(f => ccEntities.AddMonths(f.StartDate, 1) == f.ReportDate).FirstOrDefault().Quantity ?? 0,
							  HcMonth3 = g.Where(f => ccEntities.AddMonths(f.StartDate, 2) == f.ReportDate).FirstOrDefault().Quantity ?? 0
						  }).OrderBy(c => c.ClientId).ToList();

			var exceldata = new List<Row>(){
				new Row { Cells= new string[] { "CC ID", "First Name", "Last Name", "Ser", "Agency", "Org ID", "Leave Date", "Leave Reason", "Deceased Date", "Fund", "App", "Reporting Start Month + Year", "Reporting End Month + Year", "Service Type", "Service Name", "Average Amount Per Client", "Amount Per Client", "EAP Purpose", "HC Hours Month 1", "HC Hours Month 2", "HC Hours Month 3" }.Select(f => new Cell
					{
						Value = f,
						CellDataType = CellDataType.String
					})
				}
			};

			foreach (var r in result)
			{
				var row = new Row() { };
				var cells = new List<Cell>();

				cells.Add(new Cell { Value = r.ClientId.ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.FirstName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.LastName, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = r.Ser, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.AgencyName, CellDataType = CellDataType.String });
                cells.Add(new Cell { Value = r.AgencyId.ToString(), CellDataType = CellDataType.String });				
				cells.Add(new Cell { Value = r.LeaveDate.ToString(), CellDataType = CellDataType.String });
				var leaveReason = "";
				switch (r.LeaveReasonId)
				{
					case (int)LeaveReasonEnum.Deceased: leaveReason = "Deceased";
						break;
					case (int)LeaveReasonEnum.MovedAway: leaveReason = "Moved Away";
						break;
					case (int)LeaveReasonEnum.NhOah: leaveReason = LeaveReasonEnum.NhOah.DisplayName();
						break;
					case (int)LeaveReasonEnum.Other: leaveReason = "Other";
						break;
					default: leaveReason = "";
						break;
				}
				cells.Add(new Cell { Value = leaveReason, CellDataType = CellDataType.String });
				var deceasedDate = r.DeceasedDate != null ? r.DeceasedDate.ToString() : "N/A";
				cells.Add(new Cell { Value = deceasedDate, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.FundName, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.AppName, CellDataType = CellDataType.String });
				var startDate = (r.StartDate.Month).ToString() + "/" + r.StartDate.Year.ToString();
				cells.Add(new Cell { Value = startDate, CellDataType = CellDataType.String });
				var end = r.EndDate.AddMonths(-1);
				var endDate = (end.Month).ToString() + "/" + end.Year.ToString();
				cells.Add(new Cell { Value = endDate, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.ServiceType, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.ServiceName, CellDataType = CellDataType.String });
				var estAmount = r.EstAmount != null ? Math.Round((decimal)r.EstAmount, 2).ToString() : "N/A";
				cells.Add(new Cell { Value = estAmount, CellDataType = CellDataType.String });
				var amount = r.Amount != 0 ? Math.Round(r.Amount, 2).ToString() : "N/A";
				cells.Add(new Cell { Value = amount, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = r.Remarks, CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = Math.Round(r.HcMonth1, 2).ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = Math.Round(r.HcMonth2, 2).ToString(), CellDataType = CellDataType.String });
				cells.Add(new Cell { Value = Math.Round(r.HcMonth3, 2).ToString(), CellDataType = CellDataType.String });

				row.Cells = cells;
				exceldata.Add(row);
			}

			return this.Excel("Post Leave Date Reported Report", "Post Leave Date Reported Report", exceldata);
		}

		#endregion

		#region AppBallance

		public ActionResult AppBalanceToExcel(ReportingHomeModel homeModel)
		{
			var filter = homeModel.appBalanceFilter;

			var ccgrants = from appbudget in db.AppBudgets.Where(f => !f.App.AgencyGroup.ExcludeFromReports)
						   from appbudgetService in appbudget.AppBudgetServices.Where(f => !f.Agency.AgencyGroup.ExcludeFromReports)
						   group appbudgetService by appbudget.AppId into g
						   select new
						   {
							   AppId = g.Key,
							   CcGrant = g.Sum(f => f.CcGrant)
						   };

			var submittedAmounts = from a in
									   (from mr in db.MainReports.Where(MainReport.Submitted).Where(f => !f.AppBudget.App.AgencyGroup.ExcludeFromReports)
										join sra in db.viewSubreportAmounts on mr.Id equals sra.MainReportId into srag
										select new
										{
											AppId = mr.AppBudget.AppId,
											Start = mr.Start,
											End = mr.End,
											Amount = srag.Sum(f => f.Amount),
										})
								   group a by new { a.AppId } into g
								   select new
								   {
									   AppId = g.Key.AppId,
									   Amount = g.Sum(f => f.Amount)
								   };
			var approvedAmounts = from a in
									   (from mr in db.MainReports.Where(f=>f.StatusId == (int)MainReport.Statuses.Approved).Where(f => !f.AppBudget.App.AgencyGroup.ExcludeFromReports)
										join sra in db.viewSubreportAmounts on mr.Id equals sra.MainReportId into srag
										select new
										{
											AppId = mr.AppBudget.AppId,
											Start = mr.Start,
											End = mr.End,
											Amount = srag.Sum(f => f.Amount),
										})
								   group a by new { a.AppId } into g
								   select new
								   {
									   AppId = g.Key.AppId,
									   ApprovedRepsCount = g.Count(),
									   MaxRepStart = g.Max(f => f.Start),
									   MaxRepEnd = g.Max(f => f.End)
								   };

			var q = from app in db.Apps.Where(Permissions.AppsFilter).Where(f => !f.AgencyGroup.ExcludeFromReports)
					where filter.FundId == null || app.FundId == filter.FundId
					where filter.Year == null || System.Data.Objects.SqlClient.SqlFunctions.DatePart("year", app.StartDate) == filter.Year
					join ccgrant in ccgrants on app.Id equals ccgrant.AppId into ccgrantg
					from ccgrant in ccgrantg.DefaultIfEmpty()
					join submittedReports in submittedAmounts on app.Id equals submittedReports.AppId into sregpg
					from submittedReports in sregpg.DefaultIfEmpty()
					join approvedReports in approvedAmounts on app.Id equals approvedReports.AppId into aregpg
					from approvedReports in aregpg.DefaultIfEmpty()
					select new AppBalanceRow
					{
						FundName = app.Fund.Name,
						AppName = app.Name,
						Cur = app.CurrencyId,
						AgencyGroupName = app.AgencyGroup.Name,
                        AgencyGroupId = app.AgencyGroupId,
						CcGrant = (decimal?)ccgrant.CcGrant,
						ApprovedReportsCount = (int?)approvedReports.ApprovedRepsCount,
						LastApprovedReportStart = (DateTime?)approvedReports.MaxRepStart,
						LastApprovedReportEnd = (DateTime?)approvedReports.MaxRepEnd,
						TotalReported = (decimal?)submittedReports.Amount
					};

			return this.Excel("AppBalance", "Sheet1", q);
		}


		public class AppBalanceRow
		{
			[Display(Name = "Fund")]
			public string FundName { get; set; }

			[Display(Name = "SER")]
			public string AgencyGroupName { get; set; }

            [Display(Name = "SER ID")]
            public int AgencyGroupId { get; set; }

			[Display(Name = "App")]
			public string AppName { get; set; }

			[Display(Name = "CC Grant")]
			public decimal? CcGrant { get; set; }

			[Display(Name = "CUR")]
			public string Cur { get; set; }

			[Display(Name = "Approved Reports Count")]
			public int? ApprovedReportsCount { get; set; }

			[Display(Name = "Last Approved Report Start")]
			public DateTime? LastApprovedReportStart { get; set; }

			[Display(Name = "Last Approved Report End")]
			public DateTime? LastApprovedReportEnd { get; set; }

			[Display(Name = "Total  Reported Amount")]
			public decimal? TotalReported { get; set; }

			[Display(Name = "App Balance")]
			public decimal? AppBalance { get { return CcGrant - TotalReported; } }

		}
		#endregion

        #region Financial Report Approval Status Report

        public ActionResult ApprovalStatusReportToExcel(ReportingHomeModel model)
        {
            var filter = model.approvalStatusFilter;

            var result = AutomatedReportsHelper.ApprovalStatusReportHelper(db, Permissions, filter.RegionId, filter.CountryId, filter.AgencyGroupId, filter.DateFrom, filter.DateTo);

            return this.Excel("FinancialReportApprovalStatusReport", "Sheet1", result);
        }

        #endregion

        #region Functionality Score Change Report

        public ActionResult FunctionalityChangeReportToExcel(ReportingHomeModel model)
        {
            var filter = model.functionalityChangeFilter;            

            var result = AutomatedReportsHelper.FunctionalityChangeReportHelper(db, Permissions, filter.RegionId, filter.CountryId, filter.AgencyGroupId);

            return this.Excel("FunctionalityScoreChangeReport", "Sheet1", result);
        }

        #endregion

        #region Deceased Date Entry Report

        public ActionResult DeceasedDateEntryReportToExcel(ReportingHomeModel model)
        {
            var filter = model.ddeFilter;

            var result = AutomatedReportsHelper.DeceasedDateEntryReportHelper(db, Permissions, filter.RegionId, filter.CountryId, filter.AgencyGroupId);

            return this.Excel("DeceasedDateEntryReport", "Sheet1", result);
        }        

        #endregion

        #region HSEAP - Detail Report

        public ActionResult HseapDetailedToExcel(ReportingHomeModel homeModel)
        {
            var filter = homeModel.hseapFilter;

            var source = from er in db.EmergencyReports.Where(Permissions.EmergencyReportsFilter)
                         join sr in db.SubReports on er.SubReportId equals sr.Id
                         join abs in db.AppBudgetServices on sr.AppBudgetServiceId equals abs.Id
                         join ab in db.AppBudgets on abs.AppBudgetId equals ab.Id
                         join app in db.Apps on ab.AppId equals app.Id
                         join fund in db.Funds on app.FundId equals fund.Id
                         where er.ReportDate.Year == filter.Year
                         select new
                         {
                             AgencyId = er.Client.AgencyId,
                             AgencyName = er.Client.Agency.Name,
                             FundId = fund.Id,
                             FundName = fund.Name,
                             AppId = app.Id,
                             AppName = app.Name,
                             StartDate = sr.MainReport.Start,
                             EndDate = sr.MainReport.End,
                             DateOfGrant = er.ReportDate,
                             ClientId = er.ClientId,
                             CatCode = er.EmergencyReportType.Name,
                             PurposeOfGrant = er.Remarks,
                             Amount = er.Amount,
                             TotalAmount = (er.Amount ?? 0) + er.Discretionary,
                             AppCur = app.CurrencyId,
                             MasterFundId = fund.MasterFundId,
                             RegionId = er.Client.Country.RegionId,
                             CountryId = er.Client.CountryId,
                             AgencyGroupId = er.Client.Agency.GroupId
                         };

            #region filter

            if (filter.MasterFunds != null)
            {
                var mf = filter.MasterFunds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => mf.Contains(f.MasterFundId));
            }

            if(filter.Funds != null)
            {
                var funds = filter.Funds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => funds.Contains(f.FundId));
            }

            if (filter.Apps != null)
            {
                var apps = filter.Apps.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => apps.Contains(f.AppId));
            }

            if (filter.Regions != null)
            {
                var regions = filter.Regions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => regions.Contains(f.RegionId));
            }

            if (filter.Countries != null)
            {
                var countries = filter.Countries.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => countries.Contains(f.CountryId ?? 0));
            }

            if (filter.AgencyGroups != null)
            {
                var sers = filter.AgencyGroups.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => sers.Contains(f.AgencyGroupId));
            }

            if (filter.Agencies != null)
            {
                var agencies = filter.Agencies.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                source = source.Where(f => agencies.Contains(f.AgencyId));
            }

            #endregion

            var result = (from s in source.ToList()
                         select new HseadDetailedRaw
                         {
                             AgencyName = s.AgencyName,
                             AgencyId = s.AgencyId,
                             FundName = s.FundName,
                             AppName = s.AppName,
                             MonthFrom = s.StartDate.ToString("MMM"),
                             MonthTo = s.EndDate.AddMonths(-1).ToString("MMM"),
                             DateOfGrant = s.DateOfGrant.ToString("dd MMM yyyy"),
                             ClientId = s.ClientId,
                             CatCode = s.CatCode,
                             PurposeOfGrant = s.PurposeOfGrant,
                             Amount = s.Amount,
                             TotalAmount = s.TotalAmount,
                             AppCur = s.AppCur
                         }).OrderBy(f => f.AgencyName);

            return this.Excel("HSEAPDetailedReport", "Sheet1", result);
        }

        public class HseadDetailedRaw
        {
            [Display(Name = "Agency")]
            public string AgencyName { get; set; }
            [Display(Name = "Org ID")]
            public int? AgencyId { get; set; }
            [Display(Name = "Fund")]
            public string FundName { get; set; }
            [Display(Name = "App #")]
            public string AppName { get; set; }
            [Display(Name = "Month From")]
            public string MonthFrom { get; set; }
            [Display(Name = "Month To")]
            public string MonthTo { get; set; }
            [Display(Name = "DateOfGrant")]
            public string DateOfGrant { get; set; }
            [Display(Name = "CC ID")]
            public int ClientId { get; set; }
            [Display(Name = "CatCode")]
            public string CatCode { get; set; }
            [Display(Name = "PurposeOfGrant")]
            public string PurposeOfGrant { get; set; }
            [Display(Name = "Amount")]
            public decimal? Amount { get; set; }
            [Display(Name = "TotalAmount")]
            public decimal TotalAmount { get; set; }
            [Display(Name = "AppCurrency")]
            public string AppCur { get; set; }
        }

        #endregion

		#region Status Change Report

		public ActionResult StatusChangeReportToExcel(ReportingHomeModel homeModel)
		{
			var filter = homeModel.statusChangeFilter;

			var source = from mrsa in db.MainReportStatusAudits
						 where filter.IncludeApproved || mrsa.MainReport.StatusId != (int)MainReport.Statuses.Approved
						 group mrsa by mrsa.MainReportId into g
						 join mr in db.MainReports on g.Key equals mr.Id
						 where g.Any(f => f.NewStatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval || f.NewStatusId == (int)MainReport.Statuses.AwaitingAgencyResponse || 
							f.NewStatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval || f.NewStatusId == (int)MainReport.Statuses.Approved)
						 let NotSubmittedLatest = g.Where(f => f.NewStatusId == (int)MainReport.Statuses.New || f.NewStatusId == (int)MainReport.Statuses.Rejected || f.NewStatusId == (int)MainReport.Statuses.ReturnedToAgency)
													.OrderByDescending(f => f.StatusChangeDate).FirstOrDefault()
						 select new
						 {
							 RegionId = mr.AppBudget.App.AgencyGroup.Country.RegionId,
							 CountryId = mr.AppBudget.App.AgencyGroup.CountryId,
							 MasterFundName = mr.AppBudget.App.Fund.MasterFund.Name,
							 MasterFundId = mr.AppBudget.App.Fund.Id,
							 FundName = mr.AppBudget.App.Fund.Name,
							 FundId = mr.AppBudget.App.FundId,
							 AgencyGroupName = mr.AppBudget.App.AgencyGroup.Name,
							 AgencyGroupId = mr.AppBudget.App.AgencyGroupId,
							 AppName = mr.AppBudget.App.Name,
							 AppId = mr.AppBudget.AppId,
							 MrStart = mr.Start,
							 MrEnd = mr.End,
							 Year = SqlFunctions.DatePart("year", mr.Start),
							 MrStatusId = mr.StatusId,
							 NotSubmittedLatestDate = NotSubmittedLatest != null ? (DateTime?)NotSubmittedLatest.StatusChangeDate : (DateTime?)null,
							 NotSubmittedStatusId = NotSubmittedLatest != null ? (int?)NotSubmittedLatest.NewStatusId : (int?)null,
							 PAApprovalLatesDate = (DateTime?)g.Where(f => f.NewStatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval).Max(f => f.StatusChangeDate),
							 POApprovalLatestDate = (DateTime?)g.Where(f => f.NewStatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval).Max(f => f.StatusChangeDate),
							 ApprovedLatestDate = (DateTime?)g.Where(f => f.NewStatusId == (int)MainReport.Statuses.Approved).Max(f => f.StatusChangeDate),
						 };

			#region filter

			if(filter.RegionId.HasValue && filter.RegionId != 0)
			{
				source = source.Where(f => f.RegionId == filter.RegionId);
			}

			if(filter.CountryId.HasValue)
			{
				source = source.Where(f => f.CountryId == filter.CountryId);
			}

			if (filter.AgencyGroupId.HasValue)
			{
				source = source.Where(f => f.AgencyGroupId == filter.AgencyGroupId);
			}

			if(filter.MasterFundId.HasValue)
			{
				source = source.Where(f => f.MasterFundId == filter.MasterFundId);
			}

			if(filter.FundId.HasValue)
			{
				source = source.Where(f => f.FundId == filter.FundId);
			}

			if (filter.AppId.HasValue)
			{
				source = source.Where(f => f.AppId == filter.AppId);
			}

			if (filter.Year.HasValue)
			{
				source = source.Where(f => f.Year == filter.Year);
			}

			#endregion

			var result = (from s in source.ToList()
						  select new StatusChangeReportRaw
						  {
							  MasterFundName = s.MasterFundName,
							  FundName = s.FundName,
							  AgencyGroupName = s.AgencyGroupName,
							  AgencyGroupId = s.AgencyGroupId,
							  AppName = s.AppName,
							  MonthFrom = s.MrStart.ToString("MMM-yy"),
							  MonthTo = s.MrEnd.AddDays(-1).ToString("MMM-yy"),
							  CurrentStatus = ((MainReport.Statuses)s.MrStatusId).DisplayName(),
							  NotSubmittedLatestDate = s.NotSubmittedLatestDate.HasValue ? s.NotSubmittedLatestDate.Value.ToString("dd MMM yyyy") : "N/A",
							  NotSubmittedStatus = s.NotSubmittedStatusId.HasValue ? ((MainReport.Statuses)s.NotSubmittedStatusId.Value).DisplayName() : "N/A",
							  PAApprovalLatesDate = s.PAApprovalLatesDate.HasValue ? s.PAApprovalLatesDate.Value.ToString("dd MMM yyyy") : "N/A",
							  POApprovalLatestDate = s.POApprovalLatestDate.HasValue ? s.POApprovalLatestDate.Value.ToString("dd MMM yyyy") : "N/A",
							  ApprovedLatestDate = s.ApprovedLatestDate.HasValue ? s.ApprovedLatestDate.Value.ToString("dd MMM yyyy") : "N/A"
						  }).OrderBy(f => f.MasterFundName);

			return this.Excel("StatusChangeReport", "Sheet1", result);
		}

		public class StatusChangeReportRaw
		{
			[Display(Name = "Master Fund")]
			public string MasterFundName { get; set; }
			[Display(Name = "Fund")]
			public string FundName { get; set; }
			[Display(Name = "Ser")]
			public string AgencyGroupName { get; set; }
			[Display(Name = "Ser ID")]
			public int? AgencyGroupId { get; set; }
			[Display(Name = "App #")]
			public string AppName { get; set; }
			[Display(Name = "Month From")]
			public string MonthFrom { get; set; }
			[Display(Name = "Month To")]
			public string MonthTo { get; set; }
			[Display(Name = "CurrentStatus")]
			public string CurrentStatus { get; set; }
			[Display(Name = "Latest Date of New/Rejected/Returned to Agency")]
			public string NotSubmittedLatestDate { get; set; }
			[Display(Name = "New/Rejected/Returned to Agency")]
			public string NotSubmittedStatus { get; set; }
			[Display(Name = "Latest Date of Awaiting PA Approval Status")]
			public string PAApprovalLatesDate { get; set; }
			[Display(Name = "Latest Date of Awaiting PO Approval Status")]
			public string POApprovalLatestDate { get; set; }
			[Display(Name = "Latest Date of Approval Status")]
			public string ApprovedLatestDate { get; set; }
		}

		#endregion
	}   

}



