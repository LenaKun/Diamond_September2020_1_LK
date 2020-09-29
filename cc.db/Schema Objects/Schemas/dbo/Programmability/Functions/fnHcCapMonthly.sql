CREATE FUNCTION [dbo].[fnHcCapMonthly]
(
	@ClientId int,
	@sd date
)
RETURNS TABLE AS RETURN
(
	select top 1 * from
	(
		select ClientId, case when sum(datediff(day, hcs, hce) * hccap /7) > 0 then sum(datediff(day, hcs, hce) * hccap /7) else 0 end as HcCap, 
		avg(hccap) as AvgHcCap
		from (
			select clientid,
				case when hcc.StartDate > @sd then hcc.StartDate else @sd end as hcs,
				case when hcc.EndDate < dateadd(month, 1, @sd) then hcc.enddate else dateadd(month, 1, @sd) end as hce,
				HcCap
			from HcCapsMonthlyTableRaw as hcc
			where (StartDate < dateadd(month,1,@sd) and coalesce(enddate, dateadd(month,1,@sd)) > @sd)
		) as t
		where t.ClientId = @ClientId
		group by ClientId
	)as tt
)
