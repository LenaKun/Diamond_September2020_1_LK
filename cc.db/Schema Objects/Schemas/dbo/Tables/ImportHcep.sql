CREATE TABLE [dbo].[ImportHcep]
(
	[ImportId] uniqueidentifier not null references dbo.Imports(Id) on delete cascade,
	[RowIndex] int not null,

	Id int not null identity(1,1) primary key,
	ClientId int,
	StartDate datetime,
	EndDate datetime,
	
	UpdatedAt datetime,
	UpdatedBy int, 
    [CfsApproved] DATETIME NULL ,

	
)
GO
CREATE NONCLUSTERED INDEX IX_ImportHcep
ON [dbo].[ImportHcep] ([ImportId],[ClientId],[Id])