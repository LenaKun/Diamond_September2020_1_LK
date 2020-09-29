CREATE TABLE [dbo].[DaysCentersReports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubReportId] [int] NOT NULL,
	[ClientId] [int] NOT NULL,
	[SubsidesByDcc] [smallmoney] NULL,
	[VisitCost] [smallmoney] NULL,
	[VisitsCount] [int] NULL,
	[Amount] [smallmoney] NULL,
 CONSTRAINT [PK____DaysCentersReports] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[DaysCentersReports]   ADD  CONSTRAINT [FK__DaysCentersReports] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
GO

ALTER TABLE [dbo].[DaysCentersReports] CHECK CONSTRAINT [FK__DaysCentersReports]
GO

ALTER TABLE [dbo].[DaysCentersReports]   ADD  CONSTRAINT [FK__DaysCentersReports__SubRe__] FOREIGN KEY([SubReportId])
REFERENCES [dbo].[SubReports] ([Id])
GO

ALTER TABLE [dbo].[DaysCentersReports] CHECK CONSTRAINT [FK__DaysCentersReports__SubRe__]
GO

ALTER TABLE [dbo].[DaysCentersReports] ADD  CONSTRAINT [DF__VisitsCount]  DEFAULT ((0)) FOR [VisitsCount]
GO


CREATE NONCLUSTERED INDEX [_dta_index_DaysCentersReports_7_566293077__K2_7] ON [dbo].[DaysCentersReports]
(
	[SubReportId] ASC
)
INCLUDE ( 	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go


CREATE NONCLUSTERED INDEX [_dta_index_DaysCentersReports_7_566293077__K3_K2_7] ON [dbo].[DaysCentersReports]
(
	[ClientId] ASC,
	[SubReportId] ASC
)
INCLUDE ( 	[Amount]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
go

CREATE STATISTICS [_dta_stat_566293077_2_3] ON [dbo].[DaysCentersReports]([SubReportId], [ClientId])
go


