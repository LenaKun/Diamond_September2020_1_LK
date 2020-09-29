CREATE VIEW [dbo].[ViewClientsServiceDate]
	as

SELECT     ViewClientReports.ClientId, Clients.MasterId, MAX(MainReports.Start) AS LastDate
FROM         ViewClientReports LEFT OUTER JOIN
                      SubReports ON ViewClientReports.SubReportId = SubReports.Id LEFT OUTER JOIN
                      MainReports ON SubReports.MainReportId = MainReports.Id LEFT OUTER JOIN
                      Clients ON ViewClientReports.ClientId = Clients.Id
                      where dbo.fnMainReportSubmitted(MainReports.StatusId) = 1
GROUP BY ViewClientReports.ClientId, Clients.MasterId
