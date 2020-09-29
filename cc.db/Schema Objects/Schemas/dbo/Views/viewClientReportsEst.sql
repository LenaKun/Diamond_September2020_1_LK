CREATE VIEW [dbo].[viewClientReportsEst]
	AS 


select cr.*, 
		case when ReportsCount = 0 then null else sr.Amount/ReportsCount end as EstAmount,
		case when cr.ReportDate is null then mr.[End] else cr.ReportDate end as EstReportDate
	from SubReports as sr
	join mainreports as mr on sr.MainReportId = mr.Id
	join viewClientReports as cr on sr.id  = cr.SubreportId
	join (
		select SubReportId, 
		sum(totalamount) as SubReportAmount, 
		avg(totalamount) as AvgAmount,
		count(*) as ReportsCount 
		from viewClientReports
		group by SubReportId
	) as t on sr.Id = t.SubreportId

