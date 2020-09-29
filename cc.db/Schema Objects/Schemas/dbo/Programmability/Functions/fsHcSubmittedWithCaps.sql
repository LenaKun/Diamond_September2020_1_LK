CREATE FUNCTION [dbo].[fsHcSubmittedWithCaps]
(
	@y int
)
RETURNS TABLE AS RETURN
(
select clientid, startdate, enddate, sum(datediff(DAY, hcs, hce) * hccap / 7) as mcap
from
(
	select distinct
		seed.*,
		case when hc.StartDate > seed.startdate then hc.StartDate else seed.startdate end as hcs,
		case when hc.EndDate < seed.enddate then hc.EndDate else seed.enddate end as hce,
		hc.HcCap
	from 
	(
		select c.id as clientid, months.startdate, months.enddate
		from	
			dbo.clients as c
			cross apply
			(
				select startdate, dateadd(month,1,startdate) as enddate
				from
				(
					select DATEFROMPARTS(year(@y), mt.id,1) as startdate
					from dbo.Sequence(1,12,1) as mt
				) as mtt
			) as months
	) as seed

	left outer join hccapstableraw as hc on seed.clientid = hc.clientid and hc.StartDate < seed.enddate and (hc.enddate is null or hc.enddate > seed.startdate)
) as f1
group by  clientid, startdate, enddate

)
