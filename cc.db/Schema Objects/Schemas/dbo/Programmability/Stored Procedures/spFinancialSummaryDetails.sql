CREATE PROCEDURE [dbo].[spFinancialSummaryDetails]
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
		,@local_userId int = @userId
	;with t1 as (
		select t.SubReportId
			,t.ClientId
			,t.FirstName as FirstName
			,t.LastName as LastName
			,t.AgencyId
			,t.ServiceTypeName
			,t.ServiceName
			,t.ReportStart
			,t.ReportEnd
			,t.FundName
			,t.AppName
			,t.Quantity
			,t.Amount * exRate.Value as Amount
			,t.IsEstimated
			,t.AppCur
			,t.Remarks
			,0 as FakeSort
		from rep.viewFinSumDet as t-- with (noexpand)
			join dbo.viewAppExchangeRates as exRate on exRate.ToCur = @local_cur and exRate.AppId = t.AppId
		where case (select top 1 RoleId from dbo.Users where id = @userId)
					when 1 then 1
					when 2 then 
						case when AgencyId = (select top 1 AgencyId from users where id = @userId) then 1 end
					when 4 then
						case when AgencyId = (select top 1 AgencyId from users where id = @userId) then 1 end
					when 8 then
						case when AgencyGroupId in (select AgencyGroupId from dbo.useragencygroups where userid = @userId) then 1 end
					when 16 then 1
					when 32 then
						case when AgencyGroupId = (select top 1 Agencygroupid from users where id = @userId) then 1 end
					when 64 then
						case when AgencyGroupId in (select AgencyGroupId from dbo.useragencygroups where userid = @userId) then 1 end
					when 128 then
						case when MasterFundId = 73 and MainReportStatusId in (2, 8, 32, 128) then 1 end
					when 256 then 1
					when 512 then
						case when RegionId = (select top 1 RegionId from users where id = @userid) then 1 end
					when 1024 then 1
					when 2048 then
						case when AgencyId = (select top 1 AgencyId from users where id = @userId) then 1 end
					when 4096 then
						case when AgencyId = (select top 1 AgencyId from users where id = @userId) then 1 end
					when 16384 then 
						case when AgencyId = (select top 1 AgencyId from users where id = @userId) then 1 end
					when 32768 then 
						case when AgencyGroupId = (select top 1 Agencygroupid from users where id = @userId) then 1 end
				end = 1
			and (@local_start is null or t.ReportStart >= @local_start) 
			and (@local_end is null or t.ReportStart < @local_end )
			and (@local_regionid is null or t.RegionId = @local_regionId)
			and (@local_countryId is null or t.CountryId = @local_countryId)
			and (@local_stateId is null or t.StateId = @local_stateId)
			and (@local_submitted = 0 or case when t.MainReportStatusId in (2, 8, 32, 128) then 1 else 0 end = @local_submitted)
			and (@local_showEstimated = 1 or t.IsEstimated = 0)
			and (@local_agencyId is null or t.AgencyId = @local_agencyId)
			and (@local_serviceTypeId is null or t.ServiceTypeId = @local_serviceTypeId)
			and (@local_serviceId is null or t.ServiceId = @local_serviceid)
			and (@local_masterFundId is null or t.MasterFundId = @local_masterFundId)
			and (@local_fundId is null or t.FundId = @local_fundId)
			and (@local_appid is null or t.AppId = @local_appid)
			and (@local_term is null 
				or t.FirstName like @local_term + '%'
				or t.LastName like @local_term + '%')
			and (@local_clientId is null or t.ClientId = @local_clientId)
	), res as (
		select  *,
		row_number() over (
			order by FakeSort
		) as rn
		from t1
	)
	select t.SubReportId
			,t.ClientId
			,t.FirstName
			,t.LastName
			,t.AgencyId
			,t.ServiceTypeName
			,t.ServiceName
			,t.ReportStart
			,t.ReportEnd
			,t.FundName
			,t.AppName
			,t.Quantity
			,t.Amount
			,t.IsEstimated
			,cast(0 as decimal) as CcGrant
			,t.AppCur
			,t.Remarks
			,t.SubReportId
	from res as t
	where rn between @local_skip + 1 and @local_skip + @local_top
	order by rn

RETURN 0
GO
