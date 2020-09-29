CREATE TABLE [dbo].[ClientHcStatuses]
(
	[Id] INT NOT NULL PRIMARY KEY identity,
	[ClientId] int not null references dbo.clients(id) on delete cascade,
	[CountryId] int null references dbo.countries(id),
	[BirthCountryId] int null references dbo.BirthCountries(id),
	[HcStatusId] int null references dbo.HcStatuses(id),
	[ApprovalStatusId] int null references dbo.approvalstatuses(id),
	[FundStatusId] int null references dbo.FundStatuses(id),
	[StartDate] date not null,
	[UpdateDate] datetime not null default(getdate()),
	[Notify] bit not null default(1),
	constraint UQ_CHAS unique (ClientId, StartDate)
)
go


CREATE NONCLUSTERED INDEX [_dta_index_ClientHcStatuses_7_530816953__K2_K8_K1_K5] ON [dbo].[ClientHcStatuses]
(
	[ClientId] ASC,
	[StartDate] ASC,
	[Id] ASC,
	[HcStatusId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_530816953_2_1] ON [dbo].[ClientHcStatuses]([ClientId], [Id])
go

CREATE STATISTICS [_dta_stat_530816953_5_1_2] ON [dbo].[ClientHcStatuses]([HcStatusId], [Id], [ClientId])
go

CREATE STATISTICS [_dta_stat_530816953_8_2_1_5] ON [dbo].[ClientHcStatuses]([StartDate], [ClientId], [Id], [HcStatusId])
go