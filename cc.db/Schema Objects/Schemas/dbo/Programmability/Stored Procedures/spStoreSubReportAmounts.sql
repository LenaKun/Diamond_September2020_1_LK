CREATE PROCEDURE [dbo].[spStoreSubReportAmounts]
	@subreportIds dbo.singleIntCol readonly
AS
	/*
	Stores total reported amount on the subreport level in order to reduce cpu loads on queries that sums total amounts
	@subreportIds contains the ids of the subreports for which the total amount should be recalculated
	*/
	/*
	this procedure must be called whenever there are changes in one of the clientreport level tables (clientreports, clientamountreports etc.)
	which is done throug the triggers on these tables
	*/
	update t set Amount = t2.Amount
	from dbo.SubReports as t
	join @subreportIds as t1 on t.id = t1.Id
	join dbo.viewSubreportAmounts as t2 on t1.Id = t2.id
	join dbo.AppBudgetServices as bs on t.AppBudgetServiceId = bs.Id
	join dbo.services as s on bs.ServiceId = s.Id
	where s.ReportingMethodId not in (2,3,8,15)
RETURN 0
