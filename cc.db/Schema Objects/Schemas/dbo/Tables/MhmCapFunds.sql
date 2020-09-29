CREATE TABLE [dbo].[MhmCapFunds]
(
	MhmCapId int not null,
		constraint fk_MhmCapFunds_MhmCaps foreign key (MhmCapId) references dbo.MhmCaps(Id) on delete cascade,
	FundId int not null,
		constraint fk_MhmCapFunds_Funds foreign key (FundId) references dbo.Funds(Id),

		constraint pk_MhmCapFunds primary key (MhmCapId, FundId)
)
