CREATE TABLE [dbo].[History]
(
	Id bigint not null identity(1,1) primary key,
	
	ReferenceId int not null,
	TableName nvarchar(50) not null,
	FieldName nvarchar(255) not null,
	OldValue nvarchar(255) null,
	NewValue nvarchar(255) null,

	UpdatedBy int not null references dbo.Users(Id),
	UpdateDate smalldatetime not null default(getdate())
)
GO

CREATE NONCLUSTERED INDEX IX_History_TFD
ON [dbo].[History] ([TableName],[FieldName],[UpdateDate])
INCLUDE ([ReferenceId])
--created based on missing index for export modified clients query
GO

CREATE NONCLUSTERED INDEX IX_History
ON [dbo].[History] ([ReferenceId],[TableName])
GO


