CREATE TRIGGER [GFHoursTrigger]
	ON [dbo].[GrandfatherHours]
	FOR DELETE, INSERT, UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		if (Select Count(*) From inserted) > 0 and (Select Count(*) From deleted) = 0 --create
		begin
		   insert into dbo.History(FieldName, NewValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'StartDate', i.StartDate, i.ClientId, 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i
			insert into dbo.History(FieldName, NewValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'Hours', i.[Value], i.ClientId, 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i
			insert into dbo.History(FieldName, NewValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'Type', case when i.[Type] = 0 then 'Grandfathered'  when i.[Type] = 2 then 'BMF Approved' else 'Exceptional' end , i.ClientId, 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i
		end

		if (Select Count(*) From inserted) = 0 and (Select Count(*) From deleted) > 0 --delete
		begin
		   insert into dbo.History(FieldName, NewValue, OldValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'StartDate', null, d.StartDate, d.ClientId, 'GrandfatherHours', d.UpdatedAt, d.UpdatedBy from deleted as d
			insert into dbo.History(FieldName, NewValue, OldValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'Hours', null, d.[Value], d.ClientId, 'GrandfatherHours', d.UpdatedAt, d.UpdatedBy from deleted as d
			insert into dbo.History(FieldName, NewValue, OldValue, ReferenceId, TableName, UpdateDate, UpdatedBy)
			select 'Type', null, case when d.[Type] = 0 then 'Grandfathered' when d.[Type] = 2 then 'BMF Approved' else 'Exceptional' end , d.ClientId, 'GrandfatherHours', d.UpdatedAt, d.UpdatedBy from deleted as d
		end

		if (Select Count(*) From inserted) > 0 and (Select Count(*) From deleted) > 0--update
		begin
			if UPDATE(StartDate)
			begin
		    insert into dbo.History(FieldName, NewValue, OldValue,ReferenceId,TableName,UpdateDate,UpdatedBy)
				select 'StartDate', i.StartDate, d.StartDate, coalesce(i.ClientId , d.ClientId), 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i left outer join deleted as d on i.ClientId=d.ClientId
			end
			if UPDATE([Value])
			begin
		    insert into dbo.History(FieldName, NewValue, OldValue,ReferenceId,TableName,UpdateDate,UpdatedBy)
				select 'Hours', i.[Value], d.[Value], coalesce(i.ClientId , d.ClientId), 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i left outer join deleted as d on i.ClientId=d.ClientId
			end
			if UPDATE([Type])
			begin
		    insert into dbo.History(FieldName, NewValue, OldValue,ReferenceId,TableName,UpdateDate,UpdatedBy)
				select 'Type', case when i.[Type] = 0 then 'Grandfathered' when i.[Type] = 2 then 'BMF Approved' else 'Exceptional' end, case when d.[Type] = 0 then 'Grandfathered' when d.[Type] = 2 then 'BMF Approved' else 'Exceptional' end, coalesce(i.ClientId , d.ClientId), 'GrandfatherHours', i.UpdatedAt, i.UpdatedBy from inserted as i left outer join deleted as d on i.ClientId=d.ClientId
			end
		end
	END
