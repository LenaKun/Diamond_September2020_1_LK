CREATE PROCEDURE [dbo].[DeleteDCCReport]
	@SubReportId int
AS
	
	delete from dbo.DccMemberVisits
	where SubReportId = @SubReportId
	delete from dbo.DaysCentersReports
	where SubReportId = @SubReportId

	select @@ROWCOUNT
RETURN 0
