CREATE PROCEDURE [dbo].[ImportFS]
	@id uniqueidentifier,
	@agencyId int,
	@agencyGroupId int,
	@regionId int
AS
	declare @importId uniqueidentifier
	select @importid = @id
	declare @t table(Id int);

	--select top 1 1 from
	--ImportFunctionalityScores as t1
	--join ImportFunctionalityScores as t2
	--where

	merge FunctionalityScores as target
	using (
		select fs.Id, fs.ClientId, fs.StartDate, fs.DiagnosticScore, fl.Id as FunctionalityLevelId, fs.UpdatedAt, fs.UpdatedBy 
			from 
			(
				select min(Id) as Id from ImportFunctionalityScores 
				where ImportId = @importId 
				group by ClientId, StartDate
				having count(*) = 1
			)  as dfs join
			ImportFunctionalityScores as fs  on dfs.Id = fs.Id
			join (
				select clientsInnerTable.* from dbo.clients as clientsInnerTable
				join Agencies as a on clientsInnerTable.AgencyId = a.Id
				join AgencyGroups as ag on a.GroupId = ag.Id
				join Countries as country on ag.CountryId = country.Id
				and coalesce(@agencyId,clientsInnerTable.agencyId) = clientsInnerTable.agencyId
				and coalesce(@agencyGroupId, a.GroupId) = a.GroupId
				and coalesce(@regionId, country.RegionId) = country.RegionId
			) as c on fs.ClientId = c.Id
			join FunctionalityLevels as fl on (fl.MinScore <= fs.DiagnosticScore and fs.DiagnosticScore <= fl.MaxScore)
			--this will silently prevent import of future functionality scores
			where fs.StartDate < getdate()
		) as source
		on (target.ClientId = source.ClientId and target.StartDate = source.StartDate)
	when matched and target.DiagnosticScore <> source.DiagnosticScore then
		Update set Target.DiagnosticScore = source.DiagnosticScore, 
					target.FunctionalityLevelId = source.FunctionalityLevelId, 
					target.UpdatedAt = source.UpdatedAt,
					target.UpdatedBy = source.UpdatedBy
	when not matched by target then
		insert (ClientId, StartDate, FunctionalityLevelId, DiagnosticScore, UpdatedAt, UpdatedBy)
		Values (source.ClientId, source.StartDate, source.FunctionalityLevelId, source.DiagnosticScore, source.UpdatedAt, source.UpdatedBy)
	output source.Id into @t	
		;

			--clean up
		--delete used records
		delete ImportFunctionalityScores 
		from ImportFunctionalityScores join @t as t on ImportFunctionalityScores.Id = t.Id
		where ImportId=@importId ;

		--delete the import record
		if not exists (select top 1 1 from ImportFunctionalityScores where ImportId = @importId)
		begin
			delete from Imports where Id=@importId;
		end


	select @@ROWCOUNT
RETURN 0
