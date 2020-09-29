CREATE TABLE [dbo].[CfsAmounts]
(
	[Id] INT NOT NULL IDENTITY(1, 1) PRIMARY KEY, 
    [Year] INT NOT NULL, 
    [CurrencyId] CHAR(3) NOT NULL references dbo.Currencies(Id), 
    [Level] INT NOT NULL, 
    [Amount] DECIMAL(9,4) NOT NULL
)
