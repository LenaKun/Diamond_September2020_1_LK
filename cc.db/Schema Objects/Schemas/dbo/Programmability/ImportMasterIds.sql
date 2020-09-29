CREATE PROCEDURE [dbo].[ImportMasterIds]

	@ImportId uniqueidentifier,
	@UserId int
AS
	declare @t table(Id int);
	
	merge Clients as t
	using (
		--grouping is used to process duplicate rows instead of throwing exception
		select ClientId, MasterId, UpdatedById
		from ImportClients 
		where ImportId = @ImportId
		group by ClientId, MasterId, UpdatedById
	)as s on t.id = s.ClientId
	when matched and isnull(s.masterid,0) <> isnull(t.masterid,0) then
		update set	MasterId = s.MasterId,
					UpdatedAt = getdate(),
					UpdatedById = coalesce(@UserId, s.UpdatedById)
	;

	delete
	from ImportClients 
	where ImportId = @ImportId

	delete from Imports where Id = @ImportId

	select @@ROWCOUNT

RETURN 0
