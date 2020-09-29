CREATE PROCEDURE [dbo].[spFinancialSummaryOverview]
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
	
declare @allowedAgencies dbo.singleintcoltabletype;
insert into @allowedAgencies (id)
select agencyid from AllowedAgencies(@userId);
declare @roleId int = (select top 1 roleid from users where id = @userId);

--when there's no client filter: amount can be taken from the subreport level, the clientreports stats have to be outer joined to include subreports without clients list
--when there is a client filter: amounts has to be taken from the clientreport level , the clientreports stats have to be inner joined to exclude the subreports without clients list
if (@clientId is null and @term is null)
begin
	;with p3 as (
		select
			a.Name as AgencyName,
			s.id as ServiceId,
			a.id as AgencyId,
			--CASE
			--WHEN p1.ServiceId = 484 THEN rep.viewFinSumDet.Amount
			--ELSE p1.Amount 
			--END as Amount,
			p1.Amount,
			p1.FundsCount,
			p1.CcGrant,
			st.Name as ServiceTypeName,
			s.Name as ServiceName,
			mf.Name as MasterFundName,
			p0.ClientsCount as ClientsCount,
			p0.UniqueClientsCount,
			p1.Amount / nullif(p0.UniqueClientsCount,0) as AverageCostPerUnduplicatedClient
		from dbo.fnFinancialSummaryOverviewPart1(@cur, @start ,@end ,@submitted ,@showEstimated ,@agencyId ,@regionId ,@countryId ,@stateId ,@serviceTypeId ,@serviceId ,@masterFundId ,@fundId ,@appId ,@clientId ,@term ,@roleId ,@allowedAgencies) as p1
		join agencies as a on p1.AgencyId = a.Id
		join services as s on p1.ServiceId = s.Id
		join servicetypes as st on s.typeid = st.Id
		join MasterFunds as mf on p1.MasterFundId = mf.Id
		join AgencyGroups as agencyGroup on a.GroupId = agencyGroup.Id
		join Countries as country on agencyGroup.CountryId = country.Id
		--join rep.viewFinSumDet on p1.AgencyId = rep.viewFinSumDet.AgencyId and p1.ServiceId = rep.viewFinSumDet.ServiceId
		left outer join dbo.fnFinancialSummaryOverviewPart0(@start ,@end ,@submitted ,@showEstimated ,@agencyId ,@regionId ,@countryId ,@stateId ,@serviceTypeId ,@serviceId ,@masterFundId ,@fundId ,@appId ,@clientId ,@term ,@roleId ,@allowedAgencies) as p0
			on  p1.MasterFundId = p0.MasterFundId
			and p1.AgencyId = p0.AgencyId
			and p1.ServiceId = p0.ServiceId
		where 
		--filter
			(@local_regionid is null or country.RegionId = @local_regionId)
			and (@local_countryId is null or agencygroup.CountryId = @local_countryId)
			and (@local_stateId is null or agencygroup.StateId = @local_stateId)
			and (@local_showEstimated = 1 or s.ReportingMethodId not in (2,3,8))
			and (@local_agencyId is null or a.Id = @local_agencyId)
			and (@local_serviceTypeId is null or s.TypeId = @local_serviceTypeId)
			and (@local_serviceId is null or s.Id = @local_serviceid)
			and (@local_masterFundId is null or mf.Id = @local_masterFundId)
		--permissions
			and (@roleId <> 128 or mf.Id = 73)
			and (@roleId in (1,128,1024) or a.Id in (select id from @allowedAgencies))
	),
	res as (
		select  *,
		row_number() over (
			order by AgencyId
		) as rn
		from p3
	)
	select AgencyId,
			AgencyName,
			t.ServiceId as ServiceId,
			t.ServiceName as ServiceName,
			t.ServiceTypeName as ServiceTypeName,
			t.Amount as Amount,
			t.ClientsCount as ClientsCount,
			t.UniqueClientsCount,
			t.AverageCostPerUnduplicatedClient as AverageCostPerUnduplicatedClient,
			t.FundsCount as FundsCount,
			t.MasterFundName as MasterFundName,
			t.CcGrant as CcGrant
	from res as t
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
end
else
begin
	--basically the same as one above, except the amounts here must be taken from the client report level instead of the subreports
	with p3 as (
		select
			a.Name as AgencyName,
			s.id as ServiceId,
			a.id as AgencyId,
			--CASE
			--WHEN p0.ServiceId = 484 THEN rep.viewFinSumDet.Amount
			--ELSE p0.Amount 
			--END as Amount,
			p0.Amount,
			p1.FundsCount,
			p1.CcGrant,
			st.Name as ServiceTypeName,
			s.Name as ServiceName,
			mf.Name as MasterFundName,
			p0.ClientsCount as ClientsCount,
			p0.UniqueClientsCount,
			p1.Amount / nullif(p0.UniqueClientsCount,0) as AverageCostPerUnduplicatedClient
		from dbo.fnFinancialSummaryOverviewPart1(@cur, @start ,@end ,@submitted ,@showEstimated ,@agencyId ,@regionId ,@countryId ,@stateId ,@serviceTypeId ,@serviceId ,@masterFundId ,@fundId ,@appId ,@clientId ,@term ,@roleId ,@allowedAgencies) as p1
		join agencies as a on p1.AgencyId = a.Id
		join services as s on p1.ServiceId = s.Id
		join servicetypes as st on s.typeid = st.Id
		join MasterFunds as mf on p1.MasterFundId = mf.Id
		join AgencyGroups as agencyGroup on a.GroupId = agencyGroup.Id
		join Countries as country on agencyGroup.CountryId = country.Id
				join dbo.fnFinancialSummaryOverviewPart0(@start ,@end ,@submitted ,@showEstimated ,@agencyId ,@regionId ,@countryId ,@stateId ,@serviceTypeId ,@serviceId ,@masterFundId ,@fundId ,@appId ,@clientId ,@term ,@roleId ,@allowedAgencies) as p0
			on  p1.MasterFundId = p0.MasterFundId
			and p1.AgencyId = p0.AgencyId
			and p1.ServiceId = p0.ServiceId
--join rep.viewFinSumDet on p0.AgencyId = rep.viewFinSumDet.AgencyId and p0.ServiceId = rep.viewFinSumDet.ServiceId
		where 
		--filter
			(@local_regionid is null or country.RegionId = @local_regionId)
			and (@local_countryId is null or agencygroup.CountryId = @local_countryId)
			and (@local_stateId is null or agencygroup.StateId = @local_stateId)
			and (@local_showEstimated = 1 or s.ReportingMethodId not in (2,3,8))
			and (@local_agencyId is null or a.Id = @local_agencyId)
			and (@local_serviceTypeId is null or s.TypeId = @local_serviceTypeId)
			and (@local_serviceId is null or s.Id = @local_serviceid)
			and (@local_masterFundId is null or mf.Id = @local_masterFundId)
		--permissions
			and (@roleId <> 128 or mf.Id = 73)
			and (@roleId in (1,128,1024) or a.Id in (select id from @allowedAgencies))
	),
	res as (
		select  *,
		row_number() over (
			order by AgencyId
		) as rn
		from p3
	)
	select AgencyId,
			AgencyName,
			t.ServiceId as ServiceId,
			t.ServiceName as ServiceName,
			t.ServiceTypeName as ServiceTypeName,
			t.Amount as Amount,
			t.ClientsCount as ClientsCount,
			t.UniqueClientsCount,
			t.AverageCostPerUnduplicatedClient as AverageCostPerUnduplicatedClient,
			t.FundsCount as FundsCount,
			t.MasterFundName as MasterFundName,
			t.CcGrant as CcGrant
	from res as t
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn
end
RETURN 0
GO