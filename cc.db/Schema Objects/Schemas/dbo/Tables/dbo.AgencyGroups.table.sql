CREATE TABLE [dbo].[AgencyGroups]
(
	Id int not null primary key,
	CanSubmitRevisionReports bit not null default(0),
	
	RequiredMatch bit not null default(0),
	Name longString not null ,
	Addr1 mediumString null,
	Addr2  mediumString null,
	City  mediumString null,
	DisplayName as (Name+ isnull(', '+ City,'')),

	StateId int null references dbo.States(Id),
	CountryId int not null references dbo.Countries(Id),
	ReportingPeriodId int not null default(1),
		CONSTRAINT FK_AgencyGroups_ReportingPeriods FOREIGN KEY (ReportingPeriodId) REFERENCES ReportingPeriods(Id),
	ForceIsraelID bit not null default(0), 
    [ExcludeFromReports] BIT NOT NULL DEFAULT (0), 
    [Support] NCHAR(10) NULL, 
    [SupportiveCommunities] BIT NOT NULL DEFAULT (0), 
    [SC_FullSubsidy] BIT NOT NULL DEFAULT (0), 
	[ScSubsidyLevelId] int references dbo.scsubsidylevels(id), 
    [DayCenter] BIT NOT NULL DEFAULT (0),
	[Culture] varchar(5) references dbo.languages(id), 
    [DefaultCurrency] CHAR(3) NULL references dbo.Currencies(Id), 
    [IsAudit] BIT NOT NULL DEFAULT (0), 
    [CfsDate] DATETIME NULL, 
    [FluxxOrganizationId] INT NULL, 
    [FluxxOrganizationName] NVARCHAR(255) NULL,
)
go

CREATE NONCLUSTERED INDEX [_dta_index_AgencyGroups_7_68195293__K8_K1_2] ON [dbo].[AgencyGroups]
(
	[DisplayName] ASC,
	[Id] ASC
)
INCLUDE ( 	[CanSubmitRevisionReports]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_AgencyGroups_7_68195293__K1_K10_4_7_8] ON [dbo].[AgencyGroups]
(
	[Id] ASC,
	[CountryId] ASC
)
INCLUDE ( 	[Name],
	[City],
	[DisplayName]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_68195293_10_1] ON [dbo].[AgencyGroups]([CountryId], [Id])
go