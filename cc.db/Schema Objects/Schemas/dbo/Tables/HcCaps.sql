CREATE TABLE [dbo].[HcCaps]
(
	
	[ClientId] int not null,
	[ReportDate] date not null,
	[Quantity] decimal(9,4) not null,
	[MonthlyCap] decimal(9,4) not null,
	[DailyCap] decimal(9,4) not null,
	constraint PK_HcCaps primary key (clientid,reportdate)
)
go

CREATE NONCLUSTERED INDEX [_dta_index_HcCaps_12_2096726522__K1_K2_3] ON [dbo].[HcCaps]
(
	[ClientId] ASC,
	[ReportDate] ASC
)
INCLUDE ( 	[Quantity]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_HcCaps_12_2096726522__K1_K2] ON [dbo].[HcCaps]
(
	[ClientId] ASC,
	[ReportDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
