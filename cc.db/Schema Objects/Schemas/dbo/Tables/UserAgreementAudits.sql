CREATE TABLE [dbo].[UserAgreementAudits]
(
	[UserId] INT NOT NULL references dbo.Users(id),
	[UserAgreementId] int not null references dbo.UserAgreements(Id),
	constraint pk_useragreementAudits primary key (userid, useragreementid),
	[Date] datetime not null, 
    [IP] CHAR(39) NULL
)
