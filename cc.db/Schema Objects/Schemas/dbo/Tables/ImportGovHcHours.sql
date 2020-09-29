CREATE TABLE [dbo].[ImportGovHcHours]
(
	ImportId uniqueidentifier not null,
	RowIndex int not null,
	ClientId int,
	StartDate datetime,
	Value shortDec
)
