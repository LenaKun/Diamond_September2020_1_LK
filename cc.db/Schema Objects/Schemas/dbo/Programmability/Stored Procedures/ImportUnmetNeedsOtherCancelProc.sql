CREATE PROCEDURE [dbo].[ImportUnmetNeedsOtherCancelProc]
	@ImportId uniqueidentifier
AS
	delete i
	from ImportUnmetNeedsOther as i
	where i.ImportId = @ImportId

	select @@ROWCOUNT
RETURN 0