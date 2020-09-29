CREATE PROCEDURE [dbo].[spValidateCrHc]
	@mainReportId int,
	@clientId int
AS

	set nocount on
	
	declare @mrid int,
			@reportdate date,
			@q money,
			@c money,
			@wc money,
			@ac money,
			@d money;

	declare @rt table(clientid int, reportdate date, q money, c money, co money);


	declare c cursor for
		select mrid, clientid, reportdate, COALESCE(q, 0) AS Q, COALESCE(c, 0) AS C, COALESCE(WC, 0) AS WC
		from viewHcPlus
		where mrid = @mainreportid and clientid = @clientId
		order by clientid, reportdate;
	open c;
	fetch next from c into @mrid, @clientid, @reportdate, @q, @c, @WC
	while @@FETCH_STATUS = 0 
	begin
	
		begin --shared code
				--grant 12 additional days on first month in a year or after a month without reports
			set @ac = coalesce(@ac,12);
			
			begin
				-- cap overflow in days (@wc could be zero)
				if @wc = 0 begin set @d = 0 end
				else begin set @d = (@q - @c) * 7/ @wc end
				
				--accumulate the difference in cap and hours
				set @ac = @ac - @d
			
				--the additioinalc cap should not get over 12 days
				set @ac  = case when @ac > 12 then 12 else @ac end
			
				--save the result if the additional cap gets below zero 
				--(current cap and additional cap from prev month does not allow current hours to be reported)
				if @ac < 0 OR (@Q > 0 AND @WC = 0) --and @mrid is not null 
					insert into @rt values(@clientid, @reportdate, @q, @c, @ac * @wc / 7)
			end
		end

		fetch next from c into @mrid, @clientid, @reportdate, @q, @c, @WC
	end
	close c
	
	set nocount off
	select * from @rt

RETURN 0
