CREATE VIEW [dbo].[viewFinancialReportApprovalStatusReport]
	AS 
	
	SELECT mr.Id as 'MainReportId', c.RegionId as 'RegionId', c.Id as 'CountryId', ag.Id as 'AgencyGroupId', app.Name as 'AppName', mr.Start, mr.[End],
		a.Name as 'AgencyName', mrsa.StatusChangeDate, mrsa.NewStatusId, mrsa.OldStatusId
	 FROM MainReportStatusAudits as mrsa
	 join MainReports as mr on mrsa.MainReportId = mr.Id
	 join AppBudgets as appb on mr.AppBudgetId = appb.Id
	 join Apps as app on appb.AppId = app.Id
	 join AgencyGroups as ag on app.AgencyGroupId = ag.Id
	 join Countries as c on ag.CountryId = c.Id
	 join Agencies as a on ag.Id = a.GroupId
	 WHERE ag.ExcludeFromReports = 0
