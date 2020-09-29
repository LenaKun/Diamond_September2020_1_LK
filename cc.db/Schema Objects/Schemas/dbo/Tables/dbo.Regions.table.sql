CREATE TABLE [dbo].[Regions]
(
	Id int not null identity(1,1) primary key,
	Name nvarchar(255) unique,
	IncomeCriteriaRequired bit not null default(0)
)
