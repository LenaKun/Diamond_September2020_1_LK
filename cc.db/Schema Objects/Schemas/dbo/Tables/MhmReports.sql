CREATE TABLE [dbo].[MhmReports]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [SubReportId] INT NOT NULL, 
	[ClientId] int NOT null ,
    [TypeId] INT NOT NULL, 
    [Quantity]  DECIMAL(18,6) NOT NULL , 
    [Amount] MONEY NOT NULL , 
    CONSTRAINT [FK_MhmReports_MhmReports_SubReports] FOREIGN KEY ([SubReportId]) REFERENCES [dbo].SubReports([Id]), 
    CONSTRAINT [FK_MhmReports_Clients] FOREIGN KEY ([ClientId]) REFERENCES [dbo].Clients([Id]), 
    CONSTRAINT [FK_MhmReports_MhmTypes] FOREIGN KEY ([TypeId]) REFERENCES [dbo].MhmTypes([Id])
)
