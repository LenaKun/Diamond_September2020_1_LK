CREATE TABLE [dbo].[FundStatuses]
(
	Id int identity(1,1) primary key,
	Name mediumString not null unique,
	IncomeVerificationRequired bit not null default(0),
	ApprovalStatusName mediumString null,
	HomecareApprovalStatusName mediumString null,
	--ApprovalStatusId int null
	
)
