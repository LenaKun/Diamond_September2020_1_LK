CREATE TABLE [dbo].[ImportUnmetNeedsOther](
	[ImportId] [uniqueidentifier] NOT NULL,
	[RowIndex] [int] NOT NULL,
	[ClientId] [int] NOT NULL,
	[ServiceTypeImportId] [int] NOT NULL,
	[Amount] [dbo].[shortDec] NOT NULL
) ON [PRIMARY]

GO