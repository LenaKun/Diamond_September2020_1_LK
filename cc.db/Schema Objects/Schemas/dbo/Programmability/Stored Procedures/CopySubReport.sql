CREATE PROCEDURE [dbo].[CopySubReport]
	@AppBudgetServiceId int,
	@SourceMainReportId int,
	@TargetMainReportId int
AS
	declare @datediff int
	select @datediff = DATEDIFF(month, smr.start, tmr.start) 
	from MainReports as smr join
		MainReports as tmr on smr.AppBudgetId = tmr.AppBudgetId
		where smr.Id = @SourceMainReportId and tmr.Id = @TargetMainReportId

	if @datediff is not null
	begin
		merge subreports as target
		using (select * from subreports where appbudgetserviceid = @appbudgetserviceid and mainreportid = @sourcemainreportid) as source
		on target.appbudgetserviceid = source.appbudgetserviceid and target.mainreportid = @targetmainreportid and source.mainreportid = @sourcemainreportid

		when not matched by target then
		insert ([MainReportId]
		  ,[AppBudgetServiceId]
		  ,[TotalHouseholdsServed]
		  ,[ServiceUnits]
		  ,[Amount]
		  ,[MatchingSum]
		  ,[AgencyContribution]
		  ,[OverFlowReason])
		values(@targetMainReportId
			  ,@AppbudgetServiceId
			  ,source.[TotalHouseholdsServed]
			  ,source.[ServiceUnits]
			  ,source.[Amount]
			  ,source.[MatchingSum]
			  ,source.[AgencyContribution]
			  ,source.[OverFlowReason])
		when matched then
		update set [TotalHouseholdsServed] = source.[TotalHouseholdsServed]
				  ,[ServiceUnits] = source.[ServiceUnits]
				  ,[Amount] = source.[Amount]
				  ,[MatchingSum] = source.[MatchingSum]
				  ,[AgencyContribution] = source.[AgencyContribution]
				  ,[OverFlowReason] = source.[OverFlowReason]
		;

	end

RETURN 0
