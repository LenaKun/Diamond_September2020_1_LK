CREATE TABLE [dbo].[MasterFunds]
(
	Id int not null identity(1,1)  primary key,
	Name nvarchar(50) not null,

	StartDate date not null,
	EndDate date not null,

	Amount money not null,
	CurrencyCode char(3) not null references dbo.Currencies(Id), 
    [FluxxExport] BIT NULL, 
    [FluxxMasterFundId] INT NULL, 
    [FluxxMasterFundName] NVARCHAR(255) NULL,

		
)
