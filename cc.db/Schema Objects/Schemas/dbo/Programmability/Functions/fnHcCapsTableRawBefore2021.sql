CREATE FUNCTION [dbo].[fnHcCapsTableRawBefore2021]
(
	@checkPeriodStart date,
	@checkPeriodEnd date
)
RETURNS table
AS
return
(
	select *
    from HcCapsTableRaw_Original as t where (EndDate is null or StartDate < EndDate) and (EndDate is null or EndDate > @checkPeriodStart) and StartDate < @checkPeriodEnd
)
go
