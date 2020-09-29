CREATE TABLE [dbo].[AutomatedReports]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [ReportName] NVARCHAR(MAX) NOT NULL, 
    [ReportViewName] NVARCHAR(MAX) NOT NULL
)
