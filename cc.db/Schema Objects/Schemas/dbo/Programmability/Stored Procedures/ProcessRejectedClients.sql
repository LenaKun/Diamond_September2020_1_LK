CREATE PROCEDURE [dbo].[ProcessRejectedClients]
AS
	declare @t table (id int);
	update c set 
		UpdatedById = (select top 1 id from dbo.users where username='sysadmin' or username = 'ccdevadmin'),
		UpdatedAt = getdate(),
		LeaveReasonId = 3 /*other*/, 
		LeaveDate = DATEADD(day, 3, l.UpdateDate),
		AdministrativeLeave = 1,
		LeaveRemarks = 'not eligible'
	output inserted.Id into @t
	from
	dbo.Clients as c
	join approvalstatuses on c.ApprovalStatusId = ApprovalStatuses.Id
	outer apply ( 
		select MAX(updatedate) as UpdateDate
		from dbo.History as h 
		where h.ReferenceId = c.Id and h.TableName='Clients' and h.FieldName = 'ApprovalStatusId'
	) as l
	where
		ApprovalStatuses.Id = 4
		and c.LeaveDate is null and (c.AutoLeaveOverride is null or CONVERT(date, c.AutoLeaveOverride) < CONVERT(date, getdate()))
		and DATEDIFF(HOUR, l.UpdateDate, GETDATE()) > 72

	;merge ClientHcStatuses as t
	using (
		select *
		from 
		(
			select c.ID as ClientId,
				--first hc record backdated to the join date (typically after first fund status update of a new client) 
				case when exists (select top 1 1 from dbo.clienthcstatuses as i2 where i2.clientid = c.Id) then cast(getdate() as date) else c.JoinDate end as Startdate,
				c.joinDate,
				c.ApprovalStatusId,
				c.FundStatusId,
				c.BirthCountryId,
				c.CountryId,
				dbo.fnHomecareApprovalStatusId(c.FundStatusId, c.ApprovalStatusId, c.BirthCountryId, c.CountryId) as HcStatusId
			from @t AS p
			join clients as c on p.id = c.Id
		) as i1 
	) as s on t.ClientId = s.ClientId and t.StartDate = s.StartDate
	when not matched by target then
		insert (clientid, ApprovalStatusId, FundStatusId, BirthCountryId, CountryId, HcStatusId, StartDate)
		values (s.ClientId, s.ApprovalStatusId, s.FundStatusId, BirthCountryId, CountryId, s.HcStatusId, s.StartDate)
	when matched then
		update set HcStatusId = s.HcStatusId, 
			ApprovalStatusId = s.ApprovalStatusId, 
			FundStatusId = s.FundStatusId,
			BirthCountryId = s.BirthCountryId,
			CountryId = s.CountryId
	;

	select * from @t
RETURN 0
