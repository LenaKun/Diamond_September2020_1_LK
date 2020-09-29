CREATE PROCEDURE [dbo].[ImportUnmetNeedsOtherNewProc]
	@ImportId uniqueidentifier,
	@userId int
AS
	set xact_abort on 

	declare @t table(ClientId int, ServiceTypeImportId int);

	with s as (
		select ClientId, ServiceTypeImportId, min(Amount) as Amount, ag.DefaultCurrency as CUR from ImportUnmetNeedsOther
		join clients on ImportUnmetNeedsOther.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		join dbo.Agencies as a on t.AgencyId = a.Id
		join dbo.AgencyGroups as ag on a.GroupId = ag.Id
		where importid = @ImportId
		group by clientId, ServiceTypeImportId, ag.DefaultCurrency
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
	when not matched by target then 
		insert (ClientId, ServiceTypeId, Amount, CurrencyId)
		values (source.ClientId, source.ServiceTypeImportId, source.Amount, source.CUR)
	output source.ClientId, source.ServiceTypeImportId into @t;

	exec dbo.ImportUnmetNeedsOtherCancelProc @importid

	select @@ROWCOUNT
RETURN 0
