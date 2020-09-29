CREATE TABLE [dbo].[Resources]
(
	[Culture] varchar(5) not null check(ltrim(rtrim([Culture]))<>'') references dbo.languages(id),
	[Key] varchar(100) not null check(ltrim(rtrim([Key]))<>''),
	[Value] nvarchar(400) not null,
	constraint PK_Resources primary key ([Culture], [Key])
)
