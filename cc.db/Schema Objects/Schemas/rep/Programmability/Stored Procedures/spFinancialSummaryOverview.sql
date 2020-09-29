CREATE PROCEDURE [rep].[spFinancialSummaryOverview]
	@cur char(3)
	,@start date
	,@end date 
	,@submitted bit
	,@showEstimated bit
	,@agencyId int
	,@regionId int
	,@countryId int
	,@stateId int
	,@serviceTypeId int
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
	
	/*
		all the sp parameters are passed through local variables
		this must have something to do with param sniffing, 
		but using the sp params directly in the query forces query planner into single threaded execution (parllalization degree = 0)
	*/

	DECLARE @local_cur char(3) = @cur
		,@local_start date = @start
		,@local_end date = @end 
		,@local_submitted bit = @submitted
		,@local_showEstimated bit = @showEstimated
		,@local_agencyId int = @agencyId
		,@local_regionId int = @regionId
		,@local_countryId int = @countryId
		,@local_stateId int = @stateId
		,@local_serviceTypeId int = @serviceTypeId
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

	
	;with
	exchangeRates as (
		select ex.AppId, ex.ToCur, ex.Value
		from dbo.viewAppExchangeRates as ex
		where ex.tocur = @local_cur
	),
	srs as (
		select	t.SubReportId
			,t.AgencyId
			,t.ServiceTypeName
			,t.ServiceName
			,t.ReportStart
			,t.ReportEnd
			,t.FundName
			,t.AppName
			,t.IsEstimated
			,t.CcGrant
			,t.AppCur
			,exRate.Value as ExchangeRate
		from dbo.fnAllowedSubReports(@local_userid) as t
			join dbo.viewAppExchangeRates as exRate on exRate.ToCur = @local_cur and exRate.AppId = t.AppId
		where t.ExcludeFromReports  = 0
			and (@local_start is null or t.ReportStart >= @local_start) 
			and (@local_end is null or t.ReportStart < @local_end )
			and (@local_regionid is null or t.RegionId = @local_regionId)
			and (@local_countryId is null or t.CountryId = @local_countryId)
			and (@local_stateId is null or t.StateId = @local_stateId)
			and (@local_submitted = 0 or case when t.MainReportStatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			and (@local_showEstimated = 1 or t.ReportingMethodId not in (2,3,8))
			and (@local_agencyId is null or t.AgencyId = @local_agencyId)
			and (@local_serviceTypeId is null or t.ServiceTypeId = @local_serviceTypeId)
			and (@local_serviceId is null or t.ServiceId = @local_serviceid)
			and (@local_masterFundId is null or t.MasterFundId = @local_masterFundId)
			and (@local_fundId is null or t.FundId = @local_fundId)
			and (@local_appid is null or t.AppId = @local_appid)
	),
	grouped as (
		select sr.AgencyId, sr.ServiceId, sr.MasterFundId, MAX(sr.SubReportId) as SubReportId, SUM(sr.Amount) as Amount,
			COUNT(distinct sr.FundId) as FundsCount, MAX(sr.MasterFundName) as MasterFundName, MAX(sr.CcGrant) as CcGrant,
			MAX(sr.AppId) as AppId, MAX(sr.AppName) as AppName 
		from srs as sr
		group by sr.AgencyId, sr.ServiceId, sr.MasterFundId
	), CLientIds as (
		select c.Id as ClientId, c.MasterIdClcd, sr.AgencyId, sr.ServiceId, sr.MasterFundId
		from grouped as sr
		join ViewClientReports as vcr on sr.SubReportId = vcr.SubreportId
		join Clients as c on vcr.ClientId = c.Id
	), t as (
		select
		sr.AgencyId,
		a.Name as AgencyName,
		c.RegionId,
		ag.CountryId,
		ag.StateId,
		sr.ServiceId,
		s.Name as ServiceName,
		s.TypeId as ServiceTypeId,
		st.Name as ServiceTypeName,
		sr.Amount * ex.[Value] as Amount,
		case when s.ReportingMethodId = 3 then null 
			else (select COUNT(distinct ClientId) as CidCount from CLientIds cl where ClientId is not null and sr.AgencyId = cl.AgencyId and sr.ServiceId = cl.ServiceId and sr.MasterFundId = cl.MasterFundId) end as ClientsCount,
		case when s.ReportingMethodId = 3 then null 
			else (select COUNT(distinct MasterIdClcd) as MidCount from CLientIds cl where MasterIdClcd is not null and sr.AgencyId = cl.AgencyId and sr.ServiceId = cl.ServiceId and sr.MasterFundId = cl.MasterFundId) end as UniqueClientsCount,
		sr.FundsCount,
		sr.MasterFundName,
		sr.CcGrant * ex.[Value] as CcGrant,
		sr.AppId,
		sr.AppName
	from grouped as sr
	left outer join exchangeRates as ex on sr.AppId = ex.AppId
	join dbo.Agencies as a on sr.AgencyId = a.Id
	join AgencyGroups as ag on a.GroupId = ag.Id
	join Countries as c on ag.CountryId = c.Id
	join dbo.[services] as s on sr.ServiceId = s.Id
	join dbo.ServiceTypes as st on s.TypeId = st.Id
	), res as (
		select  *,
		row_number() over (
		--order by
			--case when @local_sortExp = 'FirstName' and @local_sortAsc = 1
			--	then FirstName end asc, 
			--case when @local_sortExp = 'FirstName' and @local_sortAsc = 0
			--	then FirstName end desc,
			--case when @local_sortExp = 'LastName' and @local_sortAsc = 1
			--	then LastName end asc, 
			--case when @local_sortExp = 'LastName' and @local_sortAsc = 0
			--	then LastName end desc,
			--case when @local_sortExp = 'ClientId' and @local_sortAsc = 1
			--	then ClientId end asc, 
			--case when @local_sortExp = 'ClientId' and @local_sortAsc = 0
			--	then ClientId end desc,
			--case when @local_sortExp = 'ServiceTypeName' and @local_sortAsc = 1
			--	then ServiceTypeName end asc, 
			--case when @local_sortExp = 'ServiceTypeName' and @local_sortAsc = 0
			--	then ServiceTypeName end desc,
			--case when @local_sortExp = 'ServiceName' and @local_sortAsc = 1
			--	then ServiceName end asc, 
			--case when @local_sortExp = 'ServiceName' and @local_sortAsc = 0
			--	then ServiceName end desc,
			--case when @local_sortExp = 'ReportStart' and @local_sortAsc = 1
			--	then ReportStart end asc, 
			--case when @local_sortExp = 'ReportStart' and @local_sortAsc = 0
			--	then ReportStart end desc,
			--case when @local_sortExp = 'ReportEnd' and @local_sortAsc = 1
			--	then ReportEnd end asc, 
			--case when @local_sortExp = 'ReportEnd' and @local_sortAsc = 0
			--	then ReportEnd end desc,
			--case when @local_sortExp = 'FundName' and @local_sortAsc = 1
			--	then FundName end asc, 
			--case when @local_sortExp = 'FundName' and @local_sortAsc = 0
			--	then FundName end desc,
			--case when @local_sortExp = 'AppName' and @local_sortAsc = 1
			--	then AppName end asc, 
			--case when @local_sortExp = 'AppName' and @local_sortAsc = 0
			--	then AppName end desc,
			--case when @local_sortExp = 'Quantity' and @local_sortAsc = 1
			--	then Quantity end asc, 
			--case when @local_sortExp = 'Quantity' and @local_sortAsc = 0
			--	then Quantity end desc,
			--case when @local_sortExp = 'Amount' and @local_sortAsc = 1
			--	then Amount end asc, 
			--case when @local_sortExp = 'Amount' and @local_sortAsc = 0
			--	then Amount end desc,
			--case when @local_sortExp = 'IsEstimated' and @local_sortAsc = 1
			--	then IsEstimated end asc, 
			--case when @local_sortExp = 'IsEstimated' and @local_sortAsc = 0
			--	then IsEstimated end desc
			order by AgencyName, ServiceTypeName, ServiceName
		) as rn
		from t
	)
	select sr.AgencyId,
		   sr.AgencyName,
		   sr.RegionId,
		   sr.CountryId,
		   sr.StateId,
		   sr.ServiceId,
		   sr.ServiceName,
		   sr.ServiceTypeId,
		   sr.ServiceTypeName,
		   sr.Amount,
		   sr.ClientsCount,
		   sr.UniqueClientsCount,
		   sr.FundsCount,
		   sr.MasterFundName,
		   sr.CcGrant,
		   sr.AppId,
		   sr.AppName
	from res as sr
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
	
RETURN 0
