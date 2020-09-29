
select top 1000 
MainReportId,
SubReportId,
ClientReportId, pvt.[0], pvt.[1], pvt.[2]
from (
	select ar.Quantity,
			ar.clientreportid,
			cr.clientid,
			cr.rate,
			cr.subreportid,
			sr.mainreportid,
			datediff(month, mr.Start, ar.ReportDate) as d
	from clientamountreport as ar
	join ClientReports as cr on ar.ClientReportId = cr.Id
	join SubReports as sr on cr.SubReportId = sr.Id
	join mainreports as mr on sr.mainreportid = mr.id
) as t
pivot (

sum(quantity) 
for d in ([0],[1],[2])
) as pvt



