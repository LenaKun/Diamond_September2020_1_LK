CREATE PROCEDURE [dbo].[ImportGovHcHoursCancelProc]
	@ImportId uniqueidentifier
AS
	
	delete i
	from ImportGovHcHours as i
	where i.ImportId = @ImportId

	select @@ROWCOUNT
RETURN 0
