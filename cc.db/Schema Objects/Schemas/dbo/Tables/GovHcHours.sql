CREATE TABLE [dbo].[GovHcHours]
(
	ClientId int not null,
	StartDate date not null ,
	Value shortDec not null,

	constraint PK_GovHcHours primary key (ClientId, StartDate),
	constraint FK_GovHcHours_Clients foreign key (ClientId) references dbo.Clients(Id) on delete cascade,
	constraint CK_GovHcHours_StartDate check (StartDate < getdate()),
	constraint CK_GovHcHours_Value check (Value >= 0)
)
GO 

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
CREATE NONCLUSTERED INDEX [_dta_index_GovHcHours_11_932198371__K1_K2] ON [dbo].[GovHcHours]
(
	[ClientId] ASC,
	[StartDate] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_GovHcHours_11_932198371__K1_K2_3] ON [dbo].[GovHcHours]
(
	[ClientId] ASC,
	[StartDate] ASC
)
INCLUDE ( 	[Value]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_GovHcHours_12_932198371__K1] ON [dbo].[GovHcHours]
(
	[ClientId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go