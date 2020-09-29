CREATE TABLE [dbo].[GrandfatherHours]
(
	ClientId int not null,
	StartDate date not null,
	Value shortDec not null,
	Type int not null,
	UpdatedAt datetime not null,
    UpdatedBy int not null references dbo.Users(Id),
    constraint PK_GFHours primary key (ClientId, StartDate),
	constraint FK_GFHours_Clients foreign key (ClientId) references dbo.Clients(Id) on delete cascade,
	constraint CK_GFHours_Value check (Value >= 0 and Value <= 168)
)
GO 

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
CREATE NONCLUSTERED INDEX [_dta_index_GFHours_11_932198371__K1_K2] ON [dbo].[GrandfatherHours]
(
	[ClientId] ASC,
	[StartDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_GFHours_11_932198371__K1_K2_3] ON [dbo].[GrandfatherHours]
(
	[ClientId] ASC,
	[StartDate] ASC
)
INCLUDE ( 	[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_GFHours_12_932198371__K1] ON [dbo].[GrandfatherHours]
(
	[ClientId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
