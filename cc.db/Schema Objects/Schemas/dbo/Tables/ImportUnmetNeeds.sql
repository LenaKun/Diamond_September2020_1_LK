CREATE TABLE [dbo].[ImportUnmetNeeds](
	[ImportId] [uniqueidentifier] NOT NULL,
	[RowIndex] [int] NOT NULL,
	[ClientId] [int] NULL,
	[StartDate] [datetime] NULL,
	[WeeklyHours] [dbo].[shortDec] NOT NULL
) ON [PRIMARY]

GO
