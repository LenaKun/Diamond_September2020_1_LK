CREATE TABLE [dbo].[States]
(
	Id int not null identity(1,1) primary key,
	Code char(2) not null unique,
	CountryId int not null references dbo.Countries(Id),
	Name nvarchar(255) not null unique,
)
