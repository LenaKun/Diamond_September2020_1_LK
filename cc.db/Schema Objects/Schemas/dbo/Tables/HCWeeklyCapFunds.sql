CREATE TABLE [dbo].[HCWeeklyCapFunds]
(
	HCWeeklyCapId int not null,
		constraint fk_HCWeeklyCapFunds_HCWeeklyCaps foreign key (HCWeeklyCapId) references dbo.HCWeeklyCaps(Id) on delete cascade,
	FundId int not null,
		constraint fk_HCWeeklyCapFunds_Funds foreign key (FundId) references dbo.Funds(Id),

		constraint pk_HCWeeklyCapFunds primary key (HCWeeklyCapId, FundId)
)
