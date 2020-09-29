CREATE FUNCTION [dbo].[fnHcCapsTableRaw]
(
	@checkPeriodStart date,
	@checkPeriodEnd date
)
RETURNS table
AS
return
(
	select *
    from HcCapsTableRaw as t where (EndDate is null or StartDate < EndDate) and (EndDate is null or EndDate > @checkPeriodStart) and StartDate < @checkPeriodEnd
)
go
