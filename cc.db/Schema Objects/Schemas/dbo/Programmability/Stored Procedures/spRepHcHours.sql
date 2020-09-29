CREATE PROCEDURE [dbo].[sp_Reporting_HcHours]
@RegionId int,
@From date,
@To date
AS


select 
	Regions.Name as RegionName,
	Countries.Name as CountryName,
	States.Name as StateName,
	ag.Name as AgencyGroupName,
	ag.Id as AgencyGroupId,
	hccap.HcCap as HcCapHours,
	amounts.Quantity,
	amounts.Usd as UsdAmount,
	amounts.eur as EurAmount
	from agencygroups as ag
	join Countries on ag.CountryId = Countries.Id
	join Regions on Countries.RegionId = Regions.Id
	left outer join States on ag.StateId = states.Id
	left outer join (
		select AgencyGroupId, sum(Hccap) as HcCap
		from dbo.HcCapsTable(@from, @to) as t
		group by t.AgencyGroupId 
	) as hccap on ag.id = hccap.agencygroupid
	join (
		select apps.AgencyGroupId, 
			sum(usd.value*srq.Amount) as Usd,
			sum(eur.value*srq.Amount) as Eur,
			sum(srq.Quantity) as Quantity
		from (
			select cr.SubReportId, sum(ar.Quantity * cr.Rate) as Amount, sum(ar.Quantity) as Quantity
			from ClientAmountReport as ar
			join ClientReports as cr on ar.ClientReportId = cr.Id
			where ar.ReportDate >= @From and ar.ReportDate <= @To
			group by cr.SubReportId
		) as srq
		join SubReports as sr on srq.SubReportId = sr.Id
		join MainReports as mr on sr.MainReportId = mr.Id
		join AppBudgets as appb on mr.AppBudgetId = appb.Id
		join apps on appb.AppId = apps.id
		LEFT OUTER join viewAppExchangeRates as USD on apps.id = USD.AppId and USD.ToCur='USD'
		LEFT OUTER join viewAppExchangeRates as EUR on apps.id = EUR.AppId and EUR.ToCur='EUR'
		where mr.StatusId = 2
		group by apps.AgencyGroupId
	) as amounts on ag.Id = amounts.AgencyGroupId
	where ag.ExcludeFromReports = 0 and (@RegionId is null or regions.Id = @RegionId)



RETURN 0
