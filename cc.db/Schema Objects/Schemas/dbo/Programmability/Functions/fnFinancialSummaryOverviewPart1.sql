CREATE FUNCTION [dbo].[fnFinancialSummaryOverviewPart1]
(
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
	,@RoleId int
	,@allowedAgencies dbo.singleIntColTableType readonly

)
RETURNS TABLE AS RETURN
(
	with exchangeRates as (
		select ex.AppId, ex.ToCur, ex.Value
		from dbo.viewAppExchangeRates as ex
		where ex.tocur = @cur
	)
	select f.MasterFundId, bs.AgencyId, bs.ServiceId, 
			sum(bs.CcGrant * ex.Value) as CcGrant,
			sum(t1.Amount * ex.Value) as Amount,
			count(distinct a.FundId) as FundsCount
		from AppBudgetServices as bs
		join (
			--Amount is fetched here from the subreports (precalculated)
			select AppBudgetServiceId, 
			--sum(amount) as Amount
			(sum(isnull(sr.Amount, 0)) + sum(isnull(cr.Amount, 0))) as Amount  --Lena
			from SubReports as sr
			join mainreports as mr on sr.MainReportId = mr.Id
			left outer join ClientReports as cr on sr.Id = cr.SubReportId
			where 
				--deny reading the unsubmitted reports from the bmf users
				--all the other rules will be checked later
				case when @roleId = 128 then case when mr.StatusId in (2, 8, 32, 128) then 1 end
					else 1
					end = 1
				--filter
				and (@start is null or mr.[Start] >= @start) 
				and (@end is null or mr.[Start] < @end )
				and (@submitted = 0 or case when mr.StatusId in (2, 8, 32, 128) then 1 else 0 end = @submitted)
			group by AppBudgetServiceId
		) as t1 on bs.Id = t1.AppBudgetServiceId
		join AppBudgets as b on bs.appbudgetid = b.Id
		join apps as a on b.AppId = a.Id
		join funds as f on a.FundId = f.Id
		left outer join exchangeRates as ex on b.AppId = ex.AppId
		where
		--filter
			(@fundId is null or a.FundId = @fundId)
			and (@appid is null or a.Id = @appid)
		group by f.MasterFundId, bs.AgencyId, bs.ServiceId
)
