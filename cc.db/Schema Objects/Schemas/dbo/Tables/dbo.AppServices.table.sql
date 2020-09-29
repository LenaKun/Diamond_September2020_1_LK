CREATE TABLE [dbo].[AppServices]
(
	ServiceId int not null ,
		constraint fk_appservices_services foreign key (serviceid) references dbo.[Services](Id),
	AppId int not null ,
		constraint fk_appservices_apps foreign key (appid) references dbo.Apps(Id),

	constraint PK_Services_Apps primary key (ServiceId, AppId)
)
