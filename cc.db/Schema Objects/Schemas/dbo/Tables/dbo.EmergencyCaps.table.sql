CREATE TABLE [dbo].[EmergencyCaps]
(
	Id int not null identity(1,1) primary key,
	Name longString not null,
		constraint UQ_EmergencyCaps unique(Name),
	CapPerPerson  DECIMAL(18,6) not null,
	DiscretionaryPercentage  DECIMAL(18,6)  NOT null default(0),
	CurrencyId char(3),
		constraint Fk_EmergencyCaps_Currencies foreign key (CurrencyId) references dbo.Currencies(Id),
	StartDate date not null,
	EndDate date not null,
	constraint CK_EmergencyCaps_start_end_sequential check (EndDate > StartDate),
	Active bit not null default(1),

	UpdatedAt datetime not null default(getdate()),
	UpdatedBy int not null ,
	constraint FK_EmergencyCaps_Users foreign key (UpdatedBy) references dbo.Users(id)


)	
