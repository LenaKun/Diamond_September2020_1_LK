CREATE TABLE [dbo].[MainReports]
(
	

	Id int not null identity(1,1) primary key,
	AppBudgetId int not null references dbo.AppBudgets(Id),
	Revision bit not null default(0),
	[Start] date not null,
	[End] date not null,
	ExcRate decimal(19,6) not null default(1),
	ExcRateSource nvarchar(255),

	Adjusted bit not null default(0),
	--submission details
	ProgramOverview maxString null,
	AcMeetingHeld bit not null default(0),
	Mhsa maxString null,	--Holocaust Survivor Advisory Committee Minutes

	--status details
	StatusId int not null,
		CONSTRAINT FK_MainReports_Statuses FOREIGN KEY (StatusId) REFERENCES MainReportStatuses(Id),
	RequiresAdminApproval bit not null default(0),
	UpdatedAt datetime not null default(getdate()),
	SubmittedAt datetime default(getdate()),
	ApprovedAt datetime  default(getdate()),
	UpdatedById int not null,
	ApprovedById int ,
	SubmittedById int ,	

	[LastReport] BIT NOT NULL DEFAULT (0), 
    [168OK] DATETIME NULL, 
    [ProgramOverviewFileName] NVARCHAR(MAX) NULL, 
    [MhsaFileName] NVARCHAR(MAX) NULL,
	[FluxxRequestReportId] int NULL,
	[FluxxRequestReportError] nvarchar(255) NULL, 
	[FluxxModelDocumentId] int NULL,
    [FluxxModelDocumentError] nvarchar(255) NULL,
	[FluxxStatusId] int NULL, 
    --constraints
	constraint FK__MainReports__UpdatedBy foreign key (UpdatedById) references dbo.Users(id),
	constraint FK__MainReports__ApprovedBy foreign key (ApprovedById) references  dbo.users(id),
	constraint FK__MainReports__SubmittedBy foreign key (SubmittedById) references dbo.users(id),	

	constraint UQ__MainReports unique (AppBudgetId, Start, Revision),
)
go

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
CREATE NONCLUSTERED INDEX [_dta_index_MainReports_11_1380199967__K1_K12] ON [dbo].[MainReports]
(
	[Id] ASC,
	[StatusId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_MainReports_12_1380199967__K1_4_5] ON [dbo].[MainReports]
(
	[Id] ASC
)
INCLUDE ( 	[Start],
	[End]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_MainReports_12_1380199967__K1_2_3_4_5_8_12_3369] ON [dbo].[MainReports]
(
	[Id] ASC
)
INCLUDE ( 	[AppBudgetId],
	[Revision],
	[Start],
	[End],
	[Adjusted],
	[StatusId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_MainReports_20190911-124646] ON [dbo].[MainReports]
(
	[FluxxStatusId] ASC
)
go


CREATE NONCLUSTERED INDEX [_dta_index_MainReports_7_1380199967__K2_K1_3_4_5_8_12] ON [dbo].[MainReports]
(
	[AppBudgetId] ASC,
	[Id] ASC
)
INCLUDE ( 	[Revision],
	[Start],
	[End],
	[Adjusted],
	[StatusId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

