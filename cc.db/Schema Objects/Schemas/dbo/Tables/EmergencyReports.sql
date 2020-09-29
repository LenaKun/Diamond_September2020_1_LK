CREATE TABLE [dbo].[EmergencyReports]
(
	[Id] INT NOT NULL identity PRIMARY KEY,
	SubReportId int not null references dbo.SubReports(Id), --4
	ClientId int not null references dbo.Clients(Id),		--4
	TypeId int not null references dbo.EmergencyReportTypes(Id),
	ReportDate date not null,								--emergency
	Amount smallmoney null,								--emergency
	Discretionary smallmoney not null default 0,
	
	Remarks nvarchar(255),
	[Total]  as  (Amount + Discretionary) persisted, 
    [UniqueCircumstances] NVARCHAR(MAX) NULL, 
)

go

CREATE NONCLUSTERED INDEX [IX_EmergencyReports_Import]
ON [dbo].[EmergencyReports] ([SubReportId],[ClientId],[TypeId],[ReportDate],[Amount],[Discretionary],[Remarks])

go

CREATE NONCLUSTERED INDEX [_dta_index_EmergencyReports_12_1326627769__K2_6_7] ON [dbo].[EmergencyReports]
(
	[SubReportId] ASC
)
INCLUDE ( 	[Amount],
	[Discretionary]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

GO
CREATE NONCLUSTERED INDEX IX_EmergencyReports_ClientId
ON [dbo].[EmergencyReports] ([ClientId])

GO



CREATE NONCLUSTERED INDEX [_dta_index_EmergencyReports_7_1326627769__K3_K1_K2_6_7] ON [dbo].[EmergencyReports]
(
	[ClientId] ASC,
	[Id] ASC,
	[SubReportId] ASC
)
INCLUDE ( 	[Amount],
	[Discretionary]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_1326627769_1_2] ON [dbo].[EmergencyReports]([Id], [SubReportId])
go

CREATE STATISTICS [_dta_stat_1326627769_1_3_2] ON [dbo].[EmergencyReports]([Id], [ClientId], [SubReportId])
go