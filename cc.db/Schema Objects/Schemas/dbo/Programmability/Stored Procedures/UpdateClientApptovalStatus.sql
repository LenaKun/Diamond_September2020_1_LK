CREATE PROCEDURE [dbo].[UpdateClientApptovalStatus]
	@OldStatusId int,
	@NewStatusId int,
	@UserId int
AS

	declare @t table(ClientId int, AgencyId int, FirstName nvarchar(50), LastName nvarchar(50), OldApprovalStatusId int, NewApprovalStatusId int, AgencyGroupId int);

	update clients set ApprovalStatusId = @NewStatusId, UpdatedById=@UserId, UpdatedAt=getdate()
	output inserted.id, inserted.AgencyId, inserted.FirstName, inserted.LastName, deleted.ApprovalStatusId, inserted.ApprovalStatusId, a.groupid into @t
	from clients 
	join agencies as a on clients.agencyid = a.id
	where ApprovalStatusId=@OldStatusId and ACPExported = 1 and a.groupid not in (70823,70824,70825)--test agency groups (testser,Test_Ser_CC,JDC FSU TEST SER)

	select * from @t
	
RETURN 0
