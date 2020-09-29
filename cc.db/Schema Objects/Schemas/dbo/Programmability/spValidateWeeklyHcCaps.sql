CREATE PROCEDURE [dbo].[spValidateWeeklyHcCaps]
	@mainreportid int
AS

declare @homecareWeeklyReportingMethodId int = 14
;with reportedClients as (
	select distinct ClientId, MasterId, FirstName, LastName, CapId, CapName, CapPerPerson, CurrencyId, StartDate, EndDate
	from (
		select c.id as ClientId, 
			c.MasterIdClcd as MasterId,
			c.FirstName,
			c.LastName,
			cap.Id as CapId,
			cap.Name as CapName,
			cap.CapPerPerson,
			cap.CurrencyId,
			cap.StartDate,
			cap.EndDate as EndDate
		from SubReports as sr
		join AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join services as s on bs.serviceid = s.id
		join AppBudgets as b on bs.AppBudgetId = b.Id
		join apps as a on b.AppId = a.Id
		join AgencyGroups as ag on a.AgencyGroupId = ag.Id
		join HCWeeklyCapCountries as cc on ag.CountryId = cc.CountryId
		join HCWeeklyCapFunds as cf on a.FundId = cf.FundId
		join HCWeeklyCaps as cap on cc.hcweeklycapid = cap.id and cf.hcweeklycapid = cap.id
		join clientreports as cr on sr.id = cr.subreportid
		join ClientAmountReport as ar on cr.id = ar.ClientReportId
		join clients as c on cr.clientid = c.id
		where sr.mainreportid = @mainreportid
		and s.ReportingMethodId = @homecareWeeklyReportingMethodId
		and ar.ReportDate >= cap.StartDate and ar.ReportDate < cap.EndDate
	)as t
) , quantities as (
	select t.MasterIdClcd as MasterId, CapId, capperperson, sum(Amount * ex.Value) as Amount
	from (
		select c.MasterIdClcd, cap.id as CapId, a.id as appid, cap.CurrencyId, cap.CapPerPerson, sum(cr.rate * ar.quantity) as Amount
		from SubReports as sr
		join mainreports as mr on sr.mainreportid = mr.id
		join clientreports as cr on sr.id = cr.subreportid
		join clientamountreport as ar on cr.id = ar.clientreportid
		join AppBudgetServices as bs on sr.appbudgetserviceid = bs.id
		join clients as c on cr.clientid = c.id
		join services as s on bs.serviceid = s.id
		join AppBudgets as b on bs.appbudgetid = b.id
		join apps as a on b.appid = a.id
		join agencies as agency on bs.agencyid = agency.id
		join agencygroups as ag on agency.groupid = ag.id
		join HCWeeklyCapCountries as cc on ag.CountryId = cc.CountryId
		join HCWeeklyCapFunds as cf on a.FundId = cf.FundId
		join HCWeeklyCaps as cap on cc.hcweeklycapid = cap.id and cf.hcweeklycapid = cap.id
		where (sr.mainreportid = @mainreportid or mr.StatusId in (2,32,8,128))
		and s.ReportingMethodId = @homecareWeeklyReportingMethodId
		and ar.ReportDate >= cap.StartDate and ar.ReportDate < cap.EndDate
		group by c.MasterIdClcd, cap.id, CapPerPerson, a.id, cap.CurrencyId
	) as t
	join viewAppExchangeRates as ex on t.appid = ex.AppId and t.CurrencyId = ex.ToCur
	group by MasterIdClcd, CapId, CapPerPerson
	having sum(Amount * ex.Value) > CapPerPerson
)
select t2.ClientId, t2.FirstName, t2.LastName, t2.CapName, t1.Amount, t2.CurrencyId, t2.CapPerPerson
from quantities as t1
join reportedClients  as t2 on t1.MasterId = t2.MasterId and t1.CapId = t2.CapId



RETURN 0
