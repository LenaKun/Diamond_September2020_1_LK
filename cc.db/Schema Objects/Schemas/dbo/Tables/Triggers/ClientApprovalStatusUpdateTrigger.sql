CREATE TRIGGER [ClientApprovalStatusUpdateTrigger]
	ON [dbo].[Clients]
	FOR UPDATE
	AS
	BEGIN
		SET NOCOUNT ON
		if UPDATE(ApprovalStatusId)
		begin
			insert into HomeCareEntitledPeriod (ClientId, StartDate, EndDate, UpdatedAt, UpdatedBy)
			select i.Id, GETDATE(), null, GETDATE(), i.UpdatedById
			from Inserted i
			where i.ApprovalStatusId in (2, 2048) 
				and not exists (
					select top 1 1
					from HomeCareEntitledPeriod
					where ClientId = i.Id and (EndDate is null or StartDate < GETDATE() and EndDate > GETDATE())
				)
		end
	END
