CREATE FUNCTION [dbo].[HcCap]
(
	@clientId int,
	@checkPeriodStart datetime,
	@checkPeriodEnd datetime
)
RETURNS decimal(19,4) 
AS 
begin
	return 
	(
		select top 1 HcCap from dbo.HcCapsMonthlyTable(@checkPeriodStart, @checkPeriodEnd) where ClientId = @clientId
	)
end