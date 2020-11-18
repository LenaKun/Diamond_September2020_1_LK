CREATE PROCEDURE [dbo].[spFinancialSummarySummary]
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

declare @allowedAgencies table (id int);
insert into @allowedAgencies (id)
select agencyid from AllowedAgencies(@userId);
declare @roleId int = (select top 1 roleid from users where id = @userId);

--when there's no client filter: amount can be taken from the subreport level, the clientreports stats have to be outer joined to include subreports without clients list
--when there is a client filter: amounts has to be taken from the clientreport level , the clientreports stats have to be inner joined to exclude the subreports without clients list
if (@clientId is null and @term is null)
begin
	;with exchangeRates as (
		select ex.AppId, ex.ToCur, ex.Value
		from dbo.viewAppExchangeRates as ex
		where ex.tocur = @local_cur
	), p0 as (
		select b.AppId, bs.ServiceId,
			sum(t1.Amount) as Amount,
			sum(bs.CcGrant) as CcGrant
		from AppBudgetServices as bs
		join (
			--Amount is fetched here from the subreports (precalculated)
			select AppBudgetServiceId,
			sum(coalesce(Amount, 0)) as Amount
			from viewSubreportAmounts as sr
			join mainreports as mr on sr.MainReportId = mr.Id
			where 
				--deny reading the unsubmitted reports from the bmf users
				--all the other rules will be checked later
				case when @roleId = 128 then case when mr.StatusId in (2, 8, 32, 128) then 1 end
					else 1
					end = 1
				--filter
				and (@local_start is null or mr.[Start] >= @local_start) 
				and (@local_end is null or mr.[Start] < @local_end )
				and (@local_submitted = 0 or case when mr.StatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			group by AppBudgetServiceId
		) as t1 on bs.Id = t1.AppBudgetServiceId
		join AppBudgets as b on bs.appbudgetid = b.Id
		join agencies as a on bs.AgencyId = a.Id
		join AgencyGroups as ag on a.GroupId = ag.Id
		join Countries as c on ag.CountryId = c.Id
		where
		--filter
			--columns that are lost after the grouping
			(@local_regionid is null or c.RegionId = @local_regionId)
			and (@local_countryId is null or ag.CountryId = @local_countryId)
			and (@local_stateId is null or ag.StateId = @local_stateId)
			and (@local_agencyId is null or bs.AgencyId = @local_agencyId)
			--permissions
			and (@roleId in (1,128,1024) or bs.AgencyId in (select id from @allowedAgencies))
		group by b.AppId, bs.ServiceId
	), p1 as (
		select t.AppId, t.ServiceId,
			count(distinct t.clientid) as DistinctClientCount,
			count(distinct t.masterid) as MasterClientsCount,
			sum(t.quantity) as Quantity
		from rep.viewFinSumDet as t with (noexpand)
		where 
			--filter minus properties that will servive the grouping
				(@local_start is null or t.ReportStart >= @local_start) 
			and (@local_end is null or t.ReportStart < @local_end )
			and (@local_regionid is null or t.RegionId = @local_regionId)
			and (@local_countryId is null or t.CountryId = @local_countryId)
			and (@local_stateId is null or t.StateId = @local_stateId)
			and (@local_submitted = 0 or case when t.MainReportStatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			and (@local_agencyId is null or t.AgencyId = @local_agencyId)
			and (@local_term is null 
				or t.FirstName like @local_term + '%'
				or t.LastName like @local_term + '%')
			and (@local_clientId is null or t.ClientId = @local_clientId)
			--premissions
			and case when @roleId = 128 then case when t.MainReportStatusId in (2, 8, 32, 128) and t.MasterFundId = 73 then 1 end
				else 1
				end = 1
			and (@roleId in (1,128,1024) or t.AgencyId in (select id from @allowedAgencies))
		group by t.AppId, t.ServiceId
	),p2 as (
		select  t.AppId, t.ServiceId,
			[service].Name as ServiceName,
			[serviceType].Name as ServiceTypeName,
			[app].Name as appName, 
			[fund].Name as FundName,
			mf.Name as MasterFundName,
			p1.DistinctClientCount,
			p1.Quantity,
			ex.[Value] * t.Amount as Amount,
			ex.[Value] * t.Amount / nullif(p1.DistinctClientCount, 0) as AverageCostPerClient,
			ex.[Value] * t.Amount / nullif(p1.Quantity, 0) as AverageCostPerUnit,
			t.CcGrant
		from p0 as t
		left outer join exchangeRates as ex on t.AppId = ex.AppId
		join dbo.Apps as app on t.AppId = app.Id
		join dbo.services as [service] on t.ServiceId = [service].Id
		join dbo.ServiceTypes as [serviceType] on [service].TypeId = [serviceType].Id
		join dbo.Funds as [fund] on [app].FundId = [fund].Id
		join dbo.MasterFunds as mf on [fund].MasterFundId = mf.Id
		left outer join p1 on t.AppId = p1.AppId and t.ServiceId = p1.ServiceId
		where (@local_showEstimated = 1 or [service].ReportingMethodId not in (2,3,8))
			and (@local_serviceTypeId is null or [service].TypeId = @local_serviceTypeId)
			and (@local_serviceId is null or [service].Id = @local_serviceid)
			and (@local_masterFundId is null or [fund].MasterFundId = @local_masterFundId)
			and (@local_fundId is null or [fund].Id = @local_fundId)
			and (@local_appid is null or t.appid = @local_appid)
			--permissions
			and (@roleId <> 128 or [fund].MasterFundId = 73)
	), res as (
		select  *,
		row_number() over (
			order by appid
		) as rn
		from p2
	)
	select t.ServiceTypeName as ServiceType
			,t.ServiceName as [Service]
			, t.Quantity as Quantity
			,t.Amount as Amount
			,t.DistinctClientCount as ClientsCount
			,t.AverageCostPerClient as AverageCostPerClient
			,t.AverageCostPerUnit as AverageCostPerUnit
			,@cur as Cur
			,t.appName as [App]
			,t.FundName as [Fund]
			,t.MasterFundName as [MasterFund]
			,t.CcGrant as CcGrant
			,t.ServiceId as ServiceId
			,t.AppId as AppId
	from res as t
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
end
else
begin
	;with exchangeRates as (
		select ex.AppId, ex.ToCur, ex.Value
		from dbo.viewAppExchangeRates as ex
		where ex.tocur = @local_cur
	), p0 as (
		select b.AppId, bs.ServiceId,
			--sum(CAST(t1.Amount as decimal(38,0))) as Amount,
			sum(t1.Amount) as Amount,
			sum(bs.CcGrant) as CcGrant
		from AppBudgetServices as bs
		join (
			--Amount is fetched here from the subreports (precalculated)
			select AppBudgetServiceId, 
			sum(coalesce(Amount, 0)) as Amount
			from viewSubreportAmounts as sr
			join mainreports as mr on sr.MainReportId = mr.Id
			where 
				--deny reading the unsubmitted reports from the bmf users
				--all the other rules will be checked later
				case when @roleId = 128 then case when mr.StatusId in (2, 8, 32, 128) then 1 end
					else 1
					end = 1
				--filter
				and (@local_start is null or mr.[Start] >= @local_start) 
				and (@local_end is null or mr.[Start] < @local_end )
				and (@local_submitted = 0 or case when mr.StatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			group by AppBudgetServiceId
		) as t1 on bs.Id = t1.AppBudgetServiceId
		join AppBudgets as b on bs.appbudgetid = b.Id
		join agencies as a on bs.AgencyId = a.Id
		join AgencyGroups as ag on a.GroupId = ag.Id
		join Countries as c on ag.CountryId = c.Id
		where
		--filter
			--columns that are lost after the grouping
			(@local_regionid is null or c.RegionId = @local_regionId)
			and (@local_countryId is null or ag.CountryId = @local_countryId)
			and (@local_stateId is null or ag.StateId = @local_stateId)
			and (@local_agencyId is null or bs.AgencyId = @local_agencyId)
			and (@roleId in (1,128,1024) or bs.AgencyId in (select id from @allowedAgencies))
		group by b.AppId, bs.ServiceId
	), p1 as (
		select t.AppId, t.ServiceId,
			count(distinct t.clientid) as DistinctClientCount,
			count(distinct t.masterid) as MasterClientsCount,
			sum(t.quantity) as Quantity,
			sum(t.Amount) as Amount
		from rep.viewFinSumDet as t with (noexpand)
		
		where 
			--filter minus properties that will servive the grouping
				(@local_start is null or t.ReportStart >= @local_start) 
			and (@local_end is null or t.ReportStart < @local_end )
			and (@local_regionid is null or t.RegionId = @local_regionId)
			and (@local_countryId is null or t.CountryId = @local_countryId)
			and (@local_stateId is null or t.StateId = @local_stateId)
			and (@local_submitted = 0 or case when t.MainReportStatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			and (@local_agencyId is null or t.AgencyId = @local_agencyId)
			and (@local_term is null 
				or t.FirstName like @local_term + '%'
				or t.LastName like @local_term + '%')
			and (@local_clientId is null or t.ClientId = @local_clientId)
			--premissions
			and case when @roleId = 128 then case when t.MainReportStatusId in (2, 8, 32, 128) and t.MasterFundId = 73 then 1 end
				else 1
				end = 1
			and (@roleId in (1,128,1024) or t.AgencyId in (select id from @allowedAgencies))
		group by t.AppId, t.ServiceId
	),p2 as (
		select  t.AppId, t.ServiceId,
			[service].Name as ServiceName,
			[serviceType].Name as ServiceTypeName,
			[app].Name as appName, 
			[fund].Name as FundName,
			mf.Name as MasterFundName,
			p1.DistinctClientCount,
			p1.Quantity,
			ex.[Value] * p1.Amount as Amount,
			ex.[Value] * t.Amount / nullif(p1.DistinctClientCount, 0) as AverageCostPerClient,
			ex.[Value] * t.Amount / nullif(p1.Quantity, 0) as AverageCostPerUnit,
			t.CcGrant
		from p0 as t
		left outer join exchangeRates as ex on t.AppId = ex.AppId
		join dbo.Apps as app on t.AppId = app.Id
		join dbo.services as [service] on t.ServiceId = [service].Id
		join dbo.ServiceTypes as [serviceType] on [service].TypeId = [serviceType].Id
		join dbo.Funds as [fund] on [app].FundId = [fund].Id
		join dbo.MasterFunds as mf on [fund].MasterFundId = mf.Id
		join p1 on t.AppId = p1.AppId and t.ServiceId = p1.ServiceId
		where (@local_showEstimated = 1 or [service].ReportingMethodId not in (2,3,8))
			and (@local_serviceTypeId is null or [service].TypeId = @local_serviceTypeId)
			and (@local_serviceId is null or [service].Id = @local_serviceid)
			and (@local_masterFundId is null or [fund].MasterFundId = @local_masterFundId)
			and (@local_fundId is null or [fund].Id = @local_fundId)
			and (@local_appid is null or t.appid = @local_appid)
			--permissions
			and (@roleId <> 128 or [fund].MasterFundId = 73)
			
	), res as (
		select  *,
		row_number() over (
			order by appid
		) as rn
		from p2
	)
	select t.ServiceTypeName as ServiceType
			,t.ServiceName as [Service]
			, t.Quantity as Quantity
			,t.Amount as Amount
			,t.DistinctClientCount as ClientsCount
			,t.AverageCostPerClient as AverageCostPerClient
			,t.AverageCostPerUnit as AverageCostPerUnit
			,@cur as Cur
			,t.appName as [App]
			,t.FundName as [Fund]
			,t.MasterFundName as [MasterFund]
			,t.CcGrant as CcGrant
			,t.ServiceId as ServiceId
			,t.AppId as AppId
	from res as t
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
end
RETURN 0
GO
