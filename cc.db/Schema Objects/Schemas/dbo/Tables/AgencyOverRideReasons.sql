CREATE TABLE [dbo].[AgencyOverRideReasons]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
    [Name] NVARCHAR(255) NOT NULL unique
)
