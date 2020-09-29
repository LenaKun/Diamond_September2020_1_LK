CREATE TABLE [dbo].[HCWeeklyCaps]
(
	Id int not null identity(1,1) primary key,
	Name longString not null,
		constraint UQ_HCWeeklyCaps unique(Name),
	CapPerPerson  DECIMAL(18,6) not null,
	CurrencyId char(3),
		constraint Fk_HCWeeklyCaps_Currencies foreign key (CurrencyId) references dbo.Currencies(Id),
	StartDate date not null,
	EndDate date not null,
	constraint CK_HCWeeklyCaps_start_end_sequential check (EndDate > StartDate),
	Active bit not null default(1),

	UpdatedAt datetime not null default(getdate()),
	UpdatedBy int not null ,
	constraint FK_HCWeeklyCaps_Users foreign key (UpdatedBy) references dbo.Users(id)
)
