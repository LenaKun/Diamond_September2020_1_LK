CREATE TABLE [dbo].[AppBudgets]
(

	Id int not null identity(1,1)  primary key,
	--Name mediumString not null,

	AgencyRemarks maxString null,
	PoRemarks maxString null,
	FormASubmitted bit not null default(0),

	UpdatedAt datetime null,
	UpdatedById int null references dbo.Users(Id),
	
	UpdatedByAgencyAt datetime,
	UpdatedByAgencyId int  references dbo.Users(Id),
	UpdatedBySerAt datetime,
	UpdatedBySerId int references dbo.Users(Id),
	UpdatedByRpoAt datetime,
	UpdatedByRpoId int references dbo.Users(Id),
	UpdatedByGpoAt datetime,
	UpdatedByGpoId int references dbo.Users(Id),

	--AgencyId int not null references dbo.Agencies(Id),
	--AgencyGroupId int not null ,
		--constraint fk_appbudgets_agencygroups foreign key (agencyGroupId) references dbo.AgencyGroups(Id),

	AppId int not null ,
		constraint fk_appbudgets_apps foreign key (appid) references dbo.Apps(Id),			
		constraint uq_appbudget_app unique (AppId),
	
	Revised bit not null default(0),
	ValidUntill datetime null,	--conditional approval Valid Untill
	ValidRemarks maxString null,	--conditiona approval agency remarks

	StatusId int,
		CONSTRAINT FK_AppBudgets_AppBudgetApprovalStatuses FOREIGN KEY (StatusId) REFERENCES AppBudgetApprovalStatuses(Id),
	TotalCcGrant money not null default(0),
	RequiredMatch money not null default(0), 
    FluxxStateUpdatedAt DATETIME NULL, 
    FluxxGrantRequestError NVARCHAR(255) NULL, 
    FluxxModelDocumentId INT NULL, 
    FluxxModelDocumentError NVARCHAR(255) NULL
	


	--constraint U_AppBudgets unique (AppId),
)
go
CREATE NONCLUSTERED INDEX [_dta_index_AppBudgetServices_11_421576540__K1_K2] ON [dbo].[AppBudgetServices]
(
	[Id] ASC,
	[ServiceId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_AppBudgets_12_341576255__K1_K15_8258] ON [dbo].[AppBudgets]
(
	[Id] ASC,
	[AppId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go