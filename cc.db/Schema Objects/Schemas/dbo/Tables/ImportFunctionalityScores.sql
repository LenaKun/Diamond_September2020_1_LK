CREATE TABLE [dbo].[ImportFunctionalityScores]
(
	[ImportId] uniqueidentifier not null references dbo.Imports(Id) on delete cascade,
	[RowIndex] int not null,

	[Id] int not null identity(1,1) primary key,
	ClientId int not null,
	DiagnosticScore shortDec  null,
	StartDate datetime  null,
	FunctionalityLevelId int  null,
	--tracking
	UpdatedAt datetime not null,
	UpdatedBy int not null,

	
)
