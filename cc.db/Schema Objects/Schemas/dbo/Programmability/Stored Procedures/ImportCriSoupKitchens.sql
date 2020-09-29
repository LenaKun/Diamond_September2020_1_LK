CREATE PROCEDURE [dbo].[ImportCriSoupKitchens]
	@Id uniqueidentifier
AS

	-- direct parameter passing resulted in timeout. Something to do with parameter sniffing...
	declare @importid uniqueidentifier
	set  @importid=@id

	--table for valid (insertable) data

	declare @t table(id uniqueidentifier);

	with valid as
	(
		select i.SubReportId, 
				i.ClientId
		from ImportClientReports as i
		where i.ImportId = @importId
		group by i.SubReportId, 
					i.ClientId
	)
	merge SoupKitchensReport as target
	using 
		(	
			select	i.SubReportId, 
					i.ClientId
			from valid as i
			group by	i.SubReportId,
						i.ClientId
		) as source
		on	target.SubReportId = source.SubReportId and 
			target.ClientId = source.ClientId
		when not matched by target then
			insert (SubReportId,ClientId)
			values (source.SubReportId, source.ClientId);

	with valid as
	(
		select distinct i.* 
		from ImportClientReports as i
		join (select ClientId, [Date] from ImportClientReports where ImportId = @importid group by ClientId, [Date] having COUNT(*) = 1) as u on i.ClientId = u.ClientId and i.[Date] = u.[Date]
		where i.ImportId = @importId
	)
	merge SKMembersVisits as target
	using
	(
		--ignore duplicate ([date]) records
		select	i.ImportId,
				SoupKitchensReport.Id as SoupKitchensReportId,
				i.[Date]
			from valid as i
				join SoupKitchensReport on I.SubReportId = SoupKitchensReport.SubReportId and
					I.ClientId = SoupKitchensReport.ClientId
			group by i.ImportId, SoupKitchensReport.Id, i.[Date]
	) as source on target.SKReportId = source.SoupKitchensReportId and target.[ReportDate] = source.[date]
	when not matched by target then 
		insert (SKReportId, ReportDate)
		values (source.SoupKitchensReportId, source.[Date])
	output source.ImportId into @t;

	--clean up
	--delete used records
	delete ImportClientReports 
	from ImportClientReports join @t as t on ImportClientReports.ImportId = t.Id
	where ImportId=@importId and ReportTypeId=15;

	--delete the import record
	if not exists (select top 1 1 from ImportClientReports where ImportId = @importId)
	begin
		delete from Imports where Id=@importId;
	end

	SELECT @@ROWCOUNT

RETURN 0