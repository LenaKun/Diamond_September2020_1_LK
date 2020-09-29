CREATE PROCEDURE [dbo].[DeleteSCReport]
	@SubReportId int
AS
	DELETE FROM dbo.SupportiveCommunitiesReports
	WHERE SubReportId = @SubReportId;
	select @@ROWCOUNT
RETURN 0