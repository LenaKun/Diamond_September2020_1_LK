CREATE TABLE Clients 
(
Id				int IDENTITY (1, 1) NOT NULL  primary key, --list
MasterId int null,
MasterIdClcd as coalesce(MasterId, Id),
InternalId nvarchar(100),
AgencyId			int NOT NULL references dbo.Agencies(Id),	--list

NationalId			mediumString null,	--list	--details
NationalIdTypeId	int null references dbo.NationalIdTypes(Id),	--details

--personal details
FirstName			mediumString  NOT NULL ,	--list	--insert required
MiddleName		mediumString  NULL ,	--details
LastName			mediumString  NOT NULL ,	--list		--insert required

Phone				mediumString  NULL ,	--list	--details
BirthDate				datetime NULL check(birthdate < '1948-02-09'), 	--list	--insert required(conditional)	--details

[Address]			nvarchar(255)  NULL ,	--list	--insert required	--details
City				mediumString  NULL ,	--list	--insert required	--details

--CountryId		int not NULL references dbo.Countries(Id),	--insert required	--details
StateId			int  NULL references dbo.States(Id),	--insert required	--details
ZIP				shortString  NULL ,		--details

--Leave/Join
JoinDate		datetime not null,	--list	--details
LeaveDate		datetime null,	--details
LeaveReasonId	int null references dbo.LeaveReasons(Id), 	--details

LeaveRemarks	nvarchar(255) null,
--Deceased bit not NULL default(0) ,
DeceasedDate datetime NULL ,

--eligibility/disability
ApprovalStatusId int not null default(0) references dbo.ApprovalStatuses(Id),	--list--eligibility/disability
FundStatusId int null references dbo.FundStatuses(Id),--eligibility/disability
IncomeCriteriaComplied bit not null default(0),
IncomeVerificationRequired bit not null default(0),

--other
NaziPersecutionDetails nvarchar(255) null,
Remarks nvarchar(255) null,

--Personal Details
PobCity mediumString  NULL ,		--personalDetails

BirthCountryId int null references dbo.BirthCountries(Id),
EmigrationDate	datetime null,	--personalDetails

PrevFirstName mediumString NULL,	--personalDetails
PrevLastName mediumString null,

OtherFirstName mediumString NULL,	--personalDetails
OtherLastName mediumString NULL,	--personalDetails
OtherDob datetime null,	--personalDetails
OtherId  mediumString NULL,	--personalDetails
OtherIdTypeId int null references dbo.NationalIdTypes(Id),

OtherAddress  nvarchar(255) NULL,	--personalDetails
PreviousAddressInIsrael  mediumString NULL,	--personalDetails

CompensationProgramName [mediumString] null,	--personalDetails

IsCeefRecipient bit not null default(0),
CeefId mediumString null, --A2F08214875W041
AddCompName mediumString null,
AddCompId mediumString null,

--GfHours shortDec null,

ExceptionalHours shortDec null,

MatchFlag bit NULL ,
New_Client nvarchar (10)  NULL ,

ACPExported bit not null default(0),
Gender int,
	CONSTRAINT FK_Clients_Genders FOREIGN KEY (Gender) REFERENCES Genders(Id),
UpdatedById int not null references dbo.Users(Id),
UpdatedAt datetime not null default(getdate()),
CreatedAt datetime not null default(getdate()),
AdministrativeLeave bit not null default(0),


    [AustrianEligible] BIT NOT NULL DEFAULT (0), 
    [GGReportedCount] INT NULL, 
	[ApprovalStatusUpdated] [datetime] NULL
    constraint FK__Clients_MasterClients foreign key (masterid) references dbo.Clients(id), 
    [RomanianEligible] BIT NOT NULL DEFAULT (0), 
    [CountryId] INT NULL references dbo.Countries(Id),
	[DCC_Subside] [int] NULL, 
    [DCC_VisitCost] SMALLMONEY NULL,
	[SC_MonthlyCost] SMALLMONEY  NULL, 
    [DCC_Client] BIT NOT NULL DEFAULT (0) , 
    [SC_Client] BIT NOT NULL DEFAULT (0), 
    [AutoLeaveOverride] DATETIME NULL, 
    [MAFDate] DATETIME NULL, 
    [HomecareWaitlist] BIT NOT NULL DEFAULT (0), 
    [OtherServicesWaitlist] BIT NOT NULL DEFAULT (0), 
    [CommPrefsId] INT NULL
	constraint FK__Clients_CommPrefs foreign key ([CommPrefsId]) references dbo.CommunicationsPreference(Id), 
    [CareReceivedId] INT NULL foreign key ([CareReceivedId]) references dbo.CareReceivingOptions(Id), 
    [MAF105Date] DATETIME NULL, 
    [UnableToSign] bit NOT NULL DEFAULT (0), 
    [HAS2Date] DATETIME NULL, 

)
GO

--this constraint cant be created while there are still duplicate national ids in the table
--CCDIAM-1028
CREATE UNIQUE NONCLUSTERED INDEX [UQ_ISERAELID] ON [dbo].[Clients] (NationalId, AgencyId) where NationalIdTypeId = 1 
GO

CREATE INDEX [IX_Clients_JoinDate] ON [dbo].[Clients] (JoinDate)
GO

--used to seed homecare caps query
CREATE NONCLUSTERED INDEX IX_Clients_Agency_MasterId
ON [dbo].[Clients] ([AgencyId])
INCLUDE ([Id],[MasterId])
GO

--homecare report details timeout
CREATE NONCLUSTERED INDEX IX_Clients_HCReportFilter
ON [dbo].[Clients] ([AgencyId])
INCLUDE ([Id],[FundStatusId],[IncomeCriteriaComplied])
GO

CREATE NONCLUSTERED INDEX [IX_Clients_NationalId]
ON [dbo].[Clients] ([NationalId])
GO

CREATE NONCLUSTERED INDEX [_dta_index_Clients_12_1053246807__K4_K1_K21_K56_K16_K54_K57_K17_K20_K7_9] ON [dbo].[Clients]
(
	[AgencyId] ASC,
	[Id] ASC,
	[ApprovalStatusId] ASC,
	[ApprovalStatusUpdated] ASC,
	[JoinDate] ASC,
	[AustrianEligible] ASC,
	[RomanianEligible] ASC,
	[LeaveDate] ASC,
	[DeceasedDate] ASC,
	[FirstName] ASC
)
INCLUDE ( 	[LastName]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Clients_12_1053246807__K1_2] ON [dbo].[Clients]
(
	[Id] ASC
)
INCLUDE ( 	[MasterId]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE NONCLUSTERED INDEX [_dta_index_Clients_12_1053246807__K2_K1] ON [dbo].[Clients]
(
	[MasterId] ASC,
	[Id] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Clients_12_1097770968__K3_K2_1_5_17_18_45] ON [dbo].[Clients]
(
	[MasterIdClcd] ASC,
	[MasterId] ASC
)
INCLUDE ( 	[Id],
	[AgencyId],
	[JoinDate],
	[LeaveDate]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Clients_7_665769429__K5_K1_K22_2_4_6_8_10_11_12_13_14_17_18_19_21_53_55_56_58] ON [dbo].[Clients]
(
	[AgencyId] ASC,
	[Id] ASC,
	[ApprovalStatusId] ASC
)
INCLUDE ( 	[MasterId],
	[InternalId],
	[NationalId],
	[FirstName],
	[LastName],
	[Phone],
	[BirthDate],
	[Address],
	[City],
	[JoinDate],
	[LeaveDate],
	[LeaveReasonId],
	[DeceasedDate],
	[CreatedAt],
	[AustrianEligible],
	[GGReportedCount],
	[RomanianEligible]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_665769429_41_5] ON [dbo].[Clients]([IsCeefRecipient], [AgencyId])
go

CREATE STATISTICS [_dta_stat_665769429_22_5] ON [dbo].[Clients]([ApprovalStatusId], [AgencyId])
go
