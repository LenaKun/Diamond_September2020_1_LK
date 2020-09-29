CREATE TABLE [dbo].[AppExchangeRates]
(
	[AppId] int not null,
	[CurId] char(3) not null,
	[Value] decimal(19,6) not null,

	constraint FK_AppExchangeRates_AppId foreign key ([AppId]) references[dbo].[Apps]([Id]) on delete cascade,
	constraint FK_AppExchangeRates foreign key ([CurId]) references dbo.Currencies([Id]) on delete cascade,
	constraint PK_AppExchangeRates primary key ([AppId], [CurId]),
)
