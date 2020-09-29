CREATE PROCEDURE [dbo].[DeleteSKReport]

@SubReportId int  	

AS

DELETE sk 
FROM SKMembersVisits as sk
join SoupKitchensReport as skr on sk.SKReportId = skr.Id
where skr.SubReportId=@SubReportId

DELETE FROM SoupKitchensReport
WHERE  SubReportId= @SubReportId;

select @@ROWCOUNT
RETURN 0
