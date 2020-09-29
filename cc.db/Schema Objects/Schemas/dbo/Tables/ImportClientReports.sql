CREATE TABLE [dbo].[ImportClientReports]
(
	[Id] uniqueidentifier not null primary key,
	[ImportId] uniqueidentifier not null,
	Constraint Fk__ImportClientReports__Imports foreign key (ImportId) references dbo.Imports(Id) on delete cascade,
	[ReportTypeId] int not null,
	[RowIndex] int,
	[TypeId] int,
	[ClientReportId] int,
	[SubReportId] int,
	[ClientId] int,
	[Date] datetime,
	[Rate] smallmoney,
	[Quantity] smallmoney,
	[Amount] smallmoney,
	[Discretionary] smallmoney,
	[TotalAmount] smallmoney,
	[Remarks] nvarchar(255),
	[Errors] nvarchar(max), 
    [UniqueCircumstances] NVARCHAR(MAX) NULL, 
    [HoursHoldCost] SMALLMONEY NULL, 
    [JNVCount] INT NULL, 
    [TotalCount] INT NULL
	
)
go
CREATE NONCLUSTERED INDEX [IX_ImportClientReports]
ON [dbo].[ImportClientReports] ([ImportId],[ReportTypeId])
INCLUDE ([Id],[TypeId],[SubReportId],[ClientId],[Date],[Amount],[Discretionary],[Remarks])

go
CREATE NONCLUSTERED INDEX [IX_ImportClientReports_XX]
ON [dbo].[ImportClientReports] ([ImportId])
INCLUDE ([SubReportId],[ClientId],[Date],[Rate])

GO
CREATE NONCLUSTERED INDEX [IX__ImportClientReports_XXX]
ON [dbo].[ImportClientReports] ([ImportId],[ReportTypeId])
INCLUDE ([Id],[TypeId],[SubReportId],[ClientId],[Date])
GO


