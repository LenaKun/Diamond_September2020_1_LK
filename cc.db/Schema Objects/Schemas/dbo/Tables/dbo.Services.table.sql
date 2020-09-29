CREATE TABLE [dbo].[Services]
(
	Id int not null identity(1,1),
	Name longString not null,
	TypeId int not null ,
	ReportingMethodId int not null default(1),
	[Personnel] BIT NOT NULL DEFAULT 0, 
	[EnforceTypeConstraints] bit not null default(1),
	[Active] bit not null default(1),
	[SingleClientPerYearAgency] bit not null default(0),
	ServiceLevel int not null default(1) check(ServiceLevel in (1,2,3)),
	[ExceptionalHomeCareHours] [bit] NOT NULL DEFAULT ((0)),
	[CoPGovHoursValidation] BIT NOT NULL DEFAULT (0), 
	[FluxxFieldName] nvarchar(255) NULL,
    constraint PK_Services primary key (Id),
	constraint FK_Services_ServiceTypes foreign key(TypeId) references dbo.ServiceTypes(Id) on update cascade,
    constraint UQ__Services unique (TypeId, Name),
	
)
