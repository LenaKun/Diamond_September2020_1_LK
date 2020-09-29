CREATE TABLE [dbo].[HomeCareEntitledPeriod]
(
	Id int not null identity(1,1) primary key,
	
	ClientId int not null,
		constraint FK__HomeCareEntitledPeriods_Clients foreign key (ClientId) references dbo.Clients(Id) on delete cascade,
	
	StartDate datetime not null default(getdate()),
	EndDate datetime null,
	
	UpdatedAt datetime default(getdate()),
	UpdatedBy int not null references dbo.Users(Id),
)

GO

CREATE INDEX [IX_HomeCareEntitledPeriod_ClientId] ON [dbo].[HomeCareEntitledPeriod] (ClientId)
go

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
CREATE STATISTICS [_dta_stat_1173579219_1_2] ON [dbo].[HomeCareEntitledPeriod]([Id], [ClientId])
go
CREATE NONCLUSTERED INDEX [_dta_index_HomeCareEntitledPeriod_11_1173579219__K2_K1_3_4] ON [dbo].[HomeCareEntitledPeriod]
(
	[ClientId] ASC,
	[Id] ASC
)
INCLUDE ( 	[StartDate],
	[EndDate]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
