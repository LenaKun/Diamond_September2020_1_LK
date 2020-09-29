CREATE TABLE [dbo].[UserAgreements]
(
	[Id] INT NOT NULL identity(1,1) PRIMARY KEY,
	Name nvarchar(255) not null,
	[Text] nvarchar(max) not null,
	[Date] datetime not null,
)
