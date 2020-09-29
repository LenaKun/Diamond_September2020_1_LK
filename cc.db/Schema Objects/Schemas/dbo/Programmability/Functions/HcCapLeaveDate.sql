CREATE FUNCTION [dbo].[HcCapLeaveDate]
(
	@clientId int,
	@checkPeriodStart datetime,
	@checkPeriodEnd datetime
)
RETURNS decimal(19,4) 
AS 
begin
	declare @result decimal(19,4), @prevRes decimal(19,4), @start datetime

	set @start = @checkPeriodStart
	set @result = 0
	set @prevRes = 0
	while @start < @checkPeriodEnd
	begin
		declare @curr decimal(19,4)
		set @curr = (select top 1 HcCap from dbo.HcCapsTableWithoutCarry(@start, DATEADD(DAY, 1, @start)) where ClientId = @clientId)
		if(@curr is not null) set @prevRes = @curr
		if(@prevRes > @result) set @result = @prevRes
		set @start = DATEADD(DAY, 1, @start)
	end

	return @result
end
