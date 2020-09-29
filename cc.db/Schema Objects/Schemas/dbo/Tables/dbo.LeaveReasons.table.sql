CREATE TABLE [dbo].[LeaveReasons]
(
	Id	int IDENTITY (1, 1) NOT NULL  primary key, --list
	Name nvarchar(255) not null unique,
)
