CREATE TABLE [dbo].[MainReportPostApprovalComments]
(
	CommentId int not null,
	MainReportId int not null,
	constraint PK_MainReportPostApprovalComments primary key (CommentId, MainReportId),
	constraint FK_MainReportPostApprovalComments_MainReports foreign key (MainReportId) references dbo.mainreports(id),
	constraint FK_MainReportPostApprovalComments_Comments foreign key (CommentId) references dbo.Comments(id)
)
