CREATE TABLE [dbo].[UnmetNeedsOther]
(
	[ClientId] INT NOT NULL,
    [ServiceTypeId] INT NOT NULL, 
    [Amount] [dbo].[shortDec] NOT NULL, 
    [CurrencyId] CHAR(3) NULL,
	CONSTRAINT [PK_UnmetNeedsOther] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[ServiceTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UnmetNeedsOther]   ADD  CONSTRAINT [FK_UnmetNeedsOther_Clients] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[UnmetNeedsOther]   ADD  CONSTRAINT [FK_UnmetNeedsOther_ServiceTypes] FOREIGN KEY([ServiceTypeId])
REFERENCES [dbo].[ServiceTypes] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[UnmetNeedsOther]   ADD  CONSTRAINT [FK_UnmetNeedsOther_Currencies] FOREIGN KEY([CurrencyId])
REFERENCES [dbo].[Currencies] ([Id])
ON DELETE CASCADE
GO