CREATE TABLE [dbo].[FunctionalityScores]
(
	Id int not null identity(1,1) primary key,
	ClientId int not null,
	DiagnosticScore shortDec not null,
	StartDate datetime not null,
	FunctionalityLevelId int not null,
	Errors  nvarchar(max),
	--tracking
	UpdatedAt datetime not null,
	UpdatedBy int not null references dbo.Users(Id),

	--constraints
	constraint FK__FunctionalityScores__FunctionalityLevels foreign key(FunctionalityLevelId) references dbo.FunctionalityLevels(Id),
	constraint FK__FunctionalityScores__Clients foreign key(ClientId) references dbo.Clients(Id) on delete cascade,
	constraint UQ__FunctionalityScores unique (ClientId, StartDate)
)
