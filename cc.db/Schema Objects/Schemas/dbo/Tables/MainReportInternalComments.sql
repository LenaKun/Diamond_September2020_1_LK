CREATE TABLE [dbo].[MainReportInternalComments]
(
	CommentId int not null,
	MainReportId int not null,
	constraint PK_MainReportInternalComments primary key (CommentId, MainReportId),
	constraint FK_MainReportInternalComments_MainReports foreign key (MainReportId) references dbo.mainreports(id),
	constraint FK_MainReportInternalComments_Comments foreign key (CommentId) references dbo.Comments(id)
)
