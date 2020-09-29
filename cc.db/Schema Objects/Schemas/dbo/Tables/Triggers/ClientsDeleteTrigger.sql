CREATE TRIGGER [ClientsDeleteTrigger]
	ON [dbo].[Clients]
	FOR DELETE
	AS
	BEGIN
		SET NOCOUNT ON
		delete h
		from dbo.History as h join deleted on h.ReferenceId = deleted.id
		where TableName='Clients'
	END
