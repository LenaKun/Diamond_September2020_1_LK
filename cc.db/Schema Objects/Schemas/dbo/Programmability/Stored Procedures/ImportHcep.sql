CREATE PROCEDURE [dbo].[ImportHcepProc]
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

	merge HomeCareEntitledPeriod as target
	using (
		
		select min(t.id) as Id, t.ClientId, t.StartDate, t.EndDate, t.UpdatedAt, t.UpdatedBy 
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
			left outer join HomeCareEntitledPeriod as f1 on  f1.clientid=t.clientid and
				((f1.EndDate is null or t.StartDate <= f1.EndDate) and (t.EndDate is null or t.EndDate >= f1.StartDate))
			left outer join importHcep as f2 on f2.ImportId = t.ImportId and f2.clientid=t.clientid and f2.Id <> t.Id  and
				((f2.EndDate is null or t.StartDate <= f2.EndDate) and (t.EndDate is null or t.EndDate >= f2.StartDate))
			where t.ImportId = @importId and f1.Id is null and f2.id is null and (t.EndDate is null or t.EndDate > t.StartDate)
			group by t.ClientId, t.StartDate, t.EndDate, t.UpdatedAt, t.UpdatedBy 
			
		) as source
		on (target.ClientId = source.ClientId and target.StartDate = source.StartDate)
	when matched and target.EndDate <> source.EndDate then
		Update set Target.EndDate = source.EndDate, 
					target.UpdatedAt = source.UpdatedAt,
					target.UpdatedBy = source.UpdatedBy
	when not matched by target then
		insert (ClientId, StartDate, EndDate, UpdatedAt, UpdatedBy)
		Values (source.ClientId, source.StartDate, source.EndDate, source.UpdatedAt, source.UpdatedBy) 
	output source.Id, source.ClientId into @t;

	delete s 
		from ImportHcep as s join @t as t on s.Id = t.Id

	if not exists (select top 1 1 from ImportHcep where ImportId = @importId)
		begin
			delete from Imports where Id=@importId;
		end

	update c
	set EndDate = EOMONTH(GETDATE()), EndDateReasonId = 4
	from @t t
	join CfsRows c on t.ClientId = c.ClientId
	where c.EndDate is null

	select @@ROWCOUNT
RETURN 0
