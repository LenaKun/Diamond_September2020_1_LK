CREATE PROCEDURE [dbo].[ImportUnmetNeedsOtherProc]
	@ImportId uniqueidentifier,
	@userId int
AS
	set xact_abort on 

	declare @t table(ClientId int, ServiceTypeImportId int);

	with s as (
		select ClientId, ServiceTypeImportId, min(Amount) as Amount from ImportUnmetNeedsOther
		join clients on ImportUnmetNeedsOther.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		where importid = @ImportId
		group by clientId, ServiceTypeImportId
		having count(*) = 1
	),
	t as (
		select UnmetNeedsOther.* 
		from UnmetNeedsOther
		join clients on UnmetNeedsOther.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		
	)
	merge t  as target
	using s as source  on source.ClientId = target.ClientId and source.ServiceTypeImportId = target.ServiceTypeId
	when matched then update set Amount = source.Amount
	output source.ClientId, source.ServiceTypeImportId into @t;

	exec dbo.ImportUnmetNeedsOtherCancelProc @importid

	select @@ROWCOUNT
RETURN 0
