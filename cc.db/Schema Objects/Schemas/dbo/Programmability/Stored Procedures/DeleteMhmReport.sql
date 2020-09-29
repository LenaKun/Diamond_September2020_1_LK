CREATE PROCEDURE [dbo].[DeleteMhmReport]

@SubReportId int  

	
AS

DELETE FROM MhmReports
WHERE SubReportId = @SubReportId;

select @@ROWCOUNT
RETURN 0


