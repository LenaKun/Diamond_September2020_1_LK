CREATE TABLE [dbo].[Comments]
(
	Id int not null identity(1,1) primary key,
	Content maxString not null,
	[Date] datetime not null,
	UserId int not null references dbo.Users(Id), 
    [IsFile] BIT NOT NULL DEFAULT (0), 
    [PostApprovalComment] BIT NOT NULL DEFAULT (0),

)
