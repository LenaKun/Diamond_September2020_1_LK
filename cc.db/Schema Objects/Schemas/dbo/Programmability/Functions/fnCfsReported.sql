CREATE FUNCTION [dbo].[fnCfsReported]
(
	@NationalId mediumString,
	@Date datetime
)
RETURNS bit
AS
BEGIN
	
	DECLARE @CfsSer int = 71066;
	DECLARE @result bit = 0
	if exists (
		SELECT TOP 1 1
		FROM dbo.apps as app
		join dbo.appbudgets as b on app.id = b.AppId
		join dbo.mainreports as mr on b.id = mr.appbudgetid
		join dbo.subreports as sr on mr.id = sr.mainreportid
		join dbo.clientreports as cr on sr.id = cr.subreportid
		join dbo.clients as c on cr.ClientId = c.Id
		WHERE app.agencygroupid = @cfsser 
		and c.NationalId = @NationalId
		and mr.Start = @Date
	)
		set @result = 1

	RETURN @result;
	
END
