CREATE TABLE [dbo].[ClientAmountReport]
(

	Id int not null identity(1,1) primary key,						--4
	--TypeId tinyint not null default(0),
	ClientReportId int not null references dbo.ClientReports(Id),	--4

	ReportDate date not null,										--3
	Quantity decimal(18,6) not null default(0),							--5


	Amount smallmoney not null default(0),							--4
	--Discretionary smallmoney not null default(0),	--emergency
	--PurposeOfGrant mediumString null,				--emergency

	constraint UQ__ClientAmountReport unique(ClientReportId, ReportDate),
)
GO

CREATE NONCLUSTERED INDEX [_dta_index_ClientAmountReport_11_565577053__K2_K1_K3_4] ON [dbo].[ClientAmountReport]
(
	[ClientReportId] ASC,
	[Id] ASC,
	[ReportDate] ASC
)
INCLUDE ( 	[Quantity]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_ClientAmountReport_12_565577053__K2_4_4864] ON [dbo].[ClientAmountReport]
(
	[ClientReportId] ASC
)
INCLUDE ( 	[Quantity]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go