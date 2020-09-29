CREATE PROCEDURE [dbo].[DeleteMedicalEquipmentReport]

@SubReportId int  

	
AS

DELETE FROM MedicalEquipmentReports
WHERE SubReportId = @SubReportId;

select @@ROWCOUNT
RETURN 0


