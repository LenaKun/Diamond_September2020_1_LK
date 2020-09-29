CREATE TABLE dbo.Users
(
	Id int not null identity(1,1) primary key,
	UniqueId uniqueidentifier not null default(newid()),
	
	RoleId int not null references dbo.Roles(Id),
	AgencyId int null references dbo.Agencies(Id),--agency users
	AgencyGroupId int null references dbo.AgencyGroups(Id),--"SERs"
	RegionId int null references dbo.Regions(Id),--region officers
	
	--membership data
	UserName nvarchar(256) not null unique,
	FirstName nvarchar(50),
	LastName nvarchar(50),

	Email nvarchar(256) NULL,

	Comment nvarchar(max) NULL,

	DecimalDisplayDigits int not null default(2),
	
	[AddToBcc] BIT NOT NULL DEFAULT (0), 
    [TemporaryPassword] BIT NOT NULL DEFAULT (0), 
    [Disabled] BIT NOT NULL DEFAULT (0), 
    constraint CK_Users_Roleid check  
		(
		case when (roleid = 2 or roleid = 16384) and coalesce(agencygroupid, regionid) is not null then 1 else 0 end = 0 and
		case when (roleid = 32 or roleid = 32768) and coalesce(agencyid, regionid) is not null then 1 else 0 end = 0 and
		case when roleid = 8 and coalesce(agencyid, agencygroupid) is not null then 1 else 0 end = 0 
		)
	

	

)
