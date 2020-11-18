CREATE PROCEDURE [dbo].[spHcCapsTableRawBefore2021]
	@clientid int,
	@checkPeriodStart date,
	@checkPeriodEnd date
AS
	SELECT * FROM dbo.fnHcCapsTableRawBefore2021(@checkPeriodStart, @checkPeriodEnd)
	where ClientId = @clientid
RETURN 0
