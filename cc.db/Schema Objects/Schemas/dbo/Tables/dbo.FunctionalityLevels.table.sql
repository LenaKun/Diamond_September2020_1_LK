CREATE TABLE [dbo].[FunctionalityLevels]
(
	Id int not null identity(1,1) primary key,
	Name mediumString null unique,

	Active bit not null default(0),

	MinScore shortDec not null default(0),
	MaxScore shortDec not null default(0),

	HcHoursLimit int not null default(0),
	StartDate datetime not null default(getdate()),
	
	SubstractGovHours bit not null default(0),

	--todo: change RelatedLevel to RelatedLevelId ?
	RelatedLevel int not null references [dbo].[RelatedFunctionalityLevels](Id),


)
GO

--DATABASE ENGINE TUNING ADVISOR - GRDEV - 2015-08-04
CREATE STATISTICS [_dta_stat_2030630277_5_1_2_4] ON [dbo].[FunctionalityScores]([FunctionalityLevelId], [Id], [ClientId], [StartDate])
GO
CREATE NONCLUSTERED INDEX [_dta_index_FunctionalityScores_11_2030630277__K2_K4_K1_K5] ON [dbo].[FunctionalityScores]
(
	[ClientId] ASC,
	[StartDate] ASC,
	[Id] ASC,
	[FunctionalityLevelId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_FunctionalityScores_11_2030630277__K2_K1_K4_K5] ON [dbo].[FunctionalityScores]
(
	[ClientId] ASC,
	[Id] ASC,
	[StartDate] ASC,
	[FunctionalityLevelId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
