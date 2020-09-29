CREATE VIEW [dbo].[viewHc]
	AS

	select 
		mr.id as mrid,
		mr.statusid as mrstatus,
		clientid, 
		reportdate, 
		mr.start as mrstart,
		mr.[end] as mrend,
		year(reportdate) as repyear,
		ar.quantity
	from MainReports as mr
			join AppBudgets as appb on mr.appbudgetid = appb.Id
			join SubReports as sr on mr.Id = sr.MainReportId
			join AppBudgetServices as appbs on sr.AppBudgetServiceId = appbs.id
			join Services as s on appbs.ServiceId = s.Id
			join ClientReports as cr on sr.Id = cr.SubReportId
			join ClientAmountReport as ar on cr.Id = ar.ClientReportId
	where	s.EnforceTypeConstraints = 1 and s.TypeId = 8 /* Homecare Service Type */ and s.ReportingMethodId = 5 /* Homecare (monthly) reporting method */
