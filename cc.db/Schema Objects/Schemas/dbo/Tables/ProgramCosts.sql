CREATE TABLE [dbo].[ProgramCosts]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [ProgramCostTypeId] INT NOT NULL, 
    [Amount] MONEY NOT NULL, 
    [PercentFundedByCC]  DECIMAL(18,6) NOT NULL, 
    [SubReportId] INT NOT NULL, 
    CONSTRAINT [CK_ProgramCosts_PercentFundedByCC] CHECK ( PercentFundedByCC is null or (PercentFundedByCC >=0 and PercentFundedByCC <=100)), 

    CONSTRAINT [FK_ProgramCosts_SubReports] FOREIGN KEY ([SubreportId]) REFERENCES [dbo].[SubReports]([Id]),
    CONSTRAINT [FK_ProgramCosts_ProgramCostType] FOREIGN KEY ([ProgramCostTypeId]) REFERENCES [dbo].[ProgramCostTypes]([Id]), 
    CONSTRAINT [AK_ProgramCosts_Column] UNIQUE (SubReportId, ProgramCostTypeId)
)
