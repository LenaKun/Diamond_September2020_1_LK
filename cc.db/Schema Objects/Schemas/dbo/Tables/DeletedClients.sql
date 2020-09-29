CREATE TABLE [dbo].[DeletedClients]
(
	Id int not null,

	Name nvarchar(255),
	[Address] 	nvarchar(255),
	BirthDate datetime,
	LeaveDate datetime,
	JoinDate datetime,
	DeleteRasonId int,
		CONSTRAINT FK_DeletedClients_ClientDeleteReasons FOREIGN KEY (DeleteRasonId) REFERENCES ClientDeleteReasons(Id),
	DeletedAt datetime not null,
	DeletedByUserId int references dbo.Users(Id),

	constraint PK__DeletedClients primary key(Id, DeletedAt)
)
