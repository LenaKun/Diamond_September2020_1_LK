CREATE VIEW [dbo].[viewScRepSource]
AS 
	with eligibleClients as (
		select 
			sr.Id as SubReportId,
			c.Id as ClientId, 
			repMonths.start as reportDate,
			c.SC_MonthlyCost, 
			c.JoinDate, 
			c.LeaveDate,
			mr.StatusId,
			coalesce(sc.Amount, c.SC_MonthlyCost) as Amount
			
		from SubReports as sr
		join MainReports as mr on sr.MainReportId = mr.Id
		join AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join Agencies as a on bs.AgencyId = a.Id
		join Clients as c on a.Id = c.AgencyId
		join AgencyGroups as ag on a.GroupId = ag.Id
		join [dbo].[Services] as s on bs.ServiceId = s.Id
		outer apply (
			select case when ag.ReportingPeriodId > 0 then mr.start end as start union
			select case when ag.ReportingPeriodId > 1 then dateadd(month, 1, mr.start) else mr.start end as start union
			select case when ag.ReportingPeriodId > 2 then dateadd(month, 2, mr.start) else mr.start end as start
		) as repMonths
		outer apply (
			select top 1 * from ScSubsidyAmounts as scsa
			where scsa.StartDate <= repMonths.start and scsa.LevelId = ag.scsubsidylevelid
			order by scsa.StartDate desc
		) as sc
		where s.ReportingMethodId = 12 /*  reportingmethod.SupportiveCommunities */
			and JoinDate < DATEADD(day, 25 /* sc_max_day */, repMonths.start)
			and (LeaveDate is null or LeaveDate >= DATEADD(day, 9 /*  sc_min_day*/, repMonths.start))
			and exists (
				select top 1 1 from HomeCareEntitledPeriod as hcep where hcep.ClientId = c.Id 
				and hcep.StartDate < DATEADD(day, 25 /* sc_max_day */, repMonths.start)
				and (hcep.EndDate is null or hcep.EndDate >= DATEADD(day, 10 /*  sc_min_day*/, repMonths.start))
			)
	)
	select
		subreportid, clientid, StatusId as MrStatusId,
		avg(Amount) as HoursHoldCost,
		count(Amount) as MonthsCount, 
		sum(amount) as Amount
	from eligibleclients
	group by subreportid, clientid, StatusId
