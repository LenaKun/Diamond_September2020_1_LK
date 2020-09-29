CREATE TABLE [dbo].[AppBudgetServices]
(
	Id int not null identity(1,1) primary key,
	ServiceId int not null ,
		constraint fk_appbudgetservices_services foreign key (serviceid) references dbo.[Services](Id),
	AgencyId int not null ,
		constraint fk_appbudgetservices_agencies foreign key (agencyid) references dbo.Agencies(Id),
	AppBudgetId int not null ,
		constraint fk_appbudgetservices_appbudgets foreign key (appbudgetid) references dbo.AppBudgets(Id),
	CcGrant money not null default(0),
	RequiredMatch money not null default(0),
	AgencyContribution money not null default(0), 
    [Remarks] NVARCHAR(100) NULL,

	[OriginalCcGrant] MONEY NULL, 
    constraint UQ__AppBudgetServices unique(ServiceId, AgencyId, AppBudgetId),
	
)
go
CREATE NONCLUSTERED INDEX [_dta_index_AppBudgetServices_12_421576540__K2_K4_K1_5] ON [dbo].[AppBudgetServices]
(
	[ServiceId] ASC,
	[AppBudgetId] ASC,
	[Id] ASC
)
INCLUDE ( 	[CcGrant]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX IX_AppBudgetServices_AgencyId
ON [dbo].[AppBudgetServices] ([AgencyId])
INCLUDE ([ServiceId])
GO
