CREATE PROCEDURE [dbo].[SP_DW_Apps]
	@fromDate datetime,
	@toDate datetime
AS
	select a.Id as ORG_ID, ag.Name as Ser, app.Id as 'App ID', app.StartDate as 'Period From Date', app.EndDate as 'Period To Date', f.Id as 'Fund ID', f.Name as 'Fund Name',
		app.CcGrant as 'App Amount', app.CurrencyId as 'App Amount Currency Code'
	from Agencies a
	join AgencyGroups ag on a.GroupId = ag.Id
	join Apps app on ag.Id = app.AgencyGroupId
	join Funds f on app.FundId = f.Id
	where (@fromDate is null or app.StartDate >= @fromDate)
		and (@toDate is null or app.EndDate <= @toDate)
	order by a.Id
RETURN 0
