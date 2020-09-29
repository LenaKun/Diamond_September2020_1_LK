CREATE TABLE [dbo].[Currencies]
(
	
	Id char(3) 
		not null 
		Primary key,
	Name mediumString null,
	ExcRate decimal(19,6) not null default (1), --  excrate = usd / cur
		constraint Currencies_ExcRate_not_zero check (ExcRate<>0),
)
