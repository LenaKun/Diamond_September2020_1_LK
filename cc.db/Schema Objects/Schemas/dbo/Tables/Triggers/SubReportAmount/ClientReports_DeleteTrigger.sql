CREATE TRIGGER [ClientReports_DeleteTrigger]
	ON [dbo].[ClientReports]
	WITH EXECUTE AS CALLER
	AFTER DELETE
	AS
	BEGIN

		declare @t dbo.singleIntCol;

		insert into @t(Id)
		select distinct deleted.SubReportId
		from deleted

		exec dbo.spStoreSubReportAmounts @t

	END