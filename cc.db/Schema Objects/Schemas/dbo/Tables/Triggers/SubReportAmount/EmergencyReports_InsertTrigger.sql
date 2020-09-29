CREATE TRIGGER [EmergencyReports_InsertTrigger]
	ON [dbo].[EmergencyReports]
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