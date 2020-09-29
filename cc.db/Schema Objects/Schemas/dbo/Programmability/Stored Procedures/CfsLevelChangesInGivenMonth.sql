CREATE PROCEDURE [dbo].[CfsLevelChangesInGivenMonth]
	@monthStart datetime,
	@nextMonth datetime
AS
	set nocount on

	;with Cfs as (
		select ClientId
		from CfsRows
		where StartDate < @nextMonth and (EndDate is null or EndDate > @monthStart)
		group by ClientId
	), ClientChanges as (
		select h.ClientId, h.StartDate
		from HcCapsTableRaw h
		join Cfs on h.ClientId = Cfs.ClientId
		where h.StartDate < @nextMonth and (h.EndDate is null or h.EndDate > @monthStart)
		group by h.ClientId, h.StartDate
		having COUNT(*) > 0 and (COUNT(*) > 1 or h.StartDate > @monthStart or MIN(h.EndDate) is null or MIN(h.EndDate) < @nextMonth)
		union
		select c.ClientId, c.StartDate
		from ClientHcStatuses c
		join Cfs on c.ClientId = Cfs.ClientId
		where c.StartDate >= @monthStart and c.StartDate < @nextMonth
		group by c.ClientId, c.StartDate
		having COUNT(*) > 0 and (COUNT(*) > 1 or c.StartDate > @monthStart)
	), CfsLevels as (
		select c.ClientId, c.StartDate, MIN(cfs.StartDate) as CfsStartDate, dbo.CfsLevelForGivenDate(c.ClientId, c.StartDate) as CfsLevel
		from ClientChanges c
		join CfsRows cfs on c.ClientId = cfs.ClientId
		where (cfs.StartDate is null or cfs.StartDate <= c.StartDate) and (cfs.EndDate is null or cfs.EndDate > c.StartDate)
		group by c.ClientId, c.StartDate
	), ClientsWithLevels as (
		select ClientId, MIN(StartDate) as StartDate, CfsLevel, MIN(CfsStartDate) as CfsStartDate
		from CfsLevels
		group by ClientId, CfsLevel
	), ClientsGrouped as (
		select ClientId
		from ClientsWithLevels
		group by ClientId
		having COUNT(*) > 1
	)

	select cl.ClientId, cl.StartDate, CfsLevel, CfsStartDate, c.AgencyId, a.Name as AgencyName, c.JoinDate, hc.StartDate as EiligibilityStartDate,
		fs.StartDate as DafStartDate, fs.DiagnosticScore as DafScore, fs.Name as DafLevel, gh.StartDate as GovhStartDate, gh.GovhHours
	from ClientsWithLevels cl
	join ClientsGrouped cg on cl.ClientId = cg.ClientId
	join Clients c on cl.ClientId = c.Id
	join Agencies a on c.AgencyId = a.Id
	outer apply (select top 1 h.StartDate
				from HomeCareEntitledPeriod h
				where h.ClientId = cl.ClientId and h.StartDate <= cl.StartDate and (h.EndDate is null or h.EndDate > cl.StartDate)
				order by h.StartDate desc) as hc
	outer apply (select top 1 f.DiagnosticScore, f.StartDate, fl.Name
				from FunctionalityScores f
				join FunctionalityLevels fl on f.FunctionalityLevelId = fl.Id
				where f.ClientId = cl.ClientId and f.StartDate <= cl.StartDate
				order by f.StartDate desc) as fs
	outer apply (select top 1 g.StartDate, g.[Value] as GovhHours
				from GovHcHours g
				where g.ClientId = cl.ClientId and g.StartDate <= cl.StartDate
				order by g.StartDate desc) as gh
RETURN 0
