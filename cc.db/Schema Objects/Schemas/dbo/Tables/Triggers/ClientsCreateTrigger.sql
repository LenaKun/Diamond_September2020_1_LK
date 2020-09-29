CREATE TRIGGER [ClientsCreateTrigger]
ON [dbo].[Clients]
	FOR INSERT
	AS
	BEGIN
		SET NOCOUNT ON

		if (Select Count(*) From inserted) > 0 and (Select Count(*) From deleted) = 0
		begin
			insert into dbo.History(FieldName, NewValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'CreatedAt', i.CreatedAt, i.Id, 'Clients', i.CreatedAt, i.UpdatedById from inserted as i
		end
	END
