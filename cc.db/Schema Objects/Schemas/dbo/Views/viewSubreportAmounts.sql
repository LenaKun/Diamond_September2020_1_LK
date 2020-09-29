CREATE VIEW [dbo].[viewSubreportAmounts]
	AS 


	select 
		sr.id,
		sr.MainReportId as MainReportId,
		sr.AppBudgetServiceId as AppBudgetServiceId,
		sr.MatchingSum as MatchingSum,
		sr.AgencyContribution as AgencyContribution,
		case when s.ReportingMethodId in (2,3,8,15) and s.Name <> 'Funeral Expenses'
			then isnull(sr.amount,0)
		when s.ReportingMethodId = 2 and s.Name = 'Funeral Expenses' then isnull(a1.amount,0)
			else isnull(a1.amount,0)
		end as Amount,
		a1.Quantity,
		ClientReportsCount,
		case when ClientReportsCount = 0 then null else isnull(sr.amount,0) / ClientReportsCount end as EstAmountPerClient
	from SubReports as sr
	left outer join
	(
		select SubreportId, 
			sum(TotalAmount) as Amount,
			sum(Quantity) as Quantity,
			count(*) as ClientReportsCount
		from viewClientreports as t 
		group by t.SubReportId
	
	) as a1 on sr.id = a1.SubReportId
	join AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
	join services as s on bs.ServiceId = s.Id

