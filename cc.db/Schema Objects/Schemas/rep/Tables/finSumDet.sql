CREATE TABLE [rep].[finSumDet]
(
	[SubReportId] int not null,
	[ClientId] int not null,
	[Quantity] smallmoney null,
	[Amount] money null,
	constraint PK_finsumdet primary key ([SubReportId], [ClientId])
)
GO
CREATE NONCLUSTERED INDEX IX_finsumdet
ON [rep].[finSumDet] ([SubReportId],[ClientId])
INCLUDE ([Quantity],[Amount])