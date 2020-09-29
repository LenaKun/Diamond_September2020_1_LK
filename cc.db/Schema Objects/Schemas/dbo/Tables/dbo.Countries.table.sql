CREATE TABLE [dbo].[Countries]
(
	Id int not null identity(1,1) primary key,
	Code char(2) not null unique,
	Name nvarchar(255) not null unique,
	IncomeVerificationRequired bit not null default(0),
	CcName nvarchar(255) null,
	CcCode char(2) null,
	RegionId int not null default(1) references dbo.Regions(Id),
	[Culture] varchar(5) references dbo.languages(id)
)
