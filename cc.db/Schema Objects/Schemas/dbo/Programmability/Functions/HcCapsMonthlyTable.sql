CREATE FUNCTION [dbo].[HcCapsMonthlyTable]
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
		Sum(datediff(DAY, StartDate,EndDate) * HcCap / 7) as HcCap 
	from
	( 
		select AgencyGroupId, clientid, HcCap,
		(select max(mStart.st) from (select dateadd(day, -6, StartDate) as st union
										select dateadd(day, -6, @checkPeriodStart)
									) as mStart) as StartDate,
		(select min(mEnd.et) from	(select dateadd(day, 6, EndDate) as et union
										select dateadd(day, 6, @checkPeriodEnd)
									) as mEnd) as EndDate
		from dbo.HcCapsMonthlyTableRaw
		where (EndDate is null or (DeceasedDate is null and EndDate > @checkPeriodStart) or (DeceasedDate is not null and 
		DATEADD(month, 2, DATEFROMPARTS(year(DeceasedDate), month(DeceasedDate), 1)) > @checkPeriodStart)) and StartDate < @checkPeriodEnd
	) as t
	group by 
		AgencyGroupId,
		ClientId
)
go
