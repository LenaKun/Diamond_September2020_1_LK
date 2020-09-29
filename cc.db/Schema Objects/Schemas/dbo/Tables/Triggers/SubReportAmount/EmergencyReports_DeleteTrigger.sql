CREATE TRIGGER [EmergencyReports_DeleteTrigger]
	ON [dbo].[EmergencyReports]
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