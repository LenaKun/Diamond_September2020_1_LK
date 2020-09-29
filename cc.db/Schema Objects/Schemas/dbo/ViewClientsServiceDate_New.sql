CREATE VIEW [dbo].[ViewClientsServiceDate_New]
	as
	

select ClientId, MasterId,
  case when DATEPart(month, LastStartDate)=DatePart(month,LastEndDate)
   then  DATENAME(month, LastStartDate) +' '+ DateName(year,LastStartDate)
   else (DATENAME(month, LastStartDate) +'  - '+ DATENAME(month, DateAdd(day,-1,LastEndDate)) + ' '+ DateName(year,LastStartDate)) 

   end as LastDate
from
(	
select ClientId, MasterId,
LastStartDate,DateAdd(day, -1, LastEndDate) as LastEndDate --,
   from
   ( 
	SELECT     ViewClientReports.ClientId,
 case IsNull(Clients.MasterId,0) when 0 then
 (select IsNull(Max(MasterId),'') FROM [Clients]
  where MasterIdClcd=ViewClientReports.ClientId) else IsNull(Clients.MasterId,'') end as MasterId,
  MAX(MainReports.Start) AS LastStartDate, MAX(MainReports.[End]) AS LastEndDate, MAX(MainReportId) as MaxMainReportId  
   
FROM         ViewClientReports LEFT OUTER JOIN
                      SubReports ON ViewClientReports.SubReportId = SubReports.Id LEFT OUTER JOIN
                      MainReports ON SubReports.MainReportId = MainReports.Id LEFT OUTER JOIN
                      Clients ON ViewClientReports.ClientId = Clients.Id
                      where MainReports.StatusId in (2,8,32)
GROUP BY ViewClientReports.ClientId, Clients.MasterId) x ) y

GO

