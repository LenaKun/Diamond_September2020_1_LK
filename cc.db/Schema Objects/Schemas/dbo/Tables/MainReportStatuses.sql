CREATE TABLE [dbo].[MainReportStatuses]
(
	Id int not null primary key,
	Name nvarchar(255) not null unique,
	[FluxxStatusName] nvarchar(255) NULL,
)
