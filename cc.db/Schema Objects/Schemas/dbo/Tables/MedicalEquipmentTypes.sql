CREATE TABLE [dbo].[MedicalEquipmentTypes]
(
	[Id] INT NOT NULL identity PRIMARY KEY, 
    [Name] NVARCHAR(150) NOT NULL, 
    CONSTRAINT [AK_MedicalEquipmentTypes_Name] UNIQUE ([Name])
)
