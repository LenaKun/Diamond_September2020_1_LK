CREATE TRIGGER [ClientReports_InsertTrigger]
	ON [dbo].[ClientReports]
	WITH EXECUTE AS CALLER
	AFTER INSERT
	AS
	BEGIN

		declare @t dbo.singleIntCol;

		insert into @t(Id)
		select distinct inserted.SubReportId
		from inserted

		exec dbo.spStoreSubReportAmounts @t

	END