CREATE TABLE [dbo].[RelatedFunctionalityLevels]
(
	Id int not null identity(1,1) primary key,
	Name mediumString null unique,
)
