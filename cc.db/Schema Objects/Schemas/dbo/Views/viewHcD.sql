CREATE VIEW [dbo].[viewHcD]
	AS
	with currentReports as
	(
		select t1.MrId, t1.mrstart, t1.mrend, t1.ClientId, t1.reportdate, t1.q , hc.HcCap, hc.AvgHcCap
		from
		(
			select mrid, mrstart, mrend, clientid, reportdate, sum(quantity) as q
			from viewHc
			where dbo.fnMainReportSubmitted(mrstatus) = 0 
			group by mrid, mrstart, mrend, clientid, reportdate
		) as t1
		outer apply dbo.fnHcCapMonthly(t1.clientid, t1.reportdate) as hc
	)
	,u as (
		select ClientId, ReportDate, MrId, cap as C, wcap as WC
		from 
		(
			select * from (
				select seed.*,
					coalesce(shc.monthlycap, cr.hccap) as cap,
					coalesce(shc.dailycap, cr.avghccap) as wcap
				from (
					select mrid, clientid, mrstart,mrend, DATEFROMPARTS(mryear, mm.m, 1) as reportdate 
					from
					(
						select mrid, clientid, mrstart,mrend,YEAR(mrstart) as mryear
						from currentReports
						group by mrid, clientid, mrstart,mrend, year(mrstart)
					) as i1
					outer apply(select top 1 reportdate from dbo.hccaps where clientid = i1.clientid and ReportDate>=i1.mrend) as i2
					cross join viewMonths as mm where mm.m <= case when i2.reportdate is null then month(dateadd(month,-1,i1.mrend)) else month(i2.reportdate) end
				) as seed
				left outer join currentReports as cr on seed.clientid = cr.clientid and seed.reportdate = cr.reportdate and cr.mrid = seed.mrid
				left outer join hccaps as shc on seed.clientid = shc.ClientId and seed.reportdate = shc.ReportDate
			) as tt
		) as ttt
	)
	select u.mrid, u.clientid, u.reportdate, coalesce(cr.q,0) + coalesce(shc.q,0) as q, u.C, u.WC
	from u
	join clients as c1 on u.clientid = c1.Id
	join (
		select mc.MasterId from clients as mc where mc.masterid is not null group by mc.masterid
	) as d on c1.MasterIdClcd = d.MasterId
	left outer join (
		select c.MasterIdClcd as MasterId, t1.reportdate, t1.mrid, sum(t1.q) as q
		from currentReports as t1
		join clients as c on t1.clientid = c.Id
		group by c.MasterIdClcd, t1.reportdate, t1.mrid
	) as cr on d.MasterId = cr.MasterId and u.reportdate = cr.reportdate and cr.mrid = u.mrid
	left outer join (
		select c.MasterIdClcd as MasterId, t1.reportdate, sum(t1.Quantity) as q
		from hccaps as t1 
		join clients as c on t1.clientid = c.Id
		group by c.MasterIdClcd, t1.reportdate
	) as shc on d.MasterId = shc.MasterId and u.reportdate = shc.ReportDate
	