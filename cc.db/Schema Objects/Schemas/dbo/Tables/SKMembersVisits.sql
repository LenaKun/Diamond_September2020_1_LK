CREATE TABLE [dbo].[SKMembersVisits]
(
	Id int not null identity(1,1) primary key,						--4
	--TypeId tinyint not null default(0),
	SKReportId int not null references dbo.SoupKitchensReport(Id),	--4

	ReportDate date not null,							--4
	--Discretionary smallmoney not null default(0),	--emergency
	--PurposeOfGrant mediumString null,				--emergency

	constraint UQ__SKMembersVisits unique(SKReportId, ReportDate),
)
GO

CREATE NONCLUSTERED INDEX [_dta_index_SKMembersVisits_11_565577053__K2_K1_K3_4] ON [dbo].[SKMembersVisits]
(
	[SKReportId] ASC,
	[Id] ASC,
	[ReportDate] ASC
)
WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_SKMembersVisits_12_565577053__K2_4_4864] ON [dbo].[SKMembersVisits]
(
	[SKReportId] ASC
)
WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go