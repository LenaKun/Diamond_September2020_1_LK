CREATE TABLE [dbo].[ReportingPeriods]
(
	Id int not null primary key,
	Name nvarchar(255) not null unique,
)
