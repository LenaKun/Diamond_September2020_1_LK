CREATE TRIGGER [AppBudgetStatusHistoryTrigger]
	ON [dbo].[appbudgets]
	FOR UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		if UPDATE(statusid)
		begin
			insert into dbo.History(FieldName, NewValue, OldValue,ReferenceId,TableName,UpdateDate,UpdatedBy)
				select 'statusId', i.statusid, d.statusid, coalesce(i.id , d.id), 'AppBudgets', i.UpdatedAt, i.UpdatedById from inserted as i left outer join deleted as d on i.id=d.id

			
			--clean up target table in case some appbudget services were deleted
			delete t
			from approvedappbudgetservices as t
			join inserted as b on t.appbudgetid = b.id
			where b.statusId = 5 /* 5 - approved */

			--insert ccgrants for approved budgets
			insert into ApprovedAppBudgetServices (AppBudgetId, AgencyId, ServiceId, CcGrant, RequiredMatch, AgencyContribution, RecordDate)
			select b.Id as appbudgetid, bs.AgencyId, bs.ServiceId, bs.CcGrant, bs.RequiredMatch, bs.AgencyContribution, GETDATE()
			from inserted as b
			join dbo.AppBudgetServices as bs on b.id = bs.AppBudgetId
			where b.statusId = 5 /* 5 - approved */
				
			
		end

	END
