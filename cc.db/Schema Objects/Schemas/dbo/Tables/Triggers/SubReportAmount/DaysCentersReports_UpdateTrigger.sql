CREATE TRIGGER [DaysCentersReports_UpdateTrigger]
	ON [dbo].[DaysCentersReports]
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