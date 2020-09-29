CREATE VIEW [dbo].[HcCapsMonthlyTableRaw]
AS

	
	with hcs as (
		select t1.ClientId, 
			t1.StartDate, 
			t4.StartDate as EndDate,
			coalesce(t2.Cap, 0) as Cap
		from ClientHcStatuses as t1
		join HcStatuses as t2 on t1.HcStatusId = t2.Id
		outer apply (
			select top 1 startdate from ClientHcStatuses as t3 
			where t3.ClientId = t1.ClientId and t3.StartDate > t1.StartDate
			order by t3.StartDate
		) as t4
	), fs as (
		select 
			s.ClientId, fl.HcHoursLimit, fl.SubstractGovHours, 
			cast(s.StartDate as date) as StartDate, 
			cast((select min(startdate) from FunctionalityScores as e where s.clientid = e.clientid and s.StartDate < e.StartDate) as date) as EndDate
		from FunctionalityScores as s
		join FunctionalityLevels as fl on s.functionalitylevelid = fl.id
	),
	fsc as (
		select * from 
		(
			select 
				fs.ClientId,
				fs.SubstractGovHours,
				(select max(startdate) from (select fs.startdate union select hcs.startdate) as m1) as StartDate,
				(select min(enddate) from (select fs.enddate union select hcs.enddate) as m2) as EndDate,
				case when hcs.Cap <= fs.HcHoursLimit then hcs.Cap
					 when fs.HcHoursLimit <= hcs.Cap then fs.HcHoursLimit
					 else 0 end as HcHours
			from fs
			join hcs on fs.ClientId = hcs.ClientId and (
				(fs.EndDate is null and hcs.EndDate is null) or
				(fs.EndDate is null and fs.startdate < hcs.EndDate) or
				(hcs.EndDate is null and hcs.startdate < fs.EndDate) or
				(fs.EndDate > hcs.startdate and fs.StartDate < hcs.EndDate)
			)
		) as fsc1 
	),
	ghc as(
		select gov1.ClientId, gov1.Value, 
		cast(gov1.StartDate as date) as StartDate, 
		cast((select min(gov2.StartDate) from GovHcHours as gov2 where gov1.ClientId = gov2.ClientId and gov1.StartDate < gov2.StartDate) as date) as EndDate
		from GovHcHours as gov1
	),
	hcep as (
		select ClientId, 
			cast(StartDate as date) as StartDate,
			cast(EndDate as date) as EndDate
			from HomeCareEntitledPeriod 
	),
	cfs as (
		select ClientId,
			cast(StartDate as date) as StartDate,
			cast(EndDate as date) as EndDate
			from CfsRows
	),
	c as (
		select clients.Id as ClientId, a.GroupId as AgencyGroupId,
			cast(JoinDate as date) as JoinDate,
			cast(LeaveDate as date)	as LeaveDate,
			cast(DeceasedDate as date) as DeceasedDate
		from dbo.Clients join dbo.Agencies as a on dbo.clients.AgencyId = a.Id
	)


	select t.ClientId,  
		t.AgencyGroupId,
		cast(t.StartDate as date) as StartDate,
		cast(t.EndDate as date) as EndDate,
		t.JoinDate,
		t.LeaveDate,
		t.DeceasedDate,
		case SubstractGovHours
			when 1 then
				case when HcHours > GovHcHours then HcHours - GovHcHours else 0 end
			else 
				HcHours
		end
		as HcCap,
		GovHcHours
	from
	(
			select c.ClientId as ClientId, c.AgencyGroupId,
			c.JoinDate,
			c.LeaveDate,
			c.DeceasedDate,
			fs.SubstractGovHours,
			ghc.Value as GovHcHours,
			isnull(fs.HcHours, 0) as HcHours,
			(select max(mxsd.StartDate) from (
										select fs.startDate as StartDate union
										select hcep.startDate union
										select ghc.StartDate union
										select c.JoinDate ) as mxsd
			) as StartDate,
			(select min(mxsd.EndDate) from (
										select fs.EndDate as EndDate union
										select hcep.EndDate union
										select ghc.EndDate union
										select cfs.StartDate union --when cfs has start date and no end date in that period then no hours is allowed
										select case when c.DeceasedDate is not null then DATEADD(day, 1, c.DeceasedDate) else c.LeaveDate end) as mxsd
			) as EndDate
		from c 
		join hcep on hcep.clientid=c.ClientId 
		join fsc as fs on c.Clientid = fs.clientid and (
			(hcep.EndDate is null and fs.EndDate is null) or
			(hcep.EndDate is null or hcep.EndDate > fs.StartDate) or
			(fs.EndDate is null or fs.EndDate > hcep.StartDate)
		)
		join ghc on c.ClientId  = ghc.ClientId and (
			(hcep.EndDate is null and ghc.EndDate is null) or
			(hcep.EndDate is null or hcep.EndDate > ghc.StartDate) or
			(ghc.EndDate is null or ghc.EndDate > hcep.StartDate)
		)
		left outer join cfs on c.ClientId = cfs.ClientId and (
			cfs.EndDate is null and cfs.StartDate <= hcep.StartDate
		)
	) as t where (EndDate is null or StartDate < EndDate)


