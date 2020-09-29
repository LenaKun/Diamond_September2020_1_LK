CREATE TABLE [dbo].[AppBudgetServiceAudit]
(
	[Id] INT NOT NULL identity PRIMARY KEY,
	[Date] datetime not null,

	AppBudgetId int not null ,
		constraint fk_appbudgetserviceaudit_appbudgets foreign key (appbudgetid) references dbo.AppBudgets(Id),
	ServiceId int not null ,
		constraint fk_appbudgetserviceaudit_services foreign key (serviceid) references dbo.[Services](Id),
	AgencyId int not null ,
		constraint fk_appbudgetserviceaudit_agencies foreign key (agencyid) references dbo.Agencies(Id),
	
	CcGrant money not null default(0),
	RequiredMatch money not null default(0),
	AgencyContribution money not null default(0), 
    [Remarks] NVARCHAR(100) NULL,

)
