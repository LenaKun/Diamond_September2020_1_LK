CREATE PROCEDURE [dbo].[ImportCriClientEventsCount]
	@Id uniqueidentifier
AS

	-- direct parameter passing resulted in timeout. Something to do with parameter sniffing...
	declare @iid uniqueidentifier
	set @iid = @id

	--table for valid (insertable) data

	declare @t table (Id uniqueidentifier, SubReportId int, EventDate datetime, JNVCount int, TotalCount int, Remarks nvarchar(255))

	--get the valid data
	insert into @t (id, SubReportId, EventDate, JNVCount, TotalCount, Remarks)
	select i.Id, i.SubReportId, i.[Date], i.JNVCount, i.TotalCount, i.Remarks
	from importClientReports as i
	join SubReports as sr on i.SubReportId=sr.id
	join MainReports as mr on sr.MainReportId = mr.Id

	where 
			(i.ImportId=@iid and i.ReportTypeId=16) 	--relevant import id and type=client events count 
		
			


	merge ClientEventsCountReport as target
	using @t as source on  target.SubReportId = source.SubReportId
		and target.EventDate = source.EventDate				
	when not matched by target then
		insert (SubReportId,EventDate,JNVCount,TotalCount,Remarks)
		values (source.SubReportId,source.EventDate,source.JNVCount,source.TotalCount,source.Remarks)
	when matched then
		update set JNVCount = source.JNVCount, TotalCount = source.TotalCount, Remarks = source.Remarks
	;

	
	--cleanup
	delete s 
	from importclientreports as s join @t as t on s.Id = t.Id
	
	--more cleanup
	if not exists (select top 1 1 from importClientReports as i where i.ImportId=@iid and i.ReportTypeId=16)
	begin
		delete from ImportClientReports 
		where Id = @iId
	end


	SELECT @@ROWCOUNT

RETURN 0

