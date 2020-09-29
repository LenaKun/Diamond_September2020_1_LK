CREATE TABLE [dbo].[UserAgencyGroups]
(
	[UserId] INT NOT NULL references dbo.Users(id),
	[AgencyGroupId] int not null references dbo.AgencyGroups(id) on delete cascade,
	constraint PK_UserAgencyGroups primary key ([UserId], [AgencyGroupId])

)
