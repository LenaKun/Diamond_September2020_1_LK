--CCDIAM-451
--Database update on Production deployment
CREATE TABLE [dbo].[_tmp_ClientHcStatuses]
(

	[ClientId] int not null,
	[CountryId] int null,
	[BirthCountryId] int null ,
	[HcStatusId] int null ,
	[ApprovalStatusId] int null ,
	[FundStatusId] int null ,
	[StartDate] date not null,
	[UpdateDate] datetime not null default(getdate()),
	constraint UQ_CHAS unique (ClientId, StartDate)
)
go



begin

	declare @d date = '2016-08-01'
	declare @t table (ClientId int, FundStatusId int, ApprovalStatusId int, BirthCountryId int, CountryId int, startdate datetime);

	;with s as (

		--first old value after the date
		select ReferenceId, FieldName, OldValue
		from
		(
			select *, row_number() over (partition by referenceid, fieldname order by updatedate) as r
			from (
				select id, referenceid, updatedate, fieldname, oldvalue
				from History
				where tablename = 'clients'
				and updatedate > @d
				and fieldname in ('FundStatusId', 'ApprovalStatusId', 'BirthCountryId', 'CountryId')
			) as ext1
		) as ext2
		where r = 1
	)
	,pvt as (
		select referenceid, [FundStatusId], [ApprovalStatusId], [BirthCountryId], [CountryId]
		from s
		pivot 
		(
			max(oldvalue)
			for fieldname in ([FundStatusId], [ApprovalStatusId], [BirthCountryId], [CountryId])
		) as pvt

	)
	insert into @t
	select c.id as ClientId, 
		coalesce(pvt.FundStatusId, c.fundStatusId) as FundStatusId,
		coalesce(pvt.ApprovalStatusId, c.ApprovalStatusId) as ApprovalStatusId,
		coalesce(pvt.BirthCountryId, c.BirthCountryId) as BirthCountryId,
		coalesce(pvt.CountryId, c.CountryId) as CountryId,
		@d as StartDate
	from dbo.clients as c
	left outer join pvt on c.id = pvt.ReferenceId

	merge dbo.[_tmp_ClientHcStatuses] as t
	using(
		select *
		from 
		(
			select ClientId, 
			case when exists(select top 1 1 from fundstatuses where id = FundStatusId) then FundStatusId else null end as FundStatusId, 
			ApprovalStatusId, BirthCountryId, CountryId, hcstatusid, StartDate, 
			dbo.fnHomecareApprovalStatusId(c.FundStatusId, c.ApprovalStatusId, c.BirthCountryId, c.CountryId) as HcStatusId
			from @t as c
		) as tt
		where coalesce(tt.FundStatusId, tt.ApprovalStatusId, tt.HcStatusId) is not null
		
	) as s on t.clientid = s.ClientId and t.startdate = s.startdate
	when not matched by target then
		insert (ClientId, FundStatusId, ApprovalStatusId, BirthCountryId, CountryId, hcstatusid, StartDate)
		values (s.ClientId, s.FundStatusId, s.ApprovalStatusId, s.BirthCountryId, s.CountryId, s.HcstatusId, s.StartDate)
	when matched then
		update set 
		FundStatusId = s.FundStatusId,
		ApprovalStatusId = s.ApprovalStatusId,
		BirthCountryId = s.BirthCountryId,
		CountryId = s.CountryId,
		HcStatusId = s.HcStatusId;

		delete from @t;



	;with newHistory as (
		select Id,
		referenceid, fieldname, cast(updatedate as date) as UpdateDate, NewValue
		from History
		where tablename = 'clients'
		and updatedate >'2016-08-01'
		and fieldname in ('FundStatusId', 'ApprovalStatusId', 'BirthCountryId', 'CountryId')

	), s as (
		select max(id) as Id, referenceid, fieldname,  UpdateDate
		from NewHistory
		where fieldname in ('FundStatusId', 'ApprovalStatusId', 'BirthCountryId', 'CountryId')
		group by referenceid, fieldname, updatedate
	)
	, s0 as (
		select s.ReferenceId, s.FieldName, s.UpdateDate, h.NewValue
		from history as h
		join s on h.id = s.Id
	)
	,pvt as (
		select referenceid, updatedate,[FundStatusId], [ApprovalStatusId], [BirthCountryId], [CountryId]
		from s0
		pivot 
		(
			max(newvalue)
			for fieldname in ([FundStatusId], [ApprovalStatusId], [BirthCountryId], [CountryId])
		) as pvt

		)
	insert into @t
	select
	c.id
	,coalesce(t.fundstatusid, (select top 1 fundstatusid from pvt as i where i.UpdateDate<= t.UpdateDate and i.ReferenceId = t.ReferenceId), c.fundstatusid) as FundStatusId
	,coalesce(t.ApprovalStatusId, (select top 1 ApprovalStatusId from pvt as i where i.UpdateDate<= t.UpdateDate and i.ReferenceId = t.ReferenceId), c.ApprovalStatusId) as ApprovalStatusId
	,coalesce(t.BirthCountryId, (select top 1 BirthCountryId from pvt as i where i.UpdateDate<= t.UpdateDate and i.ReferenceId = t.ReferenceId), c.BirthCountryId) as BirthCountryId
	,coalesce(t.CountryId, (select top 1 CountryId from pvt as i where i.UpdateDate<= t.UpdateDate and i.ReferenceId = t.ReferenceId), c.CountryId) as CountryId
	,t.UpdateDate
	from pvt as t
	join clients as c on t.referenceid = c.id
	order by referenceid, UpdateDate



	merge dbo.[_tmp_ClientHcStatuses] as t
	using(
		select *
		from 
		(
			select ClientId, 
			case when exists(select top 1 1 from fundstatuses where id = FundStatusId) then FundStatusId else null end as FundStatusId, 
			ApprovalStatusId, BirthCountryId, CountryId, hcstatusid, StartDate, 
			dbo.fnHomecareApprovalStatusId(c.FundStatusId, c.ApprovalStatusId, c.BirthCountryId, c.CountryId) as HcStatusId
			from @t as c
		) as tt
		where coalesce(tt.FundStatusId, tt.ApprovalStatusId, tt.HcStatusId) is not null
		
	) as s on t.clientid = s.ClientId and t.startdate = s.startdate
	when not matched by target then
		insert (ClientId, FundStatusId, ApprovalStatusId, BirthCountryId, CountryId, hcstatusid, StartDate)
		values (s.ClientId, s.FundStatusId, s.ApprovalStatusId, s.BirthCountryId, s.CountryId, s.HcstatusId, s.StartDate)
	when matched then
		update set 
		FundStatusId = s.FundStatusId,
		ApprovalStatusId = s.ApprovalStatusId,
		BirthCountryId = s.BirthCountryId,
		CountryId = s.CountryId,
		HcStatusId = s.HcStatusId;


	--delete tt
	--from ClientHcStatuses  as tt
	--where coalesce(tt.FundStatusId, tt.ApprovalStatusId, tt.HcStatusId) is null

	
end