CREATE PROCEDURE [dbo].[ImportUnmetNeedsCancelProc]
	@ImportId uniqueidentifier
AS
	delete i
	from ImportUnmetNeeds as i
	where i.ImportId = @ImportId

	select @@ROWCOUNT
RETURN 0
