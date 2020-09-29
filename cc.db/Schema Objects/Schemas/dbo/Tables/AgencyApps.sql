CREATE TABLE [dbo].[AgencyApps]
(
	[Id] INT NOT NULL identity(1,1) PRIMARY KEY, 
    [AgencyId] INT NOT NULL, 
		constraint fk_agencyapps_agencies foreign key (agencyid) references dbo.Agencies(Id),
    [AppId] INT NOT NULL, 
		constraint fk_agencyapps_apps foreign key (appid) references dbo.Apps(Id),
    [WeekStartDay] INT NOT NULL DEFAULT (0),
	constraint UQ__AgencyApps unique(AgencyId, AppId),
)
go
CREATE NONCLUSTERED INDEX IX_AgencyApps_AgencyId
ON [dbo].[AgencyApps] ([AgencyId])
INCLUDE ([AppId])
GO
