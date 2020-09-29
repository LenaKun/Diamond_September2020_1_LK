CREATE PROCEDURE [dbo].[ImportUnmetNeedsProc]
	@ImportId uniqueidentifier,
	@userId int
AS
	set xact_abort on 

	declare @t table(ClientId int, StartDate date);

	with s as (
		select ClientId, StartDate, min(WeeklyHours) as WeeklyHours from ImportUnmetNeeds
		join clients on ImportUnmetNeeds.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		where importid = @ImportId
		group by clientId, startdate
		having count(*) = 1
	),
	t as (
		select UnmetNeeds.* 
		from UnmetNeeds
		join clients on UnmetNeeds.ClientId = clients.Id
		join dbo.AllowedAgencies(@userId) as t on clients.AgencyId = t.AgencyId
		
	)
	merge t  as target
	using s as source  on source.ClientId = target.ClientId and source.StartDate = target.StartDate
	when matched then update set WeeklyHours = source.WeeklyHours
	when not matched by target then 
		insert (ClientId, StartDate, WeeklyHours)
		values (source.ClientId, source.StartDate, source.WeeklyHours)
	output source.ClientId, source.StartDate into @t;

	exec dbo.ImportUnmetNeedsCancelProc @importid

	select @@ROWCOUNT
RETURN 0
