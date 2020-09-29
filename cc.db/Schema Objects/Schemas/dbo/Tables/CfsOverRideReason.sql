CREATE TABLE [dbo].[CfsOverRideReason]
(
	[CfsId] INT NOT NULL, 
	constraint fk_cfsoverridereason_cfs foreign key ([CfsId]) references dbo.[CfsRows](Id),
    [AgencyOverRideReasonId] INT NOT NULL,
	constraint fk_cfsoverridereason_agencyoverridereason foreign key ([AgencyOverRideReasonId]) references dbo.[AgencyOverRideReasons](Id),
	constraint PK_Cfs_AgencyOverRideReasons primary key ([CfsId], [AgencyOverRideReasonId])
)
