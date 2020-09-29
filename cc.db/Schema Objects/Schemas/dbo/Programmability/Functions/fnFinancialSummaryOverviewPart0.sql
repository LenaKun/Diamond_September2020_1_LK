CREATE FUNCTION [dbo].[fnFinancialSummaryOverviewPart0]
(
	@start date
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
	,@RoleId int
	,@allowedAgencies dbo.singleIntColTableType readonly


)
RETURNS TABLE AS RETURN
(
	select t.MasterFundId, t.AgencyId, t.ServiceId,
			count(distinct t.clientid) as ClientsCount,
			count(distinct t.masterid) as UniqueClientsCount,
			sum(t.Amount) as Amount
		from rep.viewFinSumDet as t --with (noexpand)
		where 
				--filter 
				(@start is null or t.ReportStart >= @start) 
				and (@end is null or t.ReportStart < @end )
				and (@regionid is null or t.RegionId = @regionId)
				and (@countryId is null or t.CountryId = @countryId)
				and (@stateId is null or t.StateId = @stateId)
				and (@submitted = 0 or case when t.MainReportStatusId in (2, 8, 32, 128) then 1 else 0 end = @submitted)
				and (@showEstimated = 1 or t.ReportingMethodId not in (2,3,8))
				and (@agencyId is null or t.AgencyId = @agencyId)
				and (@serviceTypeId is null or t.ServiceTypeId = @serviceTypeId)
				and (@serviceId is null or t.ServiceId = @serviceid)
				and (@masterFundId is null or t.MasterFundId = @masterFundId)
				and (@fundId is null or t.FundId = @fundId)
				and (@appid is null or t.AppId = @appid)
				and (@term is null 
					or t.FirstName like @term + '%'
					or t.LastName like @term + '%')
				and (@clientId is null or t.ClientId = @clientId)
				--premissions
				and case when @roleId = 128 then case when t.MainReportStatusId in (2, 8, 32, 128) and t.MasterFundId = 73 then 1 end
					else 1
					end = 1
				and (@roleId in (1,128,1024) or t.AgencyId in (select id from @allowedAgencies))
		group by t.MasterFundId, t.AgencyId, t.ServiceId
)
