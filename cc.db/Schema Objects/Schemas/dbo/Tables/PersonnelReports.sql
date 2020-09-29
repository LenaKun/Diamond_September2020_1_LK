CREATE TABLE [dbo].[PersonnelReports]
(
	[Id] INT NOT NULL identity(1,1) PRIMARY KEY,
	[AppBudgetServiceId] int not null,
	 constraint fk_personnelreports_appbudgetservices foreign key (AppBudgetServiceId) references dbo.AppBudgetServices(Id) on delete cascade,
	[Position] nvarchar(255) not null,
	[PositionType] int not null default(0),
	[Salary] MONEY,
	[PartTimePercentage] smallmoney,
	[ServicePercentage] smallmoney,
	[Remarks] nvarchar(max)
)
