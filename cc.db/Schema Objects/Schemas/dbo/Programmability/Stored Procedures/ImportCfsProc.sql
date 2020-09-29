CREATE PROCEDURE [dbo].[ImportCfsProc]
(
	@id uniqueidentifier,
	@agencyId int,
	@agencyGroupId int,
	@regionId int
)
AS
	declare @importId uniqueidentifier
	select @importid = @id
	declare @t table (Id int, ClientId int);

	merge CfsRows as target
	using (
		
		select min(t.id) as Id, t.ClientId, t.StartDate, t.CfsApproved, t.UpdatedAt, t.UpdatedBy 
		from ImportHcep as t  
			join (
				select  clientsInnerTable.* 
				from dbo.clients as clientsInnerTable
				join Agencies as a on clientsInnerTable.AgencyId = a.Id
				join AgencyGroups as ag on a.GroupId = ag.Id
				join Countries as country on ag.CountryId = country.Id
				and coalesce(@agencyId,clientsInnerTable.agencyId) = clientsInnerTable.agencyId
				and coalesce(@agencyGroupId, a.GroupId) = a.GroupId
				and coalesce(@regionId, country.RegionId) = country.RegionId
			) as c on t.ClientId = c.Id
			where t.ImportId = @importId
			group by t.ClientId, t.StartDate, t.CfsApproved, t.UpdatedAt, t.UpdatedBy
		) as source
		on (target.ClientId = source.ClientId and (target.StartDate is null or (select COUNT(*) from CfsRows where ClientId = source.ClientId) = 1))
	when matched and (target.EndDate is null or target.EndDate > source.StartDate) then
		Update set target.StartDate = source.StartDate,
					target.UpdatedAt = source.UpdatedAt,
					target.UpdatedById = source.UpdatedBy,
					target.CfsApproved = case when target.CfsApproved is null and source.CfsApproved is null then source.StartDate
							when target.CfsApproved is null and source.CfsApproved is not null then source.CfsApproved
							else target.CfsApproved end
	output source.Id, source.ClientId into @t;

	delete s 
		from ImportHcep as s join @t as t on s.Id = t.Id

	if not exists (select top 1 1 from ImportHcep where ImportId = @importId)
		begin
			delete from Imports where Id=@importId;
		end

	select @@ROWCOUNT
RETURN 0
