CREATE TABLE [dbo].[CfsAmountCountries]
(
	CfsAmountId int not null,
		constraint fk_cfsamountCountries_CfsAmounts foreign key (CfsAmountId) references dbo.CfsAmounts(Id) on delete cascade,
	CountryId int not null,
		constraint FK_cfsamountCountries_Countries foreign key (CountryId) references dbo.Countries(Id),

		constraint PK_CfsAmountCountries primary key (CfsAmountId, CountryId)
)
