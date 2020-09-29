CREATE TABLE [dbo].[MhmTypes]
(
	[Id] INT NOT NULL identity PRIMARY KEY, 
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [AK_MhmTypes_Name] UNIQUE ([Name])
)
