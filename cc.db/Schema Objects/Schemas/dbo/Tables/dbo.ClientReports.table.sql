CREATE TABLE [dbo].[ClientReports]
(
	Id int not null identity(1,1) primary key,				--4

	SubReportId int not null,
	constraint FK_SubReports_ClientReports foreign key (SubReportId) references dbo.SubReports(Id), --4
	ClientId int not null,
	constraint FK_ClientReports_Clients foreign key (ClientId) references dbo.Clients(Id),		--4
	
	Rate smallmoney,	--homecare: rate per mainreport period
	
	Quantity smallmoney null,
	Amount smallmoney null,								--emergency
	Remarks longString null,

    constraint UQ__HomeCareReports unique(SubReportId, ClientId, Rate)
)

GO

CREATE INDEX [IX_ClientReports_ClientId] ON [dbo].[ClientReports] (ClientId)


GO

CREATE NONCLUSTERED INDEX [IX_ClientReports_SubReportId]
ON [dbo].[ClientReports] ([SubReportId])
INCLUDE ([Id],[ClientId],[Quantity],[Amount],[Remarks])

GO

CREATE STATISTICS [_dta_stat_565577053_3_1_2] ON 
[dbo].[ClientAmountReport]([ReportDate], [Id], [ClientReportId])
GO

CREATE NONCLUSTERED INDEX [_dta_index_ClientReports_11_1070626857__K2_K1_K3] ON [dbo].[ClientReports]
(
	[SubReportId] ASC,
	[Id] ASC,
	[ClientId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_ClientReports_12_1070626857__K2_K1_4_6] ON [dbo].[ClientReports]
(
	[SubReportId] ASC,
	[Id] ASC
)
INCLUDE ( 	[Rate],
	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_ClientReports_7_1070626857__K3_K2_1_4_5_6] ON [dbo].[ClientReports]
(
	[ClientId] ASC,
	[SubReportId] ASC
)
INCLUDE ( 	[Id],
	[Rate],
	[Quantity],
	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE STATISTICS [_dta_stat_1070626857_1_3] ON [dbo].[ClientReports]([Id], [ClientId])
go



