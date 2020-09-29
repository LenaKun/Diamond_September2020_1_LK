CREATE TABLE [dbo].[ServiceTypes]
(
	Id int not null primary key,
	Name mediumString not null,
	[DoNotReportInUnmetNeedsOther] BIT NOT NULL DEFAULT (0), 
    [ServiceTypeImportId] INT NULL, 
    constraint UQ_ServiceTypes unique(name),
	constraint UQ_ServiceTypes_ImportId unique([ServiceTypeImportId])
)
