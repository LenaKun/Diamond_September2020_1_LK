CREATE TRIGGER [CfsOverRideReasonTrigger]
ON [dbo].[CfsOverRideReason]
FOR INSERT,DELETE
AS
BEGIN
	SET NOCOUNT ON

	if (Select Count(*) From inserted) > 0 and (Select Count(*) From deleted) = 0
	begin
		insert into dbo.History(FieldName, NewValue, OldValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
		select 'CFS_OverRideReasons', left(dbo.string_concat(convert(nvarchar(max), co.AgencyOverRideReasonId)), 255), 
			(select top 1 NewValue from History h where h.TableName = 'Clients' and h.ReferenceId = c.ClientId and h.FieldName = 'CFS_OverRideReasons' order by h.UpdateDate desc), 
			c.ClientId, 'Clients', coalesce(c.UpdatedAt, c.CreatedAt), coalesce(c.UpdatedById, c.CreatedById)
		from inserted as i
		join CfsRows as c on i.CfsId = c.Id
		join CfsOverRideReason co on c.Id = co.CfsId
		group by c.ClientId, c.CreatedAt, c.CreatedById, c.UpdatedAt, c.UpdatedById
	end

	if (Select Count(*) From inserted) = 0 and (Select Count(*) From deleted) > 0
	begin
		insert into dbo.History(FieldName, NewValue, OldValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
		select 'CFS_OverRideReasons', left(dbo.string_concat(convert(nvarchar(max), co.AgencyOverRideReasonId)), 255), 
			(select top 1 NewValue from History h where h.TableName = 'Clients' and h.ReferenceId = c.ClientId and h.FieldName = 'CFS_OverRideReasons' order by h.UpdateDate desc), 
			c.ClientId, 'Clients', c.UpdatedAt, c.UpdatedById
		from deleted as i
		join CfsRows as c on i.CfsId = c.Id
		left outer join CfsOverRideReason co on c.Id = co.CfsId
		group by c.ClientId, c.UpdatedAt, c.UpdatedById
	end
END
