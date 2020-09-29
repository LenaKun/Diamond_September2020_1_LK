CREATE PROCEDURE [dbo].[spHcCapsTableRaw]
	@clientid int,
	@checkPeriodStart date,
	@checkPeriodEnd date
AS
	SELECT * FROM dbo.fnHcCapsTableRaw(@checkPeriodStart, @checkPeriodEnd)
	where ClientId = @clientid
RETURN 0
