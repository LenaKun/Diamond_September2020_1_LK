CREATE TABLE [dbo].[Apps]
(
	Id int not null identity(1,1)  primary key,
	FundId int not null ,
		constraint fk_apps_funds foreign key (FundId) references dbo.Funds(Id),
	AgencyGroupId int not null 
		constraint fk_apps_agenrygroups foreign key (agencygroupid) references dbo.AgencyGroups(Id),


	Name longString not null,
	constraint UQ_AppName unique(Name),

	AgencyContribution bit not null default(0),
	CcGrant money not null default(0),
	RequiredMatch money not null default(0),

	StartDate datetime not null ,
	EndDate datetime not null,

	CurrencyId char(3) not null references dbo.Currencies(Id),

	EndOfYearValidationOnly bit not null default(0),
	InterlineTransfer bit not null default(0), 
    [OtherServicesMax] MONEY NULL, 
    [HomecareMin] MONEY NULL, 
    [AdminMax] MONEY NULL,
	[MaxNonHcAmount] money check ([MaxNonHcAmount] is null or [MaxNonHcAmount] >= 0),
	[MaxAdminAmount] money check ([MaxAdminAmount] is null or [MaxAdminAmount] >= 0),
	[HistoricalExpenditureAmount] money check ([HistoricalExpenditureAmount] is null or [HistoricalExpenditureAmount] >= 0), 
	[MaxHcCaseManagementPersonnel] money null,
	[MaxServicesPersonnelOther] money null,
    [AvgReimbursementCost] MONEY NULL,
	[FluxxGrantRequestId] int NULL,
	[FluxxGrantRequestError] nvarchar(255) NULL, 
    [FuneralExpenses] INT NULL,


)
go
CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K8] ON [dbo].[Apps]
(
	[Id] ASC,
	[StartDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K9] ON [dbo].[Apps]
(
	[Id] ASC,
	[EndDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K4_114] ON [dbo].[Apps]
(
	[Id] ASC,
	[Name] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K3_K2_10] ON [dbo].[Apps]
(
	[Id] ASC,
	[AgencyGroupId] ASC,
	[FundId] ASC
)
INCLUDE ( 	[CurrencyId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_6_9850] ON [dbo].[Apps]
(
	[Id] ASC
)
INCLUDE ( 	[CcGrant]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K3_6221] ON [dbo].[Apps]
(
	[Id] ASC,
	[AgencyGroupId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Apps_12_501576825__K1_K2_6478] ON [dbo].[Apps]
(
	[Id] ASC,
	[FundId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Apps_7_501576825__K3_K2_K1_K4_6_10] ON [dbo].[Apps]
(
	[AgencyGroupId] ASC,
	[FundId] ASC,
	[Id] ASC,
	[Name] ASC
)
INCLUDE ( 	[CcGrant],
	[CurrencyId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]