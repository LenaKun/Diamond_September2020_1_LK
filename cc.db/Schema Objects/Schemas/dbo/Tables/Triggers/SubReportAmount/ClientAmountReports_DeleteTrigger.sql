CREATE TRIGGER [ClientAmountReports_DeleteTrigger]
	ON [dbo].[ClientAmountReport]
	WITH EXECUTE AS CALLER
	AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON

	declare @t dbo.singleIntCol;

	insert into @t(Id)
	select distinct t2.SubReportId
	from deleted as t1
	join clientreports as t2 on t1.ClientReportId = t2.Id

	exec dbo.spStoreSubReportAmounts @t
END
