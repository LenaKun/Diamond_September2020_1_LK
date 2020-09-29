CREATE TABLE [dbo].[UnmetNeeds](
	[ClientId] [int] NOT NULL,
	[StartDate] [date] NOT NULL,
	[WeeklyHours] [dbo].[shortDec] NOT NULL,
 CONSTRAINT [PK_UnmetNeeds] PRIMARY KEY CLUSTERED 
(
	[ClientId] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[UnmetNeeds]   ADD  CONSTRAINT [FK_UnmetNeeds_Clients] FOREIGN KEY([ClientId])
REFERENCES [dbo].[Clients] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[UnmetNeeds] CHECK CONSTRAINT [FK_UnmetNeeds_Clients]
GO

ALTER TABLE [dbo].[UnmetNeeds]  ADD  CONSTRAINT [CK_UnmetNeeds_StartDate] CHECK  (([StartDate]<getdate()))
GO

ALTER TABLE [dbo].[UnmetNeeds] CHECK CONSTRAINT [CK_UnmetNeeds_StartDate]
GO

ALTER TABLE [dbo].[UnmetNeeds]  ADD  CONSTRAINT [CK_UnmetNeeds_WeeklyHours] CHECK  (([WeeklyHours]>=(0) and [WeeklyHours]<=168))
GO

ALTER TABLE [dbo].[UnmetNeeds] CHECK CONSTRAINT [CK_UnmetNeeds_WeeklyHours]
GO
