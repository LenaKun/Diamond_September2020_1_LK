CREATE PROCEDURE [dbo].[SP_DW_ClientsByAPP]
	@fromDate datetime,
	@toDate datetime
AS
	select distinct a.Id as ORG_ID, ag.Name as Ser, appb.AppId as 'App ID', mr.[Start] as 'Report Period From Date', mr.[End] as 'Report Period To Date',	c.Id as 'CLIENT ID'
	from MainReports mr
	join AppBudgets appb on mr.AppBudgetId = appb.Id
	join SubReports sr on mr.Id = sr.MainReportId
	join ViewClientReports vcr on vcr.SubreportId = sr.Id
	join Clients c on c.Id = vcr.ClientId
	join Agencies a on a.Id = c.AgencyId
	join AgencyGroups ag on a.GroupId = ag.Id
	where (@fromDate is null or mr.[Start] >= @fromDate)
		and (@toDate is null or mr.[End] <= @toDate)
	order by a.Id
RETURN 0
