CREATE PROCEDURE dbo.InsertClient

@Id int  ,
@FirstName nvarchar(255),
@LastName nvarchar(255),

@JoinDate smalldatetime ,
@ApprovalStatusId int ,
@UpdatedById int ,
@UpdatedAt smalldatetime ,
@CreatedAt smalldatetime 
	
AS

set identity_insert dbo.clients on
INSERT INTO [dbo].[Clients]
           ([ID]
		   ,[FirstName]
		   ,[LastName]
           ,[JoinDate]
		   ,[ApprovalStatusId]
           ,[UpdatedById]
           ,[UpdatedAt]
           ,[CreatedAt]
           )
     VALUES
           (
@Id   ,
@FirstName,
@LastName,
@JoinDate  ,
@ApprovalStatusId  ,
@UpdatedById  ,
@UpdatedAt  ,
@CreatedAt  
		   )


set identity_insert dbo.clients off

select * from Clients where id=@Id
RETURN @@rowcount
