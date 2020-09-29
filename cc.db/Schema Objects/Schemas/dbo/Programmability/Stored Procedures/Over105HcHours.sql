CREATE PROCEDURE [dbo].[Over105HcHours]
	@regionId int
	,@agencyGroupId int
	,@year int
	,@term nvarchar(255)
	,@top int
	,@skip int
	,@userId int
AS
	declare		
		@Admin int =1,
		@AgencyUser int =2,
		@AgencyOfficer int =4,
		@RegionOfficer int =8,
		@GlobalOfficer int =16,
		@Ser int =32,
        @RegionAssistant int =64,
        @BMF int =128,
        @GlobalReadOnly int =256,
        @RegionReadOnly int =512,
        @AuditorReadOnly int =1024,
		@AgencyUserAndReviewer int =16384,
		@SerAndReviewer int =32768

	declare @role int,
		@userAgencyId int,
		@userAgencyGroupId int,
		@userRegionId int

	select @role = users.RoleId,
		@userAgencyId = users.AgencyId,
		@userAgencyGroupId = users.AgencyGroupId,
		@userRegionId = users.RegionId
	from Users 
	where Id = @userId

	;with tagencies as (
		select * from dbo.agencies where 
		(not (@role = @AgencyUser or @role = @AgencyUserAndReviewer or @role = @AgencyOfficer)) or @useragencyId = id
	), tagencygroups as (
		select ag.* 
		from dbo.agencygroups as ag
		join dbo.countries as c on ag.CountryId = c.Id
		where
			case when @role = @ser or @role = @SerAndReviewer
					then case when @userAgencyGroupId = ag.id then 1 end
				when @role = @RegionOfficer or @role = @RegionAssistant
					then case when exists(select top 1 1 from dbo.useragencygroups where userid = @userId and AgencyGroupId = ag.id) then 1 end
				when @role = @RegionReadOnly then 
					case when c.RegionId = @userRegionId then 1 end
				else 1 end = 1
	), tmainreports as (
		select Id
		from mainreports
		where dbo.fnMainReportSubmitted(statusid) = 1
	), ghc as(
		select gov1.ClientId, gov1.[Value], 
		cast(gov1.StartDate as date) as StartDate, 
		cast((select min(gov2.StartDate) from GovHcHours as gov2 where gov1.ClientId = gov2.ClientId and gov1.StartDate < gov2.StartDate) as date) as EndDate
		from GovHcHours as gov1
	), cras as (
		select cr.ClientId, co.RegionId, ag.Id as AgencyGroupId, ag.Name as AgencyGroupName, cra.ReportDate, cra.Quantity as Amount,
			dbo.GetWeekNumber(cra.ReportDate, coalesce(ap.WeekStartDay, 0)) as WeekNumber,
			gh.[Value] as GovHours, DATEPART(YEAR, cra.ReportDate) as [Year], DATENAME(MONTH, cra.ReportDate) as Month
		from tmainreports mr
			join SubReports sr on mr.Id = sr.MainReportId
			join ClientReports cr on sr.Id = cr.SubReportId
			join ClientAmountReport cra on cr.Id = cra.ClientReportId
			join AppBudgetServices appbs on sr.AppBudgetServiceId = appbs.Id
			join AppBudgets appb on appbs.AppBudgetId = appb.Id
			join Services s on appbs.ServiceId = s.Id
			join Clients c on cr.ClientId = c.Id
			join tagencies a on c.AgencyId = a.Id
			join tagencygroups ag on a.GroupId = ag.Id
			join Countries co on ag.CountryId = co.Id
			outer apply (select iap.* from agencyapps iap
						join agencies ia on iap.agencyid = ia.id
						join agencygroups iag on ia.groupid = iag.id
						where iag.id = ag.id and iap.appid = appb.appid) as ap
			outer apply (select top 1 * from ghc where ClientId = cr.ClientId and (EndDate is null or EndDate > cra.ReportDate) and StartDate < DATEADD(day, 7, cra.ReportDate) order by StartDate desc) as gh
		where s.ReportingMethodId = 14 /*homecare weekly*/ and
			(@regionId is null or @regionId = 0 or co.RegionId = @regionId) and
			(@agencyGroupId is null or ag.Id = @agencyGroupId) and
			(@year is null or DATEPART(YEAR, cra.ReportDate) = @year) and
			(@term is null or ag.Name like @term + '%')
	), q as (select MIN(cr.AgencyGroupId) as AgencyGroupId, MIN(cr.AgencyGroupName) as AgencyGroupName, cr.Year, MIN(cr.Month) as [Month], cr.ClientId, (SUM(cr.Amount) + coalesce(MIN(cr.GovHours), 0)) as TotalHours,
			SUM(cr.Amount) as ReportedHours, coalesce(MIN(cr.GovHours), 0) as GovHours, MIN(cr.RegionId) as RegionId, MIN(cr.ReportDate) as ReportDate, cr.WeekNumber as [Week] 
		from cras cr
		group by cr.ClientId, cr.Year, cr.WeekNumber
	), res as (
		select  AgencyGroupId, AgencyGroupName, [Year], [Month], ClientId, TotalHours, ReportedHours, GovHours, RegionId, ReportDate, [Week],
		row_number() over (
			order by ClientId, ReportDate
		) as rn
		from q
		where TotalHours > 105
	)
	select AgencyGroupId, AgencyGroupName, [Year], [Month], ClientId, TotalHours, ReportedHours, GovHours, RegionId, ReportDate, [Week]
	from res
	where rn between @skip + 1 and @skip + @top
	order by rn
RETURN 0
