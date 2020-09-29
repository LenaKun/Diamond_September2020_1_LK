CREATE PROCEDURE [dbo].[DeleteProgramCostsReport]

@SubReportId int  

	
AS

DELETE FROM ProgramCosts
WHERE SubReportId = @SubReportId;

select @@ROWCOUNT
RETURN 0


