CREATE TRIGGER [ClientReports_UpdateTrigger]
	ON [dbo].[ClientReports]
	WITH EXECUTE AS CALLER
	AFTER UPDATE
	AS
	BEGIN

		declare @t dbo.singleIntCol;

		insert into @t(Id)
		select distinct inserted.SubReportId
		from inserted

		exec dbo.spStoreSubReportAmounts @t

	END