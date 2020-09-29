CREATE TABLE [dbo].[MainReportAgencyComments]
(

	CommentId int not null,
	MainReportId int not null,
	constraint PK_MainReportAgencyComments primary key (CommentId, MainReportId),
	constraint FK_MainReportAgencyComments_MainReports foreign key (MainReportId) references dbo.mainreports(id),
	constraint FK_MainReportAgencyComments_Comments foreign key (CommentId) references dbo.Comments(id)

)
