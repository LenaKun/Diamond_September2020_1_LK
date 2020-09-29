CREATE TABLE [dbo].[ClientContacts]
(
	[Id] INT NOT NULL PRIMARY KEY identity,
	ClientId int not null references dbo.clients(id) on delete cascade,
	ContactDate date 
		check(contactDate is null or contactDate <= cast(getDate() as date)),
	ContactedUsing nvarchar(30),
	Contacted varchar(30),
	CcStaffContact nvarchar(30),
	ReasonForContact nvarchar(255),
	ResponseRecievedDate  date
		check(ResponseRecievedDate is null or ResponseRecievedDate <= cast(getDate() as date)),
	EntryDate datetime not null default(getdate()),
	UserId int not null references dbo.users(id),
	[Filename] varchar(255),

)
