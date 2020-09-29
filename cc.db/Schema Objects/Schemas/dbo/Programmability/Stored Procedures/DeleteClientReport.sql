CREATE PROCEDURE [dbo].[DeleteClientReport]

@SubReportId int  

	
AS

DELETE ar 
FROM ClientAmountReport as ar
join ClientReports as cr on ar.ClientReportId = cr.Id
where cr.SubReportId=@SubReportId

DELETE FROM ClientReports
WHERE  SubReportId= @SubReportId;

select @@ROWCOUNT
RETURN 0




