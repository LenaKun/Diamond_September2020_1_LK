CREATE TABLE [dbo].[Roles]
(
	Id int not null,
	Name nvarchar(50) not null,

	constraint PK_Roles
		primary key (Id),
)
