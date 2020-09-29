CREATE FUNCTION [dbo].[HcCapWithoutCarry]
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
		select top 1 HcCap from dbo.HcCapsTableWithoutCarry(@checkPeriodStart, @checkPeriodEnd) where ClientId = @clientId
	)
end
