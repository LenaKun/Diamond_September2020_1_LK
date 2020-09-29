CREATE FUNCTION [dbo].[HcCapsTableWithoutCarry]
(
	@checkPeriodStart date,
	@checkPeriodEnd date
)
RETURNS table
AS
return
(
	select 
		ClientId,  
		AgencyGroupId,
		Sum(datediff(DAY, StartDate,EndDate) * HcCap / 7) as HcCap,
		Sum(datediff(DAY, StartDate,EndDate) * GovHcHours / 7) as GovHcCap
	from
	( 
		select distinct AgencyGroupId, clientid, HcCap, GovHcHours,
		(select max(mStart.st) from (select StartDate as st union
										select @checkPeriodStart
									) as mStart) as StartDate,
		(select min(mEnd.et) from	(select EndDate as et union
										select @checkPeriodEnd
									) as mEnd) as EndDate
		from dbo.fnHcCapsTableRaw(@checkPeriodStart, @checkPeriodEnd)
		where (EndDate is null or EndDate > @checkPeriodStart) and StartDate < @checkPeriodEnd
	) as t
	group by 
		AgencyGroupId,
		ClientId
)
go
