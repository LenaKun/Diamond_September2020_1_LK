CREATE PROCEDURE [dbo].[spValidateMrHcD]
	@mainreportid int
AS

	set nocount on
	
	declare @mrid int,
			@clientid int,
			@reportdate date,
			@q money,
			@c money,
			@wc money,
			@ac money,
			@d money,
			@p_clientid int;

	declare @rt table(clientid int, reportdate date, q money, c money, co money);


	declare c cursor for
		select mrid, clientid, reportdate, q, c, WC
		from viewHcD
		where mrid = @mainreportid
		order by clientid, reportdate;
	open c;
	fetch next from c into @mrid, @clientid, @reportdate, @q, @c, @WC
	while @@FETCH_STATUS = 0 
	begin
	
		--reset additional cap on client change
		if not (@p_clientid = @clientid) set @ac = null;
		
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
		
		--save current clientid for next pass
		select @p_clientid = @clientid
		fetch next from c into @mrid, @clientid, @reportdate, @q, @c, @WC
	end
	close c
	
	set nocount off

	select distinct t.ClientId, s.Name as ServiceName
	from SubReports as sr
		join AppBudgetServices as appbs on sr.AppBudgetServiceId = appbs.id
		join Services as s on appbs.ServiceId = s.Id
		join ClientReports as cr on sr.Id = cr.SubReportId
		join @rt as t on cr.ClientId = t.clientid
	

RETURN 0
