CREATE TABLE [dbo].[ServiceConstraints]
(
	Id int not null identity,
	[ServiceId] int null,
	[ServiceTypeId] int null,
	[FundId] int null,

	[MinExpPercentage] decimal(9,8),
	[MaxExpPercentage] decimal(9,8),
	[Fatal] bit not null default(0),

	constraint PK_ServiceConstraints primary key(Id),
	constraint UQ_ServiceConstraints unique (ServiceId, ServiceTypeId, FundId),
	constraint CK_ServiceConstraints_Service check (coalesce(serviceId, ServiceTypeId) is not null),
	constraint CK_Services_MinExpPercentage check (MinExpPercentage is null or [MinExpPercentage] between 0 and 1),
	constraint CK_Services_MaxExpPercentage check ([MaxExpPercentage] is null or [MaxExpPercentage] between 0 and 1),
	constraint FK_ServiceConstraints_Services foreign key ([ServiceId]) references [dbo].[Services]([Id]),
	constraint FK_ServiceConstraints_ServiceTypes foreign key ([ServiceTypeId]) references [dbo].[ServiceTypes]([Id]) on update cascade,
	constraint FK_ServiceConstraints_Funds foreign key ([FundId]) references [dbo].[Funds]([Id]),
)
