CREATE TABLE [dbo].[ClientEventsCountReport]
(
	[Id] [int] IDENTITY(1,1) NOT NULL primary key,
	[SubReportId] [int] NOT NULL, 
    [EventDate] DATETIME NOT NULL, 
    [JNVCount] INT NOT NULL, 
    [TotalCount] INT NULL, 
    [Remarks] NVARCHAR(255) NULL,
)
GO

CREATE INDEX [IX_ClientEventsCountReport_SubReportId] ON [dbo].[ClientEventsCountReport] ([SubReportId])

GO
ALTER TABLE [dbo].[ClientEventsCountReport]   ADD  CONSTRAINT [FK__ClientEventsCountReport__SubRe__] FOREIGN KEY([SubReportId])
REFERENCES [dbo].[SubReports] ([Id])
GO
