CREATE TABLE [dbo].[DccMemberVisits](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SubReportId] [int] NOT NULL,
	[ClientId] [int] NOT NULL,
	[ReportDate] [date] NOT NULL,
 CONSTRAINT [PK__DccMemberVisits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
CREATE UNIQUE INDEX DCC_SubReportId_ClientId_ReportDate
ON DccMemberVisits (SubReportId, ClientId, ReportDate)
GO

ALTER TABLE [dbo].[DccMemberVisits]   ADD  CONSTRAINT [FK__DccMemberVisits__SubRe_] FOREIGN KEY([SubReportId])
REFERENCES [dbo].[SubReports] ([Id])
GO

ALTER TABLE [dbo].[DccMemberVisits] CHECK CONSTRAINT [FK__DccMemberVisits__SubRe_]
GO

ALTER TABLE [dbo].[DccMemberVisits]   ADD  CONSTRAINT [FK__DccMemberVisits_Clien_] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
GO

ALTER TABLE [dbo].[DccMemberVisits] CHECK CONSTRAINT [FK__DccMemberVisits_Clien_]
GO


