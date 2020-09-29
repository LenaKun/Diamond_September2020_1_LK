CREATE FUNCTION [dbo].[fnMainReportSubmitted]
(
	@StatusId int
)
RETURNS	BIT 
AS
BEGIN
	return case 
		when @StatusId in (1, 4, 16, 64) then 0 
		when @statusid in (2, 8, 32, 128) then 1 
		else null end;
END
