﻿CREATE TABLE [dbo].[CfsRows]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
	[ClientId] INT NOT NULL,
	constraint fk_cfs_clients foreign key ([ClientId]) references dbo.[Clients](Id),
    [CreatedAt] DATETIME NOT NULL, 
    [CreatedById] INT NOT NULL references dbo.Users(Id), 
    [UpdatedAt] DATETIME NULL, 
    [UpdatedById] INT NULL references dbo.Users(Id), 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [EndDateReasonId] INT NULL,
	constraint fk_cfs_cfsenddatereason foreign key ([EndDateReasonId]) references dbo.[CfsEndDateReasons](Id), 
    [ClientResponseIsYes] BIT NOT NULL DEFAULT (0), 
    [AgencyOverRide] BIT NOT NULL DEFAULT (0), 
    [OverRideDetails] NVARCHAR(MAX) NULL, 
    [CfsApproved] DATETIME NULL, 
    [AgencyRequestorFirstName] NVARCHAR(50) NULL, 
    [AgencyRequestorLastName] NVARCHAR(50) NULL, 
    [AgencyRequestorTitle] NVARCHAR(50) NULL, 
    [EndRequestDate] DATETIME NULL, 
    [CfsAdminRemarks] NVARCHAR(MAX) NULL, 
    [CfsAdminRejected] BIT NOT NULL DEFAULT (0), 
    [CfsAdminInternalRemarks] NVARCHAR(MAX) NULL, 
    [CfsAdminLastUpdate] DATETIME NULL, 
    [OverrideAgencyFirstName] NVARCHAR(50) NULL, 
    [OverrideAgencyLastName] NVARCHAR(50) NULL, 
    [OverrideAgencyTitle] NVARCHAR(50) NULL 
)