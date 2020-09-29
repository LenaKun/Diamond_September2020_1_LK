CREATE TABLE [dbo].[ImportClients]
(
	[ImportId] uniqueidentifier not null references dbo.Imports(Id) on delete cascade,
	[RowIndex] int not null,
	Id				int NOT NULL identity(1,1)  primary key, --list

	InternalId nvarchar(100),

	ClientId int,
	MasterId int null,

	AgencyId			int NULL ,	--list

	NationalId			mediumString null,	--list	--details
	NationalIdTypeId	int null ,	--details

	--personal details
	FirstName			mediumString  NULL ,	--list	--insert required
	MiddleName		mediumString  NULL ,	--details
	LastName			mediumString  NULL ,	--list		--insert required

	Phone				mediumString  NULL ,	--list	--details
	BirthDate				smalldatetime NULL ,	--list	--insert required(conditional)	--details

	[Address]			nvarchar(255)  NULL ,	--list	--insert required	--details
	City				mediumString  NULL ,	--list	--insert required	--details

	--CountryId		int not NULL 
	StateId			int  NULL ,	--insert required	--details
	ZIP				shortString  NULL ,		--details

	--Leave/Join
	JoinDate		smalldatetime null,	--list	--details
	LeaveDate		smalldatetime null,	--details
	LeaveReasonId	int null , 	--details

	LeaveRemarks	nvarchar(255) null,
	--Deceased bit not NULL default(0) ,
	DeceasedDate smalldatetime NULL ,

	--eligibility/disability
	ApprovalStatusId int ,	--list--eligibility/disability
	FundStatusId int null ,--eligibility/disability
	IncomeCriteriaComplied bit ,
	IncomeVerificationRequired bit,

	--other
	NaziPersecutionDetails nvarchar(255) null,
	Remarks nvarchar(255) null,

	--Personal Details
	PobCity mediumString  NULL ,		--personalDetails
	BirthCountryId int NULL,	--personalDetails

	PrevFirstName mediumString NULL,	--personalDetails
	PrevLastName mediumString null,

	OtherFirstName mediumString NULL,	--personalDetails
	OtherLastName mediumString NULL,	--personalDetails
	OtherDob smalldatetime null,	--personalDetails
	OtherId  mediumString NULL,	--personalDetails
	OtherIdTypeId int null, 

	OtherAddress  nvarchar(255) NULL,	--personalDetails
	PreviousAddressInIsrael  mediumString NULL,	--personalDetails

	CompensationProgramName [mediumString] null,	--personalDetails

	IsCeefRecipient bit,
	CeefId mediumString null, --A2F08214875W041
	AddCompName mediumString null,
	AddCompId mediumString null,

	--GfHours shortDec null,
	
	ExceptionalHours shortDec null,

	MatchFlag bit NULL ,
	New_Client nvarchar (10)  NULL ,

	Gender INT,
	UpdatedById int not null, 
	UpdatedAt smalldatetime not null default(getdate()),
	CreatedAt smalldatetime not null default(getdate()), 
    [CountryId] INT NULL, 
    [HomecareWaitlist] BIT NULL, 
    [OtherServicesWaitlist] BIT NULL, 
    [CommPrefsId] INT NULL, 
    [CareReceivedId] INT NULL, 
    [MAFDate] DATETIME NULL, 
    [MAF105Date] DATETIME NULL, 
    [UnableToSign] BIT NULL, 
    [HAS2Date] DATETIME NULL ,    --personalDetails

)
GO

CREATE NONCLUSTERED INDEX IX_ImportClients_HC
ON [dbo].[ImportClientReports] ([ImportId],[SubReportId],[Date])
INCLUDE ([ClientId],[Rate],[Remarks])
GO

CREATE NONCLUSTERED INDEX IX_ImportClients_HC1
ON [dbo].[ImportClientReports] ([ImportId],[ClientId],[Date],[Rate])
GO

CREATE NONCLUSTERED INDEX IX_ImportClientsPreview
ON [dbo].[ImportClients] ([ImportId])
INCLUDE ([RowIndex],[InternalId],[ClientId],[AgencyId],[NationalId],[NationalIdTypeId],[FirstName],[MiddleName],[LastName],[BirthDate],[Address],[StateId],[JoinDate],[LeaveDate],[LeaveReasonId],[DeceasedDate],[PobCity],[BirthCountryId],[PrevFirstName],[PrevLastName],[Gender],[CountryId],[CommPrefsId],[CareReceivedId])




