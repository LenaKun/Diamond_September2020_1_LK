CREATE VIEW [dbo].[viewHcPlus]
	AS
	
	with currentReports as
	(
		select t1.MrId, t1.mrstart, t1.mrend, t1.ClientId, t1.reportdate, t1.q 
		from
		(
			select mrid, mrstart, mrend, clientid, reportdate, sum(quantity) as q
			from viewHc
			where dbo.fnMainReportSubmitted(mrstatus) = 0 
			group by mrid, mrstart, mrend, clientid, reportdate
		) as t1		
	)
	,u as (
		select ClientId, ReportDate, MrId, coalesce(Q,0) + coalesce(SQ,0) as q, cap as C, wcap as WC
		from 
		(
			select * from (
				select seed.*, cr.q, shc.quantity as sq, 
					coalesce(shc.monthlycap, hc.hccap) as cap,
					coalesce(shc.dailycap, hc.avghccap) as wcap
				from (
					select mrid, clientid, mrstart,mrend, DATEFROMPARTS(mryear, mm.m, 1) as reportdate 
					from
					(
						select mrid, clientid, mrstart,mrend,YEAR(mrstart) as mryear
						from currentReports
						group by mrid, clientid, mrstart,mrend, year(mrstart)
					) as i1
					outer apply(
						select max(reportdate) as ReportDate from dbo.hccaps 
						where clientid = i1.clientid and DATEPART(year, ReportDate) = i1.mryear and ReportDate>=i1.mrend
					) as i2
					cross join viewMonths as mm where mm.m <= case when i2.reportdate is null then month(dateadd(month,-1,i1.mrend)) else month(i2.reportdate) end
				) as seed
				left outer join currentReports as cr on seed.clientid = cr.clientid and seed.reportdate = cr.reportdate and cr.mrid = seed.mrid
				left outer join hccaps as shc on seed.clientid = shc.ClientId and seed.reportdate = shc.ReportDate
				outer apply dbo.fnHcCapMonthly(seed.clientid, seed.reportdate) as hc
			) as tt
		) as ttt
	)
	select u.ClientId, u.ReportDate, u.MrId, u.q, u.C, coalesce(u.WC,dd.HcCap) as WC
	from u
	outer apply (
		select top 1 * 
		from HcCapsMonthlyTableRaw
		where clientid = u.clientid and deceaseddate is not null and deceaseddate between dateadd(month,-1,u.reportdate) and reportdate
		order by StartDate desc
	) as dd
