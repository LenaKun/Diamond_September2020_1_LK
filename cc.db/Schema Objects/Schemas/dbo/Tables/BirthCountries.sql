CREATE TABLE [dbo].[BirthCountries]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Name] nvarchar(50) not null unique
)
