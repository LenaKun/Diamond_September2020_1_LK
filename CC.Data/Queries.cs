using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CC.Data
{
	public class Queries
	{

		#region Emergency Caps
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///		all amounts are in usd
		/// </remarks>
		public static
		Func<IQueryable<EmergencyCap>, IQueryable<Client>, IQueryable<ClientReport>, int, IQueryable<EmergencyCapValidationResult>> EmergencyCapSummary
		{
			get
			{
				return
				(IQueryable<EmergencyCap> EmergencyCaps, IQueryable<Client> Clients, IQueryable<ClientReport> ClientReports, int mainReportId) =>

					from cmr in //gets the clients from current report
						(
							from a in
								_EcvSubQuery(EmergencyCaps, mainReportId)
							where a.MainReportId == mainReportId
							group a by new { ecid = a.ec.Id, ClientId = a.ClientId, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								ecId = ag.Key.ecid,
								ClientId = ag.Key.ClientId,
								ClientMasterId = ag.Key.ClientMasterId,
							}
						)
					join dca in //gets client-emergencycaps for the duplicate client contribution
						(
							from a in
								_EcvSubQuery(EmergencyCaps, mainReportId)
							group a by new { ecid = a.ec.Id, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								ecId = ag.Key.ecid,
								ClientMasterId = ag.Key.ClientMasterId,
								TotalAmount = ag.Sum(f => f.Amount),
								Discretionary = ag.Sum(f => f.Discretionary),
							}
						) on new { ClientMasterId = cmr.ClientMasterId, EcId = cmr.ecId } equals new { ClientMasterId = dca.ClientMasterId, EcId = dca.ecId }
					join ec in EmergencyCaps on cmr.ecId equals ec.Id
					join c in Clients on cmr.ClientId equals c.Id
					select new EmergencyCapValidationResult()
					{
						EmergencyCapId = cmr.ecId,
						ClientId = cmr.ClientId,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						CapName = ec.Name,
						CapPerPerson = ec.CapPerPerson,
						DiscretionaryPercentage = ec.DiscretionaryPercentage,
						TotalAmount = dca.TotalAmount,
						Discretionary = (dca.Discretionary ?? 0),
						Cur = ec.CurrencyId
					};
			}
		}

		public class EmergencyCapValidationResult
		{
			public int EmergencyCapId { get; set; }
			public int ClientId { get; set; }
			public string ClientFirstName { get; set; }
			public string ClientLastName { get; set; }
			public string CapName { get; set; }
			public decimal CapPerPerson { get; set; }
			public decimal DiscretionaryPercentage { get; set; }
			public decimal TotalAmount { get; set; }
			public decimal Discretionary { get; set; }
			public string Cur { get; set; }

		}

		/// <summary>
		/// emergency caps validation
		/// intermediate query - joins clientreport with emergency caps
		/// </summary>
		private static Func<IQueryable<EmergencyCap>, int, IQueryable<EcvIntermediate>> _EcvSubQuery
		{
			get
			{

				return (IQueryable<EmergencyCap> EmergencyCaps, int mainReportId) =>
					from ec in EmergencyCaps
					from c in ec.Countries
					from f in ec.Funds
					from ag in c.AgencyGroups
					from app in f.Apps
					from appf in ag.Apps
					where app.Id == appf.Id
					from ab in app.AppBudgets
					from mr in ab.MainReports
					where mr.Id == mainReportId 
						|| mr.StatusId == (int)MainReport.Statuses.Approved 
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse
					from sr in mr.SubReports
					where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.Emergency
					from cr in sr.EmergencyReports
					where cr.ReportDate >= ec.StartDate && cr.ReportDate < ec.EndDate
					select new EcvIntermediate()
					{
						ec = ec,
						EcId = ec.Id,
						ClientId = cr.Client.Id,
						SubReportId = sr.Id,
						MainReportId = mr.Id,
						ClientMasterId = (int)cr.Client.MasterIdClcd,
						Amount = (cr.Amount ?? 0),
						Discretionary = cr.Discretionary,
						cr = cr
					}
				;
			}
		}

		/// <summary>
		/// used in emergency cap validation
		/// encapsulated the return of the sub query
		/// </summary>
		private class EcvIntermediate
		{
			public EmergencyCap ec { get; set; }
			public int EcId { get; set; }
			public int ClientId { get; set; }
			public int MainReportId { get; set; }
			public int SubReportId { get; set; }

			public int ClientMasterId { get; set; }
			public decimal Amount { get; set; }
			public decimal? Discretionary { get; set; }
			public EmergencyReport cr { get; set; }
		}

		#endregion

		#region HCWeekly Caps

		public static Func<IQueryable<HCWeeklyCap>, IQueryable<Client>, IQueryable<ClientReport>, IQueryable<ClientAmountReport>, int, DateTime, IQueryable<HCWeeklyCapValidationResult>> YTDHCWeeklyCapSummary
		{
			get
			{
				return
				(IQueryable<HCWeeklyCap> HCWeeklyCaps, IQueryable<Client> Clients, IQueryable<ClientReport> ClientReports, IQueryable<ClientAmountReport> ClientAmountReports, int mainReportId, DateTime startOfYear) =>

					from cmr in //gets the clients from current report
						(
							from a in
								_YTDHccvSubQuery(HCWeeklyCaps, mainReportId, ClientAmountReports, startOfYear)
							where a.MainReportId == mainReportId
							group a by new { hccid = a.hcc.Id, ClientId = a.ClientId, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								hccId = ag.Key.hccid,
								ClientId = ag.Key.ClientId,
								ClientMasterId = ag.Key.ClientMasterId,
							}
						)
					join dca in //gets client-hcweeklycaps for the duplicate client contribution
						(
							from a in
								_YTDHccvSubQuery(HCWeeklyCaps, mainReportId, ClientAmountReports, startOfYear)
							group a by new { hccid = a.hcc.Id, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								hccId = ag.Key.hccid,
								ClientMasterId = ag.Key.ClientMasterId,
								TotalAmount = ag.Sum(f => f.Amount)
							}
						) on new { ClientMasterId = cmr.ClientMasterId, HccId = cmr.hccId } equals new { ClientMasterId = dca.ClientMasterId, HccId = dca.hccId }
					join hcc in HCWeeklyCaps on cmr.hccId equals hcc.Id
					join c in Clients on cmr.ClientId equals c.Id
					select new HCWeeklyCapValidationResult()
					{
						HCWeeklyCapId = cmr.hccId,
						ClientId = cmr.ClientId,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						AgencyId = c.AgencyId,
						AgencyName = c.Agency.Name,
						CapName = hcc.Name,
						CapPerPerson = hcc.CapPerPerson,
						TotalAmount = dca.TotalAmount,
						Cur = hcc.CurrencyId
					};
			}
		}

		public class HCWeeklyCapValidationResult
		{
			public int HCWeeklyCapId { get; set; }
			public int ClientId { get; set; }
			public string ClientFirstName { get; set; }
			public string ClientLastName { get; set; }
			public string CapName { get; set; }
			public decimal CapPerPerson { get; set; }
			public decimal TotalAmount { get; set; }
			public string Cur { get; set; }
			public int AgencyId { get; set; }
			public string AgencyName { get; set; }
		}
		/// <summary>
		/// homecare caps validation
		/// intermediate query - joins clientreport with homecare caps
		/// </summary>
		private static Func<IQueryable<HCWeeklyCap>, int, IQueryable<ClientAmountReport>, DateTime, IQueryable<HccvIntermediate>> _YTDHccvSubQuery
		{
			get
			{
				return (IQueryable<HCWeeklyCap> HCWeeklyCaps, int mainReportId, IQueryable<ClientAmountReport> ClientAmountReports, DateTime startOfYear) =>
					from hcc in HCWeeklyCaps
					from c in hcc.Countries
					from f in hcc.Funds
					from ag in c.AgencyGroups
					from app in f.Apps
					from appf in ag.Apps
					where app.Id == appf.Id
					from ab in app.AppBudgets
					from mr in ab.MainReports
					where mr.Id == mainReportId
						|| mr.StatusId == (int)MainReport.Statuses.Approved
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse
					where hcc.StartDate < mr.End && hcc.EndDate > startOfYear && hcc.EndDate > mr.Start
					from sr in mr.SubReports
					where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.HomecareWeekly
					from cr in sr.ClientReports
					join cra in ClientAmountReports on cr.Id equals cra.ClientReportId
					where cra.ReportDate >= startOfYear && cra.ReportDate < mr.End
					select new HccvIntermediate()
					{
						hcc = hcc,
						HccId = hcc.Id,
						ClientId = cr.Client.Id,
						SubReportId = sr.Id,
						MainReportId = mr.Id,
						ClientMasterId = (int)cr.Client.MasterIdClcd,
						Amount = cra.Quantity * (cr.Rate ?? 0),
						cr = cr,
						cra = cra
					}
				;
			}
		}

		/// <summary>
		/// used in homecare cap validation
		/// encapsulated the return of the sub query
		/// </summary>
		private class HccvIntermediate
		{
			public HCWeeklyCap hcc { get; set; }
			public int HccId { get; set; }
			public int ClientId { get; set; }
			public int MainReportId { get; set; }
			public int SubReportId { get; set; }
			public int ClientMasterId { get; set; }
			public decimal Amount { get; set; }
			public ClientReport cr { get; set; }
			public ClientAmountReport cra { get; set; }
		}

		#endregion

		#region Mhm Caps
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		///		all amounts are in usd
		/// </remarks>
		public static
		Func<IQueryable<MhmCap>, IQueryable<Client>, IQueryable<ClientReport>, int, IQueryable<MhmCapValidationResult>> MhmCapSummary
		{
			get
			{
				return
				(IQueryable<MhmCap> MhmCaps, IQueryable<Client> Clients, IQueryable<ClientReport> ClientReports, int mainReportId) =>

					from cmr in //gets the clients from current report
						(
							from a in
								_MhmcvSubQuery(MhmCaps, mainReportId)
							where a.MainReportId == mainReportId
							group a by new { mhmcid = a.mhmc.Id, ClientId = a.ClientId, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								mhmcId = ag.Key.mhmcid,
								ClientId = ag.Key.ClientId,
								ClientMasterId = ag.Key.ClientMasterId,
							}
						)
					join dca in //gets client-mhmcaps for the duplicate client contribution
						(
							from a in
								_MhmcvSubQuery(MhmCaps, mainReportId)
							group a by new { mhmcid = a.mhmc.Id, ClientMasterId = a.ClientMasterId } into ag
							select new
							{
								mhmcId = ag.Key.mhmcid,
								ClientMasterId = ag.Key.ClientMasterId,
								TotalAmount = ag.Sum(f => f.Amount)
							}
						) on new { ClientMasterId = cmr.ClientMasterId, MhmcId = cmr.mhmcId } equals new { ClientMasterId = dca.ClientMasterId, MhmcId = dca.mhmcId }
					join mhmc in MhmCaps on cmr.mhmcId equals mhmc.Id
					join c in Clients on cmr.ClientId equals c.Id
					select new MhmCapValidationResult()
					{
						MhmCapId = cmr.mhmcId,
						ClientId = cmr.ClientId,
						ClientFirstName = c.FirstName,
						ClientLastName = c.LastName,
						CapName = mhmc.Name,
						CapPerPerson = mhmc.CapPerPerson,
						TotalAmount = dca.TotalAmount,
						Cur = mhmc.CurrencyId
					};
			}
		}

		public class MhmCapValidationResult
		{
			public int MhmCapId { get; set; }
			public int ClientId { get; set; }
			public string ClientFirstName { get; set; }
			public string ClientLastName { get; set; }
			public string CapName { get; set; }
			public decimal CapPerPerson { get; set; }
			public decimal TotalAmount { get; set; }
			public string Cur { get; set; }
		}

		/// <summary>
		/// mhm caps validation
		/// intermediate query - joins clientreport with mhm caps
		/// </summary>
		private static Func<IQueryable<MhmCap>, int, IQueryable<MhmcvIntermediate>> _MhmcvSubQuery
		{
			get
			{

				return (IQueryable<MhmCap> MhmCaps, int mainReportId) =>
					from mhmc in MhmCaps
					from c in mhmc.Countries
					from f in mhmc.Funds
					from ag in c.AgencyGroups
					from app in f.Apps
					from appf in ag.Apps
					where app.Id == appf.Id
					from ab in app.AppBudgets
					from mr in ab.MainReports
					where mr.Id == mainReportId
						|| mr.StatusId == (int)MainReport.Statuses.Approved
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramAssistantApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingProgramOfficerApproval
						|| mr.StatusId == (int)MainReport.Statuses.AwaitingAgencyResponse
					from sr in mr.SubReports
					where sr.AppBudgetService.Service.ReportingMethodId == (int)Service.ReportingMethods.ClientNamesAndCosts && sr.AppBudgetService.Service.TypeId == 12 /* Minor Home Modifications Service Type */
					from cr in sr.ClientReports
					where mr.End > mhmc.StartDate && mr.Start <= mhmc.EndDate
					select new MhmcvIntermediate()
					{
						mhmc = mhmc,
						MhmcId = mhmc.Id,
						ClientId = cr.Client.Id,
						SubReportId = sr.Id,
						MainReportId = mr.Id,
						ClientMasterId = (int)cr.Client.MasterIdClcd,
						Amount = (cr.Amount ?? 0),
						cr = cr
					}
				;
			}
		}

		/// <summary>
		/// used in mhm cap validation
		/// encapsulated the return of the sub query
		/// </summary>
		private class MhmcvIntermediate
		{
			public MhmCap mhmc { get; set; }
			public int MhmcId { get; set; }
			public int ClientId { get; set; }
			public int MainReportId { get; set; }
			public int SubReportId { get; set; }

			public int ClientMasterId { get; set; }
			public decimal Amount { get; set; }
			public ClientReport cr { get; set; }
		}

		#endregion


		#region ServiceConstraints

		public static IQueryable<ServiceConstraint> GetServiceConstraints(ccEntities db, int appId)
		{
			var q = from app in db.Apps
					where app.Id == appId
					from service in app.Services
					select new
					{
						FundId = app.FundId,
						ServiceId = service.Id
					};

			var qq = from s in q
					 join c in db.ServiceConstraints.Where(f => f.FundId == null) on s.ServiceId equals c.ServiceId into cg
					 from c in cg.DefaultIfEmpty()
					 join fc in db.ServiceConstraints on new { ServiceId = (int?)s.ServiceId, FundId = (int?)s.FundId }
					 equals new { ServiceId = fc.ServiceId, FundId = fc.FundId } into fcg
					 from fc in fcg.DefaultIfEmpty()
					 where fc != null || c != null
					 select fc ?? c;

			return qq;
		}
		public static IQueryable<ServiceConstraint> GetServiceTypeConstraints(ccEntities db, int appId)
		{
			var q = from item in
						(from app in db.Apps
						 where app.Id == appId
						 from service in app.Services
						 select new
						 {
							 FundId = app.FundId,
							 ServiceTypeId = service.TypeId
						 })
					group item by item into itemsGroup
					select itemsGroup.Key;

			var qq = from s in q
					 join c in db.ServiceConstraints.Where(f => f.FundId == null) on s.ServiceTypeId equals c.ServiceTypeId into cg
					 from c in cg.DefaultIfEmpty()
					 join fc in db.ServiceConstraints on new { ServiceTypeId = (int?)s.ServiceTypeId, FundId = (int?)s.FundId }
					 equals new { ServiceTypeId = fc.ServiceTypeId, FundId = fc.FundId } into fcg
					 from fc in fcg.DefaultIfEmpty()
					 where fc != null || c != null
					 select fc ?? c;

			return qq;
		}




		#endregion


		public struct KeyValue<T1, T2>
		{
			public T1 Key { get; set; }
			public T2 Value { get; set; }
		}
	}
}
