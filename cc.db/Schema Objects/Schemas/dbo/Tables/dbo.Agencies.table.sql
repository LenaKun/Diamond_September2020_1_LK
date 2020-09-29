CREATE TABLE [dbo].[Agencies]
(
	Id int not null identity(1,1),
	Name longString not null,-- unique,
	GroupId int not null references dbo.AgencyGroups(Id),
	constraint PK_Agencies primary key (Id)
)
go
CREATE NONCLUSTERED INDEX [_dta_index_Agencies_11_245575913__K1] ON [dbo].[Agencies]
(
	[Id] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Agencies_7_857770113__K1_2] ON [dbo].[Agencies]
(
	[Id] ASC
)
INCLUDE ( 	[Name]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Agencies_7_857770113__K3_K1] ON [dbo].[Agencies]
(
	[GroupId] ASC,
	[Id] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go
CREATE NONCLUSTERED INDEX [_dta_index_Agencies_7_857770113__K1_K3] ON [dbo].[Agencies]
(
	[Id] ASC,
	[GroupId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

