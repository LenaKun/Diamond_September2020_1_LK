CREATE PROCEDURE [dbo].[ImportCriSupportiveCommunities]
	@Id uniqueidentifier
AS

	-- direct parameter passing resulted in timeout. Something to do with parameter sniffing...
	declare @iid uniqueidentifier
	set @iid = @id

	--table for valid (insertable) data

	declare @t table (Id uniqueidentifier, SubReportId int, ClientId int, HoursHoldCost money, MonthsCount int)

	--get the valid data
	insert into @t (id, SubReportId, ClientId, HoursHoldCost, MonthsCount)
	select i.Id, i.SubReportId, i.ClientId, i.HoursHoldCost, i.TotalCount
	from importClientReports as i
	
	join Clients as c on i.ClientId = c.id
	join SubReports as sr on i.SubReportId=sr.id
	join MainReports as mr on sr.MainReportId = mr.Id

	where 
			(i.ImportId=@iid and i.ReportTypeId=12) 	--relevant import id and type=supportive community 
		
			


	merge SupportiveCommunitiesReports as target
	using @t as source on  target.SubReportId = source.SubReportId 
				and target.ClientId = source.ClientId 
				
	when not matched by target then
		insert (SubReportId,ClientId,HoursHoldCost,MonthsCount)
		values (source.SubReportId,source.ClientId,source.HoursHoldCost,source.MonthsCount)
	when matched then
		update set hoursholdcost = source.HoursHoldCost,
					MonthsCount = source.MonthsCount
	;

	
	--cleanup
	delete s 
	from importclientreports as s join @t as t on s.Id = t.Id
	
	--more cleanup
	if not exists (select top 1 1 from importClientReports as i where i.ImportId=@iid and i.ReportTypeId=12)
	begin
		delete from ImportClientReports 
		where Id = @iId
	end


	SELECT @@ROWCOUNT

RETURN 0

