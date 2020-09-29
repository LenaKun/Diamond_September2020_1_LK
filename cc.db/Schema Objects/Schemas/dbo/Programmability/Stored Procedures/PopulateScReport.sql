CREATE PROCEDURE [dbo].[PopulateScReport]
	@subReportId int
AS

	delete from dbo.SupportiveCommunitiesReports
	where SubReportId = @subReportId

	
	insert into dbo.SupportiveCommunitiesReports(SubReportId, ClientId, HoursHoldCost, MonthsCount, Amount)
	select SubReportId, ClientId, HoursHoldCost, MonthsCount, Amount
	from dbo.viewScRepSource where SubReportId = @subReportId

	select @@rowcount

RETURN @@rowcount
