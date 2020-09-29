CREATE TABLE [dbo].[Imports]
(
	[Id] uniqueidentifier default(newsequentialid()) PRIMARY KEY,
	[TargetId] int,
	[UserId] int not null references dbo.users(id) on delete cascade,
	[StartedAt] datetime not null
)
