CREATE FUNCTION [dbo].[Sc_MonthsCount]
(
	@clientId int,
	@mrStart datetime,
	@mrEnd datetime,
	@sc_min_day int,
	@sc_max_day int,
	@joinDate datetime,
	@leaveDate datetime
)
RETURNS INT
AS
BEGIN
	declare @dates table (mrStart datetime,  mrEnd datetime,  minLeave datetime,  maxJoin datetime);
	declare @d datetime = @mrStart;
	declare @result int;

	while @d < @mrEnd
	begin
		insert into @dates
		select @d, 
			dateadd(MONTH, 1, @d),
			dateadd(DAY, @sc_min_day, @d),
			DATEADD(DAY, @sc_max_day, @d) 
		set @d = DATEADD(month, 1, @d)
	end


	select @result = sum(case when @joinDate < t.maxJoin and (@leaveDate is null or @leaveDate > t.minLeave) then 1 else 0 end)
	from @dates as t
		join HomeCareEntitledPeriod as hcep 
			on hcep.ClientId = @clientId
			and hcep.StartDate < t.mrEnd and (hcep.EndDate is null or hcep.EndDate > t.mrStart)
	
	RETURN @result
END
