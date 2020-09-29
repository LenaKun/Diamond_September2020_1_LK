CREATE TABLE [dbo].[ProgramCostTypes]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [AK_ProgramCostTypes_Name] UNIQUE ([Name])
)
