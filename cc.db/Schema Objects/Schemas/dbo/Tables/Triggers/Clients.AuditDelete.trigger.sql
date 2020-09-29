CREATE TRIGGER [ClientsTableAuditDeleteTrigger]
	ON [dbo].[Clients]
	with execute as caller
	AFTER delete
AS 
begin

	select * into #tempinserted from inserted
	select * into #tempdeleted from deleted

	
	declare @bit int ,
	@field int ,
	@char int
	select @field = 0
	declare @tablename varchar(255)
	set @tablename='Clients'
	declare @fieldName varchar(255)
	declare @insertquery varchar(max)

	while @field < (select max(colid) from syscolumns where id = (select id from sysobjects where name = @tablename))
	begin
		select @field = @field + 1
		select @bit = (@field - 1 )% 8 + 1
		select @bit = power(2,@bit - 1)
		select @char = ((@field - 1) / 8) + 1

		--print @char + ' '+  @field + ' '+ @bit -- debug code to check the bits that are tested.
		if substring(COLUMNS_UPDATED(),@char, 1) & @bit > 0
		begin
			select @fieldName=name from syscolumns where colid = @field and id = (select id from sysobjects where name = @tablename)
			set @insertquery=
				'insert INTO dbo.history
				(
					ReferenceId,
					TableName,
					FieldName,
					OldValue,
					NewValue,
					UpdatedBy, 
					UpdateDate
				)
				select i.id,
					'''+@tablename+''',
					'''+@fieldName+''',
					d.'+@fieldName+',
					null,
					i.updatedbyId,
					i.updatedat
				from #tempdeleted as d on i.id=d.id
				where d.'+@fieldName+' is not null'
			--print @insertquery --debug
			exec(@insertquery)
		end
	end
end