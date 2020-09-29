CREATE VIEW [dbo].[ViewClientsServiceDate_New]
	as
	

with submitted as (
	select *
	from MainReports mr 
	where dbo.fnMainReportSubmitted(mr.StatusId) = 1
), maxMr as (
select vcr.ClientId, MAX(mr.[Start]) as MaxStartDate
from submitted mr
join SubReports sr on mr.Id = sr.MainReportId
join ViewClientReports vcr on sr.Id = vcr.SubReportId
group by vcr.ClientId
), mrs as (
	select vcr.ClientId, c.MasterId, mr.[Start] as StartDate, DateAdd(day, -1, mr.[End]) as EndDate,
		s.TypeId as ServiceTypeId, s.ReportingMethodId
	from maxMr
	join ViewClientReports vcr on maxMr.ClientId = vcr.ClientId
	join SubReports sr on vcr.SubReportId = sr.Id
	join MainReports mr on sr.MainReportId = mr.Id
	join AppBudgetServices appbs on sr.AppBudgetServiceId = appbs.Id
	join [Services] s on appbs.ServiceId = s.Id
	join Clients c on vcr.ClientId = c.Id
	where mr.[Start] >= maxMr.MaxStartDate and dbo.fnMainReportSubmitted(mr.StatusId) = 1
)

select ClientId, MasterId,
case when DATEPart(month, LastStartDate) = DatePart(month,LastEndDate)
then  DATENAME(month, LastStartDate) +' '+ DateName(year,LastStartDate)
else (DATENAME(month, LastStartDate) +' - '+ DATENAME(month, LastEndDate) + ' '+ DateName(year,LastStartDate)) 
end as LastDate, CAST(IsHomecare as bit) as IsHomecare
from
(	
	select ClientId,
	case IsNull(MasterId,0) when 0 then
	(
		select IsNull(Max(MasterId),'') FROM [dbo].[Clients]
		where MasterIdClcd = ClientId
	) else IsNull(MasterId,'') end as MasterId,
	MAX(StartDate) as LastStartDate, MAX(EndDate) as LastEndDate,
	MAX(case when servicetypeid = 8 and (ReportingMethodId = 5 or ReportingMethodId = 14) then 1 else 0 end) as IsHomecare
	from mrs m
	group by ClientId, MasterId
) y




















