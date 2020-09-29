CREATE TABLE [dbo].[MainReportStatusAudits]
(
	Id int not null identity(1,1) primary key,
	MainReportId int not null ,
	OldStatusId int not null,
	NewStatusId int not null,
	StatusChangeDate datetime not null,
	UserId int not null,
	
	constraint FK__MainReportsStausAudits__MainReport foreign key (MainReportId) references dbo.MainReports(id),
	constraint FK__MainReportsStausAudits__User foreign key (UserId) references dbo.Users(id),

)
