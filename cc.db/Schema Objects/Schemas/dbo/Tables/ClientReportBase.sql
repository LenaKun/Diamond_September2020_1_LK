CREATE TABLE [dbo].[ClientReportBase]
(
	[Id] INT NOT NULL PRIMARY KEY,
	SubReportId int not null references dbo.SubReports(Id), --4
	ClientId int not null references dbo.Clients(Id),		--4
)
