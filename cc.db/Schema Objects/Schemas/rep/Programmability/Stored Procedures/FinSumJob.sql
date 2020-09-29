CREATE PROCEDURE [rep].[FinSumJob]
AS
	/* v3 (v2 completed after 2+ hours on a good day while mainly crunching the tempdb io) */
	set xact_abort on

	--calculate amount (average) per single client report where amount is reported on subreport level
	declare @reportscount table(subreportid int, Amount money)
	;with t1 as (
		select subreportid, count(*) as cnt from dbo.ViewClientReports group by subreportid
	),	t2 as (
		select t1.subreportid
			,ISNULL(sr.Amount / NULLIF(t1.cnt, 0), 0) as Amount
		from t1
		join dbo.SubReports as sr on t1.Subreportid = sr.Id
		join dbo.AppBudgetServices as bs on sr.AppBudgetServiceId = bs.Id
		join dbo.Agencies as a on bs.AgencyId = a.Id
		join dbo.AgencyGroups as ag on a.GroupId = ag.id
		join dbo.Services as s on bs.ServiceId = s.Id
		where ag.ExcludeFromReports = 0 and s.reportingmethodid in (2,3,8,15)
	)
	insert into @reportscount(subreportid, Amount)
	select subreportid, Amount
	from t2
	;

	with t as (
		select * from rep.finsumdet
	)
	merge t
	using (
		select t.subreportid, t.clientid, 
			isnull(case when sr.subreportid is not null then t.cnt * sr.Amount else t.amount end,0) as amount,
			isnull(case when sr.subreportid is not null then t.cnt else t.qnt end,0) as quantity
		from 
		(
			select t.subreportid, t.clientid, count(*) as cnt, sum(quantity) as qnt, sum(amount) as amount
			from dbo.ViewClientReports as t
			group by t.subreportid, t.clientid
		) as t
		left outer join @reportscount as sr on t.subreportid = sr.subreportid
	) as s on t.clientid = s.clientid and t.subreportid = s.subreportid
	when not matched by target then
	insert (SubReportId, ClientId, Quantity, Amount)
	values (s.SubReportId, s.ClientId, s.Quantity, s.Amount)
	when matched and not (
		(s.Amount = t.Amount or (s.Amount is null and t.Amount is null)) and
		(s.Quantity = t.Quantity or (s.Quantity is null and t.Quantity is null))
	)
	then
	update set amount = s.amount, quantity = s.quantity
	when not matched by source then delete;

	
	insert into dbo.[Global]([Message], [Date])
	values ('FinSumJob', getdate());
		
RETURN 0
