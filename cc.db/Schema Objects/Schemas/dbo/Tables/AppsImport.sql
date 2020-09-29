CREATE TABLE [dbo].[AppsImport]
(
	RowId int not null identity primary key,
	[Id] uniqueidentifier NOT NULL,
	FundId int,
	AgencyGroupId int,
	Name longString,
	AgencyContribution bit,
	CcGrant money,
	RequiredMatch money,
	StartDate datetime,
	EndDate datetime,
	CurrencyId char(3),
	USDRate money,
	ILSRate money,
	EURRate money,
	EndOfYearValidationOnly bit,
	InterlineTransfer bit,
	[MaxNonHcAmount] money,
	[MaxAdminAmount] money,
	[HistoricalExpenditureAmount] money, 
    [AvgReimbursementCost] MONEY NULL,

)
