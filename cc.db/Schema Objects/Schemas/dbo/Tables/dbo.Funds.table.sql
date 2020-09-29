CREATE TABLE [dbo].[Funds]
(
	Id int not null identity(1,1)  primary key,
	MasterFundId int not null references dbo.MasterFunds(Id),

	Name nvarchar(50) not null,

	StartDate date not null,
	EndDate date not null,

	Amount money not null,
	CurrencyCode char(3) not null references dbo.Currencies(Id), 
    [AustrianEligibleOnly] BIT NOT NULL DEFAULT (0),
	[RomanianEligibleOnly] BIT NOT NULL DEFAULT (0), 
    [OtherServicesMax] MONEY NULL, 
    [HomecareMin] MONEY NULL, 
    [AdminMax] MONEY NULL, 
    [FluxxFundId] INT NULL, 
    [FluxxFundName] NVARCHAR(255) NULL,
	--constraint UQ_Funds_Name unique(Name)

)
