CREATE TABLE [dbo].[MainReportPoComments]
(

	CommentId int not null,
	MainReportId int not null,
	constraint PK_MainReportPoComments primary key (CommentId, MainReportId),
	constraint FK_MainReportPoComments_MainReports foreign key (MainReportId) references dbo.mainreports(id),
	constraint FK_MainReportPoComments_Comments foreign key (CommentId) references dbo.Comments(id)

)
