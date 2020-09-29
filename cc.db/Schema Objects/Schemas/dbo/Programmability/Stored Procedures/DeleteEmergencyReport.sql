CREATE PROCEDURE [dbo].[DeleteEmergencyReport]

@SubReportId int  

	
AS

DELETE FROM EmergencyReports
WHERE SubReportId = @SubReportId;

select @@ROWCOUNT
RETURN 0



