CREATE TABLE [dbo].[MedicalEquipmentReports]
(
	[Id] INT NOT NULL IDENTITY PRIMARY KEY, 
    [SubReportId] INT NOT NULL, 
	[ClientId] int NOT null ,
    [EquipmentTypeId] INT NOT NULL, 
    [Quantity]  DECIMAL(18,6) NOT NULL , 
    [Amount] MONEY NOT NULL , 
    CONSTRAINT [FK_MedicalEquipmentReports_SubReports] FOREIGN KEY ([SubReportId]) REFERENCES [dbo].SubReports([Id]), 
    CONSTRAINT [FK_MedicalEquipmentReports_Clients] FOREIGN KEY ([ClientId]) REFERENCES [dbo].Clients([Id]), 
    CONSTRAINT [FK_MedicalEquipmentReports_EquipmentTypes] FOREIGN KEY ([EquipmentTypeId]) REFERENCES [dbo].MedicalEquipmentTypes([Id])
)
