CREATE TABLE [dbo].[CfsEndDateReasons]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1), 
    [Name] NVARCHAR(255) NOT NULL unique, 
    [Show] BIT NOT NULL DEFAULT (0)
)
