CREATE TABLE [dbo].[ScSubsidyAmounts]
(
	[LevelId] int not null references dbo.scsubsidylevels(id),
	[FullSubsidy] bit not null,
	[StartDate] date not null check(datepart(day, [StartDate]) = 1),
	[Amount] money,
	constraint FK_ScSubsidyLevels primary key ([LevelId], [FullSubsidy], [StartDate])
)
