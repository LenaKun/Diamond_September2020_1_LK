CREATE TRIGGER [ClientAmountReports_UpdateTrigger]
	ON [dbo].[ClientAmountReport]
	WITH EXECUTE AS CALLER
	AFTER UPDATE
AS
BEGIN
	SET NOCOUNT ON

	declare @t dbo.singleIntCol;

	insert into @t(Id)
	select distinct t2.SubReportId
	from inserted as t1
	join clientreports as t2 on t1.ClientReportId = t2.Id

	exec dbo.spStoreSubReportAmounts @t
END
