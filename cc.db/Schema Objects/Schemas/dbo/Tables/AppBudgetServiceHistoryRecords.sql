CREATE TABLE [dbo].[ApprovedAppBudgetServices]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	AppBudgetId int not null,
	ServiceId int not null,
	AgencyId int not null,
	CcGrant money not null default(0),
	RequiredMatch money not null default(0),
	AgencyContribution money not null default(0),
	RecordDate datetime not null
)
