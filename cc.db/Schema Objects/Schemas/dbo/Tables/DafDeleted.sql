CREATE TABLE [dbo].[DafDeleted]
(
	[Id] INT NOT NULL PRIMARY KEY,
	[ClientId] int not null,
	[AgencyId] int not null,

	[EvaluatorId] int not null,

	[StatusId] int not null,
	[AssessmentDate] datetime,
	[EffectiveDate] datetime,
	[Xml] xml,
	[GovernmentHours] shortDec null,
	[ExceptionalHours] shortDec null,
	[Comments] nvarchar(max),
	[EvaluatorPosition] NVARCHAR(MAX) null,

	[SignedAt] datetime,
	[SignedBy] int,

	[ReviewedAt] datetime,
	[ReviewedBy] int,

	[DeletedAt] datetime not null,
	[DeletedBy] int not null,
	
	[CreatedAt] datetime not null,
	[CreatedBy] int not null,

	[UpdatedAt] datetime,
	[UpdatedBy] int,


	


)
