CREATE TABLE [dbo].[HCWeeklyCapCountries]
(
	HCWeeklyCapId int not null,
		constraint fk_HCWeeklyCapCountries_HCWeeklyCaps foreign key (HCWeeklyCapId) references dbo.HCWeeklyCaps(Id) on delete cascade,
	CountryId int not null,
		constraint FK_HCWeeklyCapCountries_Countries foreign key (CountryId) references dbo.Countries(Id),

		constraint PK_HCWeeklyCapCountries primary key (HCWeeklyCapId, CountryId)
)
