CREATE PROCEDURE [dbo].[ImportGovHcHoursProc]
	@ImportId uniqueidentifier,
	@userId int
AS
	set xact_abort on 

	declare @t table(ClientId int, StartDate date);

	with s as (
		select ClientId, StartDate, min(Value) as Value from ImportGovHcHours
		join clients on ImportGovHcHours.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		where importid = @ImportId
		group by clientId, startdate
		having count(*) = 1
	),
	t as (
		select GovHcHours.* 
		from GovHcHours
		join clients on GovHcHours.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		
	)
	merge t  as target
	using s as source  on source.ClientId = target.ClientId and source.StartDate = target.StartDate
	when matched then update set Value = source.Value
	when not matched by target then 
		insert (ClientId, StartDate, Value)
		values (source.ClientId, source.StartDate, source.Value)
	output source.ClientId, source.StartDate into @t;

	exec dbo.ImportGovHcHoursCancelProc @importid

	select @@ROWCOUNT
RETURN 0
