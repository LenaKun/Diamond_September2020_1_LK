CREATE TABLE [dbo].[EmergencyReportTypes]
(
	[Id] INT NOT NULL PRIMARY KEY,
	Name nvarchar(255) not null unique,
	[Description] nvarchar(255) null

)
