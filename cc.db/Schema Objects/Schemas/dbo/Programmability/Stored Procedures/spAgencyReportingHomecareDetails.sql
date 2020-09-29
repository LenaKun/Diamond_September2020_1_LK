CREATE PROCEDURE [dbo].[spAgencyReportingHomecareDetails]
	@cur char(3)
	,@start date
	,@end date 
	,@submitted bit
	,@agencyId int
	,@regionId int
	,@countryId int
	,@stateId int
	,@serviceId int
	,@masterFundId int
	,@fundId int
	,@appId int
	,@clientId int
	,@term nvarchar(255)
	,@sortExp varchar(50)
	,@sortAsc bit
	,@top int
	,@skip int
	,@userId int
AS
	DECLARE @local_cur char(3) = @cur
		,@local_start date = @start
		,@local_end date = @end 
		,@local_submitted bit = @submitted
		,@local_agencyId int = @agencyId
		,@local_regionId int = @regionId
		,@local_countryId int = @countryId
		,@local_stateId int = @stateId
		,@local_serviceId int = @serviceId
		,@local_masterFundId int = @masterFundId
		,@local_fundId int = @fundId
		,@local_appId int = @appId
		,@local_clientId int = @clientId
		,@local_term nvarchar(255) = @term
		,@local_sortExp varchar(50) = @sortExp
		,@local_sortAsc bit = @sortAsc
		,@local_top int = @top
		,@local_skip int = @skip
		,@local_userId int = @userId
	
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
	where Id = @local_userId
	

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
					then case when exists(select top 1 1 from dbo.useragencygroups where userid = @local_userId and AgencyGroupId = ag.id) then 1 end
				when @role = @RegionReadOnly then 
					case when c.RegionId = @userRegionId then 1 end
				else 1 end = 1
	), tmasterfunds as (
		select * from MasterFunds
		where (not (@role = @BMF)) or id = 73
	), tmainreports as (
		select *
		from mainreports
		where (not (@role = @BMF)) or statusid in (2, 8, 32, 128)
	), srs as (
		select distinct	a.Id as AgencyId,
				fund.Name as FundName,
				app.Name as AppName,
				cr.ClientId,
				s.Name as ServiceName,
				cra.ReportDate,
				cra.Quantity,
				cr.Rate,
				app.CurrencyId as CurId,
				agapp.WeekStartDay as SelectedDayOfWeek,
				case when s.ReportingMethodId = 14 then 1 else 0 end as IsWeekly,
				mr.[Start] as MrStart,
				exRate.Value as ExchangeRate,
				0 as FakeSort
		from tmainreports as mr
			join subreports as sr on mr.id = sr.mainreportid
			join ClientReports as cr on sr.Id = cr.SubReportId
			join ClientAmountReport as cra on cr.Id = cra.ClientReportId
			join appbudgetservices as appbs on sr.appbudgetserviceid = appbs.id
			join appbudgets as appb on appbs.AppBudgetId = appb.id
			join apps as app on appb.appid = app.id
			join funds as fund on app.fundid = fund.id
			join tmasterfunds as mfund on fund.masterfundid = mfund.id
			join Services as s on appbs.serviceid = s.id
			join servicetypes as st on s.typeid = st.id
			join tagencies as a on appbs.agencyid = a.id
			join tagencygroups as ag on a.groupid = ag.id
			join countries as c on ag.countryid = c.id
			join viewAppExchangeRates as exRate on exRate.ToCur = @local_cur and exRate.AppId = App.Id
			left outer join States on ag.StateId = States.Id
			left outer join AgencyApps as agapp on a.Id = agapp.AgencyId and app.Id = agapp.AppId
		where ag.ExcludeFromReports = 0 and s.TypeId = 8 and (s.ReportingMethodId = 5 or s.ReportingMethodId = 14)
			and (@local_start is null or mr.start >= @local_start) 
			and (@local_end is null or mr.start < @local_end )
			and (@local_regionId is null or @local_regionId = 0 or c.RegionId = @local_regionId)
			and (@local_countryId is null or c.Id = @local_countryId)
			and (@local_stateId is null or States.Id = @local_stateId)
			and (@local_submitted = 0 or case when dbo.fnMainReportSubmitted(mr.StatusId) = 1 then 1 else 0 end = @local_submitted)
			and (@local_agencyId is null or appbs.AgencyId = @local_agencyId)
			and (@local_serviceId is null or s.id = @local_serviceId)
			and (@local_masterFundId is null or mfund.Id = @local_masterFundId)
			and (@local_fundId is null or fund.id = @local_fundId)
			and (@local_appId is null or app.id = @local_appId)
			and (@local_clientId is null or cr.ClientId = @local_clientId)
			and (@local_term is null or app.Name like @local_term + '%' or fund.Name like @local_term + '%' or s.Name like @local_term + '%')
	), res as (
		select  *,
		row_number() over (
			order by FakeSort
		) as rn
		from srs
	)
	select	sr.AgencyId
			,sr.FundName
			,sr.AppName
			,sr.ClientId
			,sr.ServiceName
			,sr.Quantity
			,sr.Rate
			,case when sr.ExchangeRate > 0 then sr.ExchangeRate * sr.Quantity * sr.Rate else sr.Quantity * sr.Rate end as Amount
			,sr.CurId
			,sr.SelectedDayOfWeek
			,CAST(sr.IsWeekly as bit) as IsWeekly
			,sr.MrStart
			,sr.ReportDate
	from res as sr
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
RETURN 0
GO
