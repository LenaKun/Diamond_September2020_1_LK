CREATE PROCEDURE [dbo].[ProcessNoContact]
AS
	declare @t table (id int);
	update c set 
		UpdatedById = (select top 1 id from dbo.users where username='sysadmin' or username = 'ccdevadmin'),
		UpdatedAt = getdate(),
		LeaveReasonId = 11 /*No Contact (90 days)*/, 
		LeaveDate = getdate(),
		AdministrativeLeave = 1
	output inserted.Id into @t
	from
	dbo.Clients as c
	join approvalstatuses on c.ApprovalStatusId = ApprovalStatuses.Id
	outer apply ( 
		select MAX(updatedate) as UpdateDate
		from dbo.History as h 
		where h.ReferenceId = c.Id and h.TableName='Clients' and h.FieldName = 'ApprovalStatusId' and h.UpdateDate >= '08-16-2014'
	) as l
	where
		c.DeceasedDate is null
		and ApprovalStatuses.Id = 64
		and c.AdministrativeLeave = 0 and (c.AutoLeaveOverride is null or CONVERT(date, c.AutoLeaveOverride) < CONVERT(date, getdate()))
		and DATEDIFF(DAY, l.UpdateDate, GETDATE()) >= 90
	select * from @t
RETURN 0
