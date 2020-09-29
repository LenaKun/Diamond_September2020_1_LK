CREATE TABLE [dbo].[EmergencyCapFunds]
(
	EmergencyCapId int not null,
		constraint fk_emergencycapfunds_EmergencyCaps foreign key (EmergencyCapId) references dbo.EmergencyCaps(Id) on delete cascade,
	FundId int not null,
		constraint fk_EmergencyCapFunds_Funds foreign key (FundId) references dbo.Funds(Id),

		constraint pk_emergencyCapFunds_Funds primary key (EmergencyCapId, FundId)
)
