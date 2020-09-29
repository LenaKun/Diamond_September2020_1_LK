CREATE TABLE [dbo].[MembershipUser]
(
	--stores sensitive information
	Id int not null primary key references dbo.Users(Id) on delete cascade,
	
	LoweredUserName nvarchar(256) not null,
	LoweredEmail nvarchar(256) NULL,

	[Password] nvarchar(128) NOT NULL,
	[PasswordFormat] int NOT NULL,
	[PasswordSalt] nvarchar(128) NOT NULL,
	PasswordQuestion nvarchar(256) NULL,
	PasswordAnswer nvarchar(128) NULL,

	IsApproved bit NOT NULL,
	IsLockedOut bit NOT NULL,

	ExpirationDate datetime,
	
	CreateDate datetime NOT NULL,
	LastLoginDate datetime NULL,
	LastPasswordChangedDate datetime NULL,
	LastLockoutDate datetime NULL,
	FailedPasswordAttemptCount int NULL,
	FailedPasswordAttemptWindowStart datetime NULL,
	FailedPasswordAnswerAttemptCount int NULL,
	FailedPasswordAnswerAttemptWindowStart datetime NULL,

)
go
CREATE NONCLUSTERED INDEX IX_MembershipUser_LoweredUserName
ON [dbo].[MembershipUser] ([LoweredUserName])
