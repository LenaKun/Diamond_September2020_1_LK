CREATE TRIGGER [SC_Clients_Sync]
	ON [dbo].[Clients]
	FOR DELETE, INSERT, UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		if update(LeaveDate) or update(JoinDate)
		begin

			;with reports as (
				select r2.*
				from inserted as r1
				join viewScRepSource as r2 on r1.Id = r2.ClientId
				where dbo.fnMainReportSubmitted(r2.MrStatusId) = 0
			)
			merge supportivecommunitiesReports as t
			using reports as s on t.clientid = s.clientid and t.subreportid = s.subreportid
			when not matched by target then
			insert (SubReportId, ClientId, MonthsCount, Amount)
			values (S.SubReportId, S.ClientId, S.MonthsCount, s.Amount);

		END
	END
