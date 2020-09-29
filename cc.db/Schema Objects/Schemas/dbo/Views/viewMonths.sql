CREATE VIEW [dbo].[viewMonths]
	AS

select m 	
							from (
							select 1 as m
							union select 2
							union select 3
							union select 4
							union select 5
							union select 6
							union select 7
							union select 8
							union select 9
							union select 10
							union select 11
							union select 12
							) as m
