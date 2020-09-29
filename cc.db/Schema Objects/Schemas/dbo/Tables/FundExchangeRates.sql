CREATE TABLE [dbo].[FundExchangeRates]
(
	[FundId] int not null,
	[CurId] char(3) not null,
	[Value] decimal(19,6) not null,

	constraint FK_FundExchangeRates_FundId foreign key ([FundId]) references[dbo].[Funds]([Id]) on delete cascade,
	constraint FK_FundExchangeRates foreign key ([CurId]) references dbo.Currencies([Id]) on delete cascade,
	constraint PK_FundExchangeRates primary key ([FundId], [CurId]),
)
