CREATE TABLE [dbo].[EmergencyCapCountries]
(
	
	EmergencyCapId int not null,
		constraint fk_emergencycapCountries_EmergencyCaps foreign key (EmergencyCapId) references dbo.EmergencyCaps(Id) on delete cascade,
	CountryId int not null,
		constraint FK_EmergencyCapCountries_Countries foreign key (CountryId) references dbo.Countries(Id),

		constraint PK_EmergencyCapCountries primary key (EmergencyCapId, CountryId)
)
