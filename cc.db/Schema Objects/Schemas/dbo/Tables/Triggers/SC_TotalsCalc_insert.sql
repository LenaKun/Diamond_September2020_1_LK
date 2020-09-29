CREATE TRIGGER [SC_TotalsCalc_insert]
	ON [dbo].[SupportiveCommunitiesReports]
	FOR INSERT
	AS
	BEGIN
		SET NOCOUNT ON
		if update(hoursholdcost)
		begin
			update t set amount = s.Amount, MonthsCount = COALESCE(i.MonthsCount ,s.MonthsCount)
			from SupportiveCommunitiesReports as t
			join inserted as i on t.id = i.id
			join viewScRepSource as s on t.ClientId = s.ClientId and t.SubReportId = s.SubReportId
			where i.Amount is null or i.MonthsCount is null
		end
	END
