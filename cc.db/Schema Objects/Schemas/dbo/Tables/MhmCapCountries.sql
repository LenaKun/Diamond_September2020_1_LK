CREATE TABLE [dbo].[MhmCapCountries]
(
	MhmCapId int not null,
		constraint fk_MhmCapCountries_MhmCaps foreign key (MhmCapId) references dbo.MhmCaps(Id) on delete cascade,
	CountryId int not null,
		constraint FK_MhmCapCountries_Countries foreign key (CountryId) references dbo.Countries(Id),

		constraint PK_MhmCapCountries primary key (MhmCapId, CountryId)
)
