CREATE TRIGGER [SC_TotalsCalc_update]
	ON [dbo].[SupportiveCommunitiesReports]
	FOR UPDATE
	AS
	BEGIN
		SET NOCOUNT ON
		if update(monthscount)
		begin
			update t set Amount = s.HoursHoldCost * i.MonthsCount
			from SupportiveCommunitiesReports as t
			join inserted as i on t.id = i.id
			join viewScRepSource as s on t.ClientId = s.ClientId and t.SubReportId = s.SubReportId
		end
	END
