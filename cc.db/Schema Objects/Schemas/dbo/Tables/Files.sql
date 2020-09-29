CREATE TABLE [dbo].[Files]
(
	[Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [UploadDate] DATETIME NOT NULL, 
    [IsLandingPage] BIT NOT NULL DEFAULT (0), 
    [FileEnding] NVARCHAR(50) NOT NULL, 
    [Order] FLOAT NOT NULL DEFAULT (0), 

)
