CREATE TABLE [dbo].[SupportiveCommunitiesReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubReportId] [int] NOT NULL,
	[ClientId] [int] NOT NULL,
	[HoursHoldCost] [smallmoney] NULL,
	[Amount] [smallmoney] NULL,
	[MonthsCount] INT NULL DEFAULT 0,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE INDEX SC_SubReportId_ClientId
ON SupportiveCommunitiesReports (SubReportId, ClientId)
GO


CREATE NONCLUSTERED INDEX [_dta_index_SupportiveCommunitiesReports_7_742293704__K2_5] ON [dbo].[SupportiveCommunitiesReports]
(
	[SubReportId] ASC
)
INCLUDE ( 	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_SupportiveCommunitiesReports_7_742293704__K3_K2_1_5] ON [dbo].[SupportiveCommunitiesReports]
(
	[ClientId] ASC,
	[SubReportId] ASC
)
INCLUDE ( 	[Id],
	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_742293704_1_3_2] ON [dbo].[SupportiveCommunitiesReports]([Id], [ClientId], [SubReportId])
go

