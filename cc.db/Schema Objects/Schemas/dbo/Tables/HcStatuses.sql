CREATE TABLE [dbo].[HcStatuses]
(
	[Id] INT NOT NULL PRIMARY KEY, 
	[Name] mediumString not null unique,
    [Cap] INT NOT NULL
)
