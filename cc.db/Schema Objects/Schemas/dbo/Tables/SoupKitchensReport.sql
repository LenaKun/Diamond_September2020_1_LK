CREATE TABLE [dbo].[SoupKitchensReport]
(
	[Id] [int] IDENTITY(1,1) NOT NULL primary key,
	[SubReportId] [int] NOT NULL,
	[ClientId] [int] NOT NULL,
	constraint UQ__SoupKitchensReport unique([SubReportId], [ClientId])
)
GO

CREATE INDEX [IX_SoupKitchensReport_ClientId] ON [dbo].[SoupKitchensReport] (ClientId)

GO

GO

CREATE STATISTICS [_dta_stat_SoupKitchensReport] ON 
[dbo].[SKMembersVisits]([ReportDate], [Id], [SKReportId])
GO

CREATE NONCLUSTERED INDEX [_dta_index_SoupKitchensReport_K2_K1_K3] ON [dbo].[SoupKitchensReport]
(
	[SubReportId] ASC,
	[Id] ASC,
	[ClientId] ASC
)WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SoupKitchensReport]   ADD  CONSTRAINT [FK__SoupKitchensReport_Clients] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
GO

ALTER TABLE [dbo].[SoupKitchensReport] CHECK CONSTRAINT [FK__SoupKitchensReport_Clients]
GO

ALTER TABLE [dbo].[SoupKitchensReport]   ADD  CONSTRAINT [FK__SoupKitchensReport__SubRe__] FOREIGN KEY([SubReportId])
REFERENCES [dbo].[SubReports] ([Id])
GO

ALTER TABLE [dbo].[SoupKitchensReport] CHECK CONSTRAINT [FK__SoupKitchensReport__SubRe__]
GO