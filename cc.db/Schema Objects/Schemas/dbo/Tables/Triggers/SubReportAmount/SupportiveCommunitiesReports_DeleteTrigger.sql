CREATE TRIGGER [SupportiveCommunitiesReports_DeleteTrigger]
	ON [dbo].[SupportiveCommunitiesReports]
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