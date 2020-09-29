CREATE VIEW [dbo].[viewDatesHistory]
	AS 
	
	SELECT [Id]
		,[ReferenceId]
		,[TableName]
		,[FieldName]
		,CASE WHEN OldValue is null or isdate(oldvalue) = 0 then null
			else CONVERT(DATETIME, OLDVALUE)
			end AS OldValue
		,CASE WHEN NewValue is null or isdate(Newvalue) = 0 then null
			else CONVERT(DATETIME, NewVALUE)
			end AS NewValue
		,[UpdatedBy]
		,[UpdateDate]
	FROM [dbo].[History]